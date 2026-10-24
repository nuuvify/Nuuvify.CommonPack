# Gerenciado pelo Altivy.Groundwork (componente release). Não edite: mudanças vão para o catálogo.
# Funções compartilhadas pelos scripts de release, publicação e deploy em tools/scripts/ci.
#Requires -Version 7.0
Set-StrictMode -Version Latest

$script:SemVerPattern = '^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(-(?<pre>[0-9A-Za-z-]+(\.[0-9A-Za-z-]+)*))?$'
$script:ManifestKinds = @('nuget', 'container', 'asset')

function Test-SemVer {
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Version)
    return $Version -match $script:SemVerPattern
}

function Test-ReleaseTagName {
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Tag)
    return $Tag.StartsWith('v', [System.StringComparison]::Ordinal) -and (Test-SemVer $Tag.Substring(1))
}

function Compare-SemVer {
    <# Retorna -1, 0 ou 1 conforme a precedência SemVer 2.0.0 (metadados de build não são aceitos). #>
    [OutputType([int])]
    param(
        [Parameter(Mandatory)][string] $Left,
        [Parameter(Mandatory)][string] $Right
    )

    $a = [regex]::Match($Left, $script:SemVerPattern)
    $b = [regex]::Match($Right, $script:SemVerPattern)
    if (-not $a.Success -or -not $b.Success) {
        throw "Versão SemVer inválida: '$Left' ou '$Right'."
    }

    foreach ($part in 'major', 'minor', 'patch') {
        $comparison = ([bigint]$a.Groups[$part].Value).CompareTo([bigint]$b.Groups[$part].Value)
        if ($comparison -ne 0) { return [Math]::Sign($comparison) }
    }

    $preA = $a.Groups['pre'].Success ? $a.Groups['pre'].Value : ''
    $preB = $b.Groups['pre'].Success ? $b.Groups['pre'].Value : ''
    if ($preA -eq $preB) { return 0 }
    if (-not $preA) { return 1 }
    if (-not $preB) { return -1 }

    $idsA = $preA.Split('.')
    $idsB = $preB.Split('.')
    for ($i = 0; $i -lt [Math]::Min($idsA.Count, $idsB.Count); $i++) {
        $numA = $idsA[$i] -match '^\d+$'
        $numB = $idsB[$i] -match '^\d+$'
        if ($numA -and $numB) {
            $comparison = ([bigint]$idsA[$i]).CompareTo([bigint]$idsB[$i])
        }
        elseif ($numA) { $comparison = -1 }
        elseif ($numB) { $comparison = 1 }
        else { $comparison = [string]::CompareOrdinal($idsA[$i], $idsB[$i]) }

        if ($comparison -ne 0) { return [Math]::Sign($comparison) }
    }

    return [Math]::Sign($idsA.Count.CompareTo($idsB.Count))
}

function Read-CiCdConfig {
    param([string] $Path = '.github/ci-cd.json')
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Configuração do pipeline ausente: $Path."
    }

    try {
        return Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json -AsHashtable
    }
    catch {
        throw "Configuração do pipeline inválida: $Path."
    }
}

function Get-AttestationPolicy {
    <#
    Lê "attestations" de .github/ci-cd.json. Ausente significa true (padrão seguro); somente o booleano
    false desativa a atestação. Qualquer outro valor é erro, para não haver interpretação ambígua.
    #>
    [OutputType([bool])]
    param([Parameter(Mandatory)][hashtable] $Config)
    if (-not $Config.ContainsKey('attestations')) { return $true }
    $value = $Config['attestations']
    if ($value -isnot [bool]) { throw 'attestations em .github/ci-cd.json deve ser o booleano true ou false.' }
    return $value
}

function Get-VersionFromFile {
    <# Lê <VersionPrefix> de .props/.csproj/.targets ou $.version de .json. #>
    [OutputType([string])]
    param([Parameter(Mandatory)][string] $Path)
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Arquivo de versão ausente: $Path."
    }

    $content = Get-Content -LiteralPath $Path -Raw
    if ($Path -match '\.json$') {
        $version = [string]((ConvertFrom-Json -InputObject $content -AsHashtable)['version'] ?? '')
    }
    else {
        $match = [regex]::Match($content, '<VersionPrefix>\s*(?<v>[^<\s]+)\s*</VersionPrefix>')
        $version = $match.Success ? $match.Groups['v'].Value : ''
    }

    if (-not (Test-SemVer $version)) {
        throw "Versão ausente ou fora de SemVer em ${Path}: '$version'."
    }

    return $version
}

