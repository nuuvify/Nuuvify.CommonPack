<#
.SYNOPSIS
    Audita ou aplica a politica de acesso da organizacao Nuuvify no GitHub.

.DESCRIPTION
    Usa o GitHub CLI para reconciliar owners da organizacao, privilegios dos membros,
    time de colaboradores e permissoes do repositorio. Sem -Apply, nenhuma alteracao
    e executada. O rebaixamento de owners exige confirmacao adicional.

.PARAMETER PolicyPath
    Caminho do arquivo JSON que descreve o estado desejado.

.PARAMETER Apply
    Aplica as correcoes encontradas. Sem este parametro, o script apenas audita.

.PARAMETER ConfirmOwnerDemotion
    Permite rebaixar owners fora da lista protegida para member.

.PARAMETER OwnerDemotionConfirmation
    Para execucao nao interativa, deve conter os logins a rebaixar, separados por
    virgula e em ordem alfabetica.

.PARAMETER ValidateOnly
    Valida somente a estrutura e as invariantes do arquivo de politica, sem exigir gh.

.EXAMPLE
    ./tools/scripts/set-github-access-policy.ps1

.EXAMPLE
    ./tools/scripts/set-github-access-policy.ps1 -Apply

.EXAMPLE
    ./tools/scripts/set-github-access-policy.ps1 -Apply -ConfirmOwnerDemotion
#>