function Get-ProjectVersionFiles {
    [OutputType([string[]])]
    param([Parameter(Mandatory)][hashtable] $Config)
    $files = @($Config['versionFiles'] ?? @() | Where-Object { $_ })
    if ($files.Count -eq 0) {
        throw 'versionFiles vazio em .github/ci-cd.json.'
    }

    foreach ($file in $files) {
        if ([System.IO.Path]::IsPathRooted($file) -or $file -match '(^|[\\/])\.\.([\\/]|$)') {
            throw "Arquivo de versão fora do repositório: $file."
        }
    }

    return $files
}

function Invoke-GhJson {
    <#
    Executa 'gh api' e devolve o JSON convertido; retorna $null para HTTP 404 quando -AllowNotFound.
    Nunca repassa a saída de erro do gh, que pode conter dados do ambiente.
    #>
    param(
        [Parameter(Mandatory)][string] $Path,
        [string] $Method = 'GET',
        [hashtable] $Body = $null,
        [switch] $AllowNotFound,
        [switch] $Paginate
    )

    $arguments = @('api', '--method', $Method, '-H', 'Accept: application/vnd.github+json', '-H', 'X-GitHub-Api-Version: 2022-11-28')
    if ($Paginate) { $arguments += @('--paginate', '--slurp') }
    if ($null -ne $Body) { $arguments += @('--input', '-') }
    $arguments += $Path
    $errorFile = New-TemporaryFile
    try {
        if ($null -ne $Body) {
            $output = ConvertTo-Json -InputObject $Body -Depth 32 -Compress | & gh @arguments 2> $errorFile.FullName
        }
        else {
            $output = & gh @arguments 2> $errorFile.FullName
        }

        $exitCode = $LASTEXITCODE
        if ($exitCode -ne 0) {
            if ($AllowNotFound -and (Get-Content -LiteralPath $errorFile.FullName -Raw) -match 'HTTP 404') {
                return $null
            }

            throw "GitHub API falhou ($Method $Path, código $exitCode)."
        }
    }
    finally {
        Remove-Item -LiteralPath $errorFile.FullName -Force -ErrorAction SilentlyContinue
    }

    $text = ($output | Out-String).Trim()
    if (-not $text) { return $null }
    $result = ConvertFrom-Json -InputObject $text -AsHashtable -Depth 64
    if ($Paginate) {
        # --slurp devolve uma lista de páginas; listas paginadas são achatadas em um único array.
        return , @($result | ForEach-Object { $_ })
    }

    return $result
}

function Assert-ReleaseManifest {
    <# Valida o schema do release-manifest.json e, opcionalmente, a tag e o commit esperados. #>
    param(
        [Parameter(Mandatory)][hashtable] $Manifest,
        [string] $ExpectedTag = '',
        [string] $ExpectedCommit = ''
    )

    if ($Manifest['schemaVersion'] -ne 1) { throw 'release-manifest.json com schemaVersion não suportado.' }
    foreach ($field in 'repository', 'version', 'tag', 'commit', 'rollbackMinimumVersion') {
        if (-not ($Manifest[$field] -is [string]) -or -not $Manifest[$field]) {
            throw "release-manifest.json sem o campo $field."
        }
    }

    if (-not (Test-SemVer $Manifest['version']) -or $Manifest['tag'] -ne "v$($Manifest['version'])") {
        throw 'release-manifest.json com versão ou tag inconsistentes.'
    }

    if (-not (Test-SemVer $Manifest['rollbackMinimumVersion'])) {
        throw 'release-manifest.json com rollbackMinimumVersion fora de SemVer.'
    }

    if ($Manifest['commit'] -notmatch '^[0-9a-f]{40}$') { throw 'release-manifest.json com commit inválido.' }
    if ($Manifest.ContainsKey('attestation') -and $Manifest['attestation'] -notin @('github', 'none')) {
        throw 'release-manifest.json com attestation inválido; use github ou none.'
    }
    if ($ExpectedTag -and $Manifest['tag'] -ne $ExpectedTag) { throw "release-manifest.json pertence a $($Manifest['tag']), não a $ExpectedTag." }
    if ($ExpectedCommit -and $Manifest['commit'] -ne $ExpectedCommit) { throw 'release-manifest.json aponta para outro commit.' }

    $artifacts = @($Manifest['artifacts'] ?? @())
    if ($artifacts.Count -eq 0) { throw 'release-manifest.json sem artefatos.' }
    Assert-ReleaseEntries -Entries $artifacts
}