[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'Medium')]
param (
    [string]$PolicyPath = (Join-Path $PSScriptRoot 'github_access_policy.json'),
    [switch]$Apply,
    [switch]$ConfirmOwnerDemotion,
    [string]$OwnerDemotionConfirmation,
    [switch]$ValidateOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:Results = [System.Collections.Generic.List[object]]::new()
$script:HasBlocked = $false
$script:HasDrift = $false

function Add-PolicyResult {
    param (
        [Parameter(Mandatory = $true)]
        [ValidateSet('OK', 'DRIFT', 'CHANGED', 'BLOCKED')]
        [string]$Status,

        [Parameter(Mandatory = $true)]
        [string]$Resource,

        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    $script:Results.Add([pscustomobject]@{
            Status   = $Status
            Resource = $Resource
            Message  = $Message
        })

    if ($Status -eq 'BLOCKED') {
        $script:HasBlocked = $true
    }
    elseif ($Status -eq 'DRIFT') {
        $script:HasDrift = $true
    }
}

function Invoke-Gh {
    param (
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments,

        [string]$Body,
        [switch]$AllowNotFound
    )

    $previousErrorActionPreference = $ErrorActionPreference
    $previousGhDockerTty = $env:GH_DOCKER_TTY
    $hasNativeErrorPreference = $null -ne (Get-Variable -Name PSNativeCommandUseErrorActionPreference -ErrorAction SilentlyContinue)
    if ($hasNativeErrorPreference) {
        $previousNativeErrorPreference = $PSNativeCommandUseErrorActionPreference
    }

    try {
        $ErrorActionPreference = 'Continue'
        $env:GH_DOCKER_TTY = '0'
        if ($hasNativeErrorPreference) {
            $PSNativeCommandUseErrorActionPreference = $false
        }

        if ([string]::IsNullOrWhiteSpace($Body)) {
            $output = ((& gh @Arguments 2>&1) | Out-String).Trim()
        }
        else {
            $output = (($Body | & gh @Arguments --input - 2>&1) | Out-String).Trim()
        }
        $exitCode = $LASTEXITCODE
        $output = [regex]::Replace($output, "`e\][^`a]*(?:`a|`e\\)", '')
        $output = [regex]::Replace($output, "`e\[[0-?]*[ -/]*[@-~]", '')
        $output = $output.Trim()
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
        if ($null -eq $previousGhDockerTty) {
            Remove-Item Env:GH_DOCKER_TTY -ErrorAction SilentlyContinue
        }
        else {
            $env:GH_DOCKER_TTY = $previousGhDockerTty
        }

        if ($hasNativeErrorPreference) {
            $PSNativeCommandUseErrorActionPreference = $previousNativeErrorPreference
        }
    }

    if ($exitCode -ne 0) {
        if ($AllowNotFound -and $output -match '(?i)(HTTP 404|Not Found)') {
            return $null
        }

        throw "Falha ao executar gh $($Arguments -join ' '): $output"
    }

    return $output
}

function Invoke-GhApi {
    param (
        [Parameter(Mandatory = $true)]
        [string]$Endpoint,

        [ValidateSet('GET', 'POST', 'PATCH', 'PUT', 'DELETE')]
        [string]$Method = 'GET',

        [object]$Body,
        [switch]$AllowNotFound
    )

    $arguments = @('api', '--method', $Method, $Endpoint)
    $json = $null
    if ($null -ne $Body) {
        $json = $Body | ConvertTo-Json -Depth 10 -Compress
    }

    $output = Invoke-Gh -Arguments $arguments -Body $json -AllowNotFound:$AllowNotFound
    if ([string]::IsNullOrWhiteSpace($output)) {
        return $null
    }

    return $output | ConvertFrom-Json
}

function Invoke-GhApiList {
    param (
        [Parameter(Mandatory = $true)]
        [string]$Endpoint
    )

    $separator = if ($Endpoint.Contains('?')) { '&' } else { '?' }
    $arguments = @('api', "$Endpoint${separator}per_page=100", '--paginate', '--slurp')
    $output = Invoke-Gh -Arguments $arguments
    if ([string]::IsNullOrWhiteSpace($output)) {
        return @()
    }

    $pages = @($output | ConvertFrom-Json)
    return @($pages | ForEach-Object { @($_) })
}

function Test-SameSet {
    param (
        [string[]]$Actual,
        [string[]]$Expected
    )

    $actualNormalized = @($Actual | ForEach-Object { $_.ToLowerInvariant() } | Sort-Object -Unique)
    $expectedNormalized = @($Expected | ForEach-Object { $_.ToLowerInvariant() } | Sort-Object -Unique)
    return (($actualNormalized -join '|') -eq ($expectedNormalized -join '|'))
}

function Test-ProtectedAdministrator {
    param (
        [Parameter(Mandatory = $true)]
        [string]$Login,

        [Parameter(Mandatory = $true)]
        [string[]]$ProtectedAdministrators
    )

    return $ProtectedAdministrators -contains $Login.ToLowerInvariant()
}

function Test-WritePermission {
    param (
        [Parameter(Mandatory = $true)]
        [object]$Permissions,

        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    $permission = $Permissions.PSObject.Properties[$Name]
    return $null -ne $permission -and $permission.Value -eq 'write'
}

function Invoke-PolicyMutation {
    param (
        [Parameter(Mandatory = $true)]
        [string]$Resource,

        [Parameter(Mandatory = $true)]
        [string]$Description,

        [Parameter(Mandatory = $true)]
        [scriptblock]$Action
    )

    if (-not $Apply) {
        Add-PolicyResult -Status DRIFT -Resource $Resource -Message $Description
        return
    }

    if ($PSCmdlet.ShouldProcess($Resource, $Description)) {
        & $Action
        Add-PolicyResult -Status CHANGED -Resource $Resource -Message $Description
    }
}

function Get-TeamSlug {
    param ([Parameter(Mandatory = $true)][string]$Name)

    return $Name.Trim().ToLowerInvariant() -replace '[^a-z0-9]+', '-'
}

function Assert-Policy {
    param ([Parameter(Mandatory = $true)][object]$Policy)

    if ([string]::IsNullOrWhiteSpace($Policy.organization) -or
        [string]::IsNullOrWhiteSpace($Policy.repository)) {
        throw 'A politica deve informar organization e repository.'
    }

    if (-not (Test-SameSet -Actual $Policy.protectedAdministrators -Expected @('lzocateli', 'eusener'))) {
        throw 'A politica deve proteger exatamente os administradores lzocateli e eusener.'
    }

    if (-not (Test-SameSet -Actual $Policy.collaboratorTeam.maintainers -Expected $Policy.protectedAdministrators)) {
        throw 'Os maintainers do time devem ser exatamente os administradores protegidos.'
    }

    if ($Policy.collaboratorTeam.repositoryPermission -ne 'push') {
        throw "O time de colaboradores deve usar a permissao 'push', correspondente a Write."
    }

    $teamSlug = Get-TeamSlug -Name $Policy.collaboratorTeam.name
    $allowedTeamSlugs = @($Policy.allowedRepositoryTeams | ForEach-Object { Get-TeamSlug -Name $_ })
    if ($allowedTeamSlugs -notcontains $teamSlug) {
        throw 'O time de colaboradores deve constar em allowedRepositoryTeams.'
    }
}

function Assert-Prerequisites {
    param ([Parameter(Mandatory = $true)][object]$Policy)

    if ($PSVersionTable.PSVersion.Major -lt 7) {
        throw 'PowerShell 7 ou superior e obrigatorio.'
    }

    if ($null -eq (Get-Command gh -ErrorAction SilentlyContinue)) {
        throw 'GitHub CLI (gh) nao foi encontrado no PATH.'
    }

    Invoke-Gh -Arguments @('auth', 'status', '--hostname', 'github.com') | Out-Null
    $authenticatedLogin = [string](Invoke-Gh -Arguments @('api', 'user', '--jq', '.login'))
    $authenticatedLogin = $authenticatedLogin.Trim().ToLowerInvariant()
    [string[]]$protectedAdministrators = @(
        $Policy.protectedAdministrators | ForEach-Object { ([string]$_).Trim().ToLowerInvariant() }
    )

    Write-Host "Conta autenticada: $authenticatedLogin"
    Write-Host "Organizacao: $($Policy.organization)"
    Write-Host "Repositorio: $($Policy.organization)/$($Policy.repository)"
    Write-Host "Administradores protegidos: $($protectedAdministrators -join ', ')"

    $isProtectedAdministrator = $false
    foreach ($protectedAdministrator in $protectedAdministrators) {
        if ([string]::Equals(
                $protectedAdministrator,
                $authenticatedLogin,
                [System.StringComparison]::OrdinalIgnoreCase)) {
            $isProtectedAdministrator = $true
            break
        }
    }

    if (-not $isProtectedAdministrator) {
        throw "A conta autenticada '$authenticatedLogin' nao pertence a lista de administradores protegidos."
    }

    $membership = Invoke-GhApi -Endpoint "orgs/$($Policy.organization)/memberships/$authenticatedLogin"
    if ($membership.state -ne 'active' -or $membership.role -ne 'admin') {
        throw "A conta '$authenticatedLogin' deve ser owner ativo da organizacao '$($Policy.organization)'."
    }

    $permission = Invoke-GhApi -Endpoint "repos/$($Policy.organization)/$($Policy.repository)/collaborators/$authenticatedLogin/permission"
    if ($permission.permission -ne 'admin') {
        throw "A conta '$authenticatedLogin' deve ter Admin no repositorio '$($Policy.repository)'."
    }

    Add-PolicyResult -Status OK -Resource 'authentication' -Message "Autenticado como owner protegido '$authenticatedLogin'."
}

function Sync-OrganizationSettings {
    param ([Parameter(Mandatory = $true)][object]$Policy)

    $organization = Invoke-GhApi -Endpoint "orgs/$($Policy.organization)"
    $desired = $Policy.organizationSettings
    $drift =
    $organization.default_repository_permission -ne $desired.defaultRepositoryPermission -or
    $organization.members_can_create_repositories -ne $desired.membersCanCreateRepositories -or
    $organization.members_can_create_public_repositories -ne $desired.membersCanCreatePublicRepositories -or
    $organization.members_can_create_private_repositories -ne $desired.membersCanCreatePrivateRepositories -or
    $organization.members_can_create_teams -ne $desired.membersCanCreateTeams

    if (-not $drift) {
        Add-PolicyResult -Status OK -Resource 'organization settings' -Message 'Privilegios gerais dos membros estao restritos.'
        return
    }

    $body = @{
        default_repository_permission           = $desired.defaultRepositoryPermission
        members_can_create_repositories         = $desired.membersCanCreateRepositories
        members_can_create_public_repositories  = $desired.membersCanCreatePublicRepositories
        members_can_create_private_repositories = $desired.membersCanCreatePrivateRepositories
        members_can_create_teams                = $desired.membersCanCreateTeams
    }
    if ($organization.plan.name -eq 'enterprise') {
        $body.members_can_create_internal_repositories = $desired.membersCanCreateInternalRepositories
    }

    Invoke-PolicyMutation -Resource "organization/$($Policy.organization)" -Description 'Restringir privilegios gerais dos membros.' -Action {
        Invoke-GhApi -Endpoint "orgs/$($Policy.organization)" -Method PATCH -Body $body | Out-Null
    }
}

function Sync-OrganizationOwners {
    param ([Parameter(Mandatory = $true)][object]$Policy)

    $protected = @($Policy.protectedAdministrators | ForEach-Object { $_.ToLowerInvariant() })
    $owners = @(Invoke-GhApiList -Endpoint "orgs/$($Policy.organization)/members?role=admin")
    $ownerLogins = @($owners.login | ForEach-Object { $_.ToLowerInvariant() })

    foreach ($login in $protected) {
        if ($ownerLogins -contains $login) {
            Add-PolicyResult -Status OK -Resource "owner/$login" -Message 'Owner protegido esta ativo.'
            continue
        }

        Invoke-PolicyMutation -Resource "owner/$login" -Description 'Promover usuario protegido para owner.' -Action {
            Invoke-GhApi -Endpoint "orgs/$($Policy.organization)/memberships/$login" -Method PUT -Body @{ role = 'admin' } | Out-Null
        }
    }

    $extraOwners = @($ownerLogins | Where-Object { $protected -notcontains $_ } | Sort-Object)
    if ($extraOwners.Count -eq 0) {
        return
    }

    $expectedConfirmation = $extraOwners -join ','
    $confirmation = $OwnerDemotionConfirmation
    if ($Apply -and $ConfirmOwnerDemotion -and [string]::IsNullOrWhiteSpace($confirmation)) {
        $confirmation = Read-Host "Confirme os owners que serao rebaixados digitando: $expectedConfirmation"
    }

    foreach ($login in $extraOwners) {
        if (-not $Apply) {
            Add-PolicyResult -Status DRIFT -Resource "owner/$login" -Message 'Owner fora da lista protegida deve ser rebaixado para member.'
            continue
        }

        if (-not $ConfirmOwnerDemotion) {
            Add-PolicyResult -Status BLOCKED -Resource "owner/$login" -Message 'Use -ConfirmOwnerDemotion para autorizar o rebaixamento.'
            continue
        }

        if ($confirmation.Trim().ToLowerInvariant() -ne $expectedConfirmation) {
            Add-PolicyResult -Status BLOCKED -Resource "owner/$login" -Message 'Confirmacao nominal dos owners nao corresponde ao esperado.'
            continue
        }

        Invoke-PolicyMutation -Resource "owner/$login" -Description 'Rebaixar owner nao autorizado para member.' -Action {
            Invoke-GhApi -Endpoint "orgs/$($Policy.organization)/memberships/$login" -Method PUT -Body @{ role = 'member' } | Out-Null
        }
    }
}

function Sync-CollaboratorTeam {
    param ([Parameter(Mandatory = $true)][object]$Policy)

    $team = $Policy.collaboratorTeam
    $teamSlug = Get-TeamSlug -Name $team.name
    $currentTeam = Invoke-GhApi -Endpoint "orgs/$($Policy.organization)/teams/$teamSlug" -AllowNotFound

    if ($null -eq $currentTeam) {
        Invoke-PolicyMutation -Resource "team/$teamSlug" -Description 'Criar time de colaboradores.' -Action {
            Invoke-GhApi -Endpoint "orgs/$($Policy.organization)/teams" -Method POST -Body @{
                name                 = $team.name
                description          = $team.description
                privacy              = $team.privacy
                notification_setting = $team.notificationSetting
            } | Out-Null
        }
        if (-not $Apply) {
            return
        }
        $currentTeam = Invoke-GhApi -Endpoint "orgs/$($Policy.organization)/teams/$teamSlug"
    }

    $teamDrift =
    $currentTeam.description -ne $team.description -or
    $currentTeam.privacy -ne $team.privacy -or
    $currentTeam.notification_setting -ne $team.notificationSetting
    if ($teamDrift) {
        Invoke-PolicyMutation -Resource "team/$teamSlug" -Description 'Atualizar configuracao do time de colaboradores.' -Action {
            Invoke-GhApi -Endpoint "orgs/$($Policy.organization)/teams/$teamSlug" -Method PATCH -Body @{
                name                 = $team.name
                description          = $team.description
                privacy              = $team.privacy
                notification_setting = $team.notificationSetting
            } | Out-Null
        }
    }
    else {
        Add-PolicyResult -Status OK -Resource "team/$teamSlug" -Message 'Configuracao do time esta correta.'
    }

    $desiredMaintainers = @($team.maintainers | ForEach-Object { $_.ToLowerInvariant() })
    $desiredMembers = @($team.members | ForEach-Object { $_.ToLowerInvariant() })
    $desiredUsers = @($desiredMaintainers + $desiredMembers | Sort-Object -Unique)
    $currentMembers = @(Invoke-GhApiList -Endpoint "orgs/$($Policy.organization)/teams/$teamSlug/members")
    $currentLogins = @($currentMembers.login | ForEach-Object { $_.ToLowerInvariant() })

    foreach ($login in $desiredUsers) {
        $desiredRole = if ($desiredMaintainers -contains $login) { 'maintainer' } else { 'member' }
        $membership = Invoke-GhApi -Endpoint "orgs/$($Policy.organization)/teams/$teamSlug/memberships/$login" -AllowNotFound
        if ($null -ne $membership -and $membership.role -eq $desiredRole) {
            Add-PolicyResult -Status OK -Resource "team-member/$login" -Message "Papel '$desiredRole' esta correto."
            continue
        }

        Invoke-PolicyMutation -Resource "team-member/$login" -Description "Definir papel '$desiredRole' no time." -Action {
            Invoke-GhApi -Endpoint "orgs/$($Policy.organization)/teams/$teamSlug/memberships/$login" -Method PUT -Body @{ role = $desiredRole } | Out-Null
        }
    }

    foreach ($login in @($currentLogins | Where-Object { $desiredUsers -notcontains $_ })) {
        Invoke-PolicyMutation -Resource "team-member/$login" -Description 'Remover usuario nao declarado do time.' -Action {
            Invoke-GhApi -Endpoint "orgs/$($Policy.organization)/teams/$teamSlug/memberships/$login" -Method DELETE | Out-Null
        }
    }

    $repositoryPermission = Invoke-GhApi -Endpoint "orgs/$($Policy.organization)/teams/$teamSlug/repos/$($Policy.organization)/$($Policy.repository)" -AllowNotFound
    if ($null -ne $repositoryPermission -and $repositoryPermission.role_name -eq 'write') {
        Add-PolicyResult -Status OK -Resource "team-repository/$teamSlug" -Message 'Time possui Write no repositorio.'
    }
    else {
        Invoke-PolicyMutation -Resource "team-repository/$teamSlug" -Description 'Conceder Write ao time no repositorio.' -Action {
            Invoke-GhApi -Endpoint "orgs/$($Policy.organization)/teams/$teamSlug/repos/$($Policy.organization)/$($Policy.repository)" -Method PUT -Body @{ permission = $team.repositoryPermission } | Out-Null
        }
    }
}

function Sync-RepositoryAccess {
    param ([Parameter(Mandatory = $true)][object]$Policy)

    $protected = @($Policy.protectedAdministrators | ForEach-Object { $_.ToLowerInvariant() })
    $allowedTeams = @($Policy.allowedRepositoryTeams | ForEach-Object { Get-TeamSlug -Name $_ })
    $teams = @(Invoke-GhApiList -Endpoint "repos/$($Policy.organization)/$($Policy.repository)/teams")
    foreach ($team in $teams) {
        if ($allowedTeams -contains $team.slug.ToLowerInvariant()) {
            continue
        }

        Invoke-PolicyMutation -Resource "repository-team/$($team.slug)" -Description 'Remover time nao autorizado do repositorio.' -Action {
            Invoke-GhApi -Endpoint "orgs/$($Policy.organization)/teams/$($team.slug)/repos/$($Policy.organization)/$($Policy.repository)" -Method DELETE | Out-Null
        }
    }

    $directCollaborators = @(Invoke-GhApiList -Endpoint "repos/$($Policy.organization)/$($Policy.repository)/collaborators?affiliation=direct")
    foreach ($collaborator in $directCollaborators) {
        $login = $collaborator.login.ToLowerInvariant()
        if (Test-ProtectedAdministrator -Login $login -ProtectedAdministrators $protected) {
            continue
        }

        Invoke-PolicyMutation -Resource "repository-collaborator/$login" -Description 'Remover grant direto; acesso autorizado deve vir do time.' -Action {
            Invoke-GhApi -Endpoint "repos/$($Policy.organization)/$($Policy.repository)/collaborators/$login" -Method DELETE | Out-Null
        }
    }

    $administrators = @(Invoke-GhApiList -Endpoint "repos/$($Policy.organization)/$($Policy.repository)/collaborators?permission=admin")
    foreach ($administrator in $administrators) {
        $login = $administrator.login.ToLowerInvariant()
        if (-not (Test-ProtectedAdministrator -Login $login -ProtectedAdministrators $protected)) {
            Add-PolicyResult -Status BLOCKED -Resource "repository-admin/$login" -Message 'Usuario ainda possui Admin por fonte efetiva; revisar time, Enterprise ou role customizada.'
        }
    }
}

function Audit-AdministrativeIntegrations {
    param ([Parameter(Mandatory = $true)][object]$Policy)

    try {
        $installationsResponse = Invoke-GhApi -Endpoint "orgs/$($Policy.organization)/installations"
        foreach ($installation in @($installationsResponse.installations)) {
            $hasAdministrativeAccess =
            (Test-WritePermission -Permissions $installation.permissions -Name 'administration') -or
            (Test-WritePermission -Permissions $installation.permissions -Name 'organization_administration') -or
            (Test-WritePermission -Permissions $installation.permissions -Name 'members')
            if ($hasAdministrativeAccess) {
                Add-PolicyResult -Status BLOCKED -Resource "github-app/$($installation.app_slug)" -Message 'GitHub App possui permissao administrativa; revisar manualmente.'
            }
        }
    }
    catch {
        Add-PolicyResult -Status BLOCKED -Resource 'github-apps' -Message "Nao foi possivel auditar instalacoes: $($_.Exception.Message)"
    }
}

function Write-PolicyReport {
    $script:Results | Format-Table -AutoSize -Wrap

    $summary = $script:Results | Group-Object Status | Sort-Object Name
    Write-Host ''
    Write-Host 'Resumo:'
    foreach ($item in $summary) {
        Write-Host "  $($item.Name): $($item.Count)"
    }
}

if (-not (Test-Path -LiteralPath $PolicyPath -PathType Leaf)) {
    throw "Arquivo de politica nao encontrado: $PolicyPath"
}

$policy = Get-Content -LiteralPath $PolicyPath -Raw | ConvertFrom-Json
$policy.protectedAdministrators = [string[]]@(
    $policy.protectedAdministrators | ForEach-Object { ([string]$_).Trim().ToLowerInvariant() }
)
Assert-Policy -Policy $policy

if ($ValidateOnly) {
    Write-Host "Politica valida: $PolicyPath"
    exit 0
}

Assert-Prerequisites -Policy $policy
Sync-OrganizationSettings -Policy $policy
Sync-OrganizationOwners -Policy $policy
Sync-CollaboratorTeam -Policy $policy
Sync-RepositoryAccess -Policy $policy
Audit-AdministrativeIntegrations -Policy $policy
Write-PolicyReport

if ($script:HasBlocked) {
    exit 2
}

if ($script:HasDrift -and -not $Apply) {
    exit 1
}

exit 0