function Assert-ReleaseEntries {
    param([Parameter(Mandatory)][AllowEmptyCollection()][object[]] $Entries)
    $keys = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    foreach ($entry in $Entries) {
        if ($entry -isnot [System.Collections.IDictionary]) { throw 'Entrada de artefato inválida.' }
        if ($entry['kind'] -notin $script:ManifestKinds) { throw "Tipo de artefato inválido: $($entry['kind'])." }
        foreach ($field in 'name', 'reference') {
            if (-not ($entry[$field] -is [string]) -or -not $entry[$field] -or $entry[$field] -match '[\x00-\x1f]') {
                throw "Artefato sem $field válido."
            }
        }

        if ($entry['digest'] -notmatch '^sha256:[0-9a-f]{64}$') { throw "Digest inválido para $($entry['name'])." }
        if (-not $keys.Add("$($entry['kind'])/$($entry['name'])")) { throw "Artefato duplicado: $($entry['kind'])/$($entry['name'])." }
    }
}

function Test-RollbackAllowed {
    <#
    Decide a trava de rollback: alvo abaixo da versão implantada só é aceito se não for menor
    que rollbackMinimumVersion do manifesto da versão implantada. Não há bypass.
    #>
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory)][string] $Target,
        [AllowEmptyString()][string] $Current = '',
        [AllowEmptyString()][string] $CurrentMinimum = ''
    )

    if (-not $Current -or (Compare-SemVer $Target $Current) -ge 0) {
        return @{ Allowed = $true; Rollback = $false; Reason = '' }
    }

    if (-not $CurrentMinimum) {
        return @{ Allowed = $false; Rollback = $true; Reason = "Manifesto da versão implantada $Current sem rollbackMinimumVersion; rollback recusado." }
    }

    if ((Compare-SemVer $Target $CurrentMinimum) -lt 0) {
        return @{
            Allowed  = $false
            Rollback = $true
            Reason   = "Rollback de $Current para $Target recusado: rollbackMinimumVersion de $Current é $CurrentMinimum. Faça roll-forward com nova release."
        }
    }

    return @{ Allowed = $true; Rollback = $true; Reason = '' }
}

function Find-GitHubRelease {
    <# Localiza a release pela tag, inclusive rascunhos (releases/tags/{tag} não retorna rascunhos). #>
    param(
        [Parameter(Mandatory)][string] $Repository,
        [Parameter(Mandatory)][string] $Tag
    )

    $releases = Invoke-GhJson -Path "repos/$Repository/releases?per_page=100" -Paginate
    return @($releases ?? @()) | Where-Object { $_['tag_name'] -eq $Tag } | Select-Object -First 1
}

function Save-ReleaseAsset {
    <# Baixa o asset preservando os bytes (PowerShell 7.4+ não recodifica redirecionamento nativo para arquivo). #>
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)][string] $Repository,
        [Parameter(Mandatory)][hashtable] $Release,
        [Parameter(Mandatory)][string] $Name,
        [Parameter(Mandatory)][string] $Destination
    )

    $asset = @($Release['assets'] ?? @()) | Where-Object { $_['name'] -eq $Name } | Select-Object -First 1
    if ($null -eq $asset) { return $false }
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent ([System.IO.Path]::GetFullPath($Destination))) | Out-Null
    & gh api -H 'Accept: application/octet-stream' "repos/$Repository/releases/assets/$($asset['id'])" > $Destination
    if ($LASTEXITCODE -ne 0) { throw "Falha ao baixar o asset $Name de $($Release['tag_name'])." }
    return $true
}

function Get-ReleaseAssetText {
    param(
        [Parameter(Mandatory)][string] $Repository,
        [Parameter(Mandatory)][hashtable] $Release,
        [Parameter(Mandatory)][string] $Name
    )

    $temporary = New-TemporaryFile
    try {
        if (-not (Save-ReleaseAsset -Repository $Repository -Release $Release -Name $Name -Destination $temporary.FullName)) { return $null }
        return [System.IO.File]::ReadAllText($temporary.FullName)
    }
    finally {
        Remove-Item -LiteralPath $temporary.FullName -Force -ErrorAction SilentlyContinue
    }
}

function Write-GitHubOutput {
    param(
        [Parameter(Mandatory)][hashtable] $Values,
        [string] $Path = ''
    )

    foreach ($key in @($Values.Keys | Sort-Object)) {
        $value = [string]$Values[$key]
        if ($value -match '[\r\n]') { throw "Saída $key contém quebra de linha." }
        if ($Path) { "$key=$value" | Add-Content -LiteralPath $Path -Encoding utf8 }
        Write-Output "$key=$value"
    }
}

Export-ModuleMember -Function Test-SemVer, Test-ReleaseTagName, Compare-SemVer, Read-CiCdConfig, Get-AttestationPolicy, Get-VersionFromFile,
Get-ProjectVersionFiles, Invoke-GhJson, Assert-ReleaseManifest, Assert-ReleaseEntries, Test-RollbackAllowed,
Find-GitHubRelease, Save-ReleaseAsset, Get-ReleaseAssetText, Write-GitHubOutput
