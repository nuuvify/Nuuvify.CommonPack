#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Gera (e opcionalmente publica) os pacotes NuGet da solucao local, para testes manuais.

.DESCRIPTION
    Script de uso local que:
      1. Deduz o nome da solucao a partir do nome da pasta raiz do repositorio.
      2. Le a versao base do pacote em src/Directory.Build.props (tag <Version>).
      3. Quando -Environment for diferente de PRD, monta a versao como
         "<version>-dev.<BuildNumber>". Quando for PRD, usa a versao exata.
      4. Valida que todo projeto empacotavel em src/ possui o arquivo
         "<Projeto>.targets" exigido pelo Directory.Build.props para o pack funcionar.
      5. Limpa bin/obj e artefatos previos da mesma versao, e executa "dotnet pack"
         gerando os .nupkg/.snupkg em c:\software\<SolutionName>.
      6. Opcionalmente publica os pacotes gerados (-Publish) em um dos feeds
            suportados (-Feed), validando antes se as credenciais necessarias do
            feed estao definidas.

     Regras de execucao:
        - Sem parametros: executa pack.
        - Com -Pack: executa pack.
        - Com -Publish: publica pacotes ja existentes na pasta de saida (sem pack).
        - Com -Pack -Publish: executa pack e depois publica.

    Use -DryRun para simular todas as etapas (limpeza, pack e publish) sem executar
    nenhum comando destrutivo, "dotnet pack" ou "dotnet nuget push".

.PARAMETER Environment
    Ambiente alvo da build. Valores aceitos: DEV, QAS, PRD.
    Diferente de PRD  -> versao "<version>-dev.<BuildNumber>".
    PRD               -> versao exata definida em Directory.Build.props.

.PARAMETER BuildNumber
    Numero usado no sufixo "-dev.<BuildNumber>" quando -Environment for diferente de PRD.
    Se omitido, usa $env:BUILD_BUILDID (Azure DevOps) ou um timestamp "yyyyMMddHHmm".

.PARAMETER Configuration
    Configuracao de build usada no "dotnet pack". Padrao: Release.

.PARAMETER Pack
    Quando presente, executa a etapa de empacotamento (dotnet pack).
    Combinado com -Publish, realiza pack e depois publicacao.

.PARAMETER Publish
    Quando presente, publica no feed indicado por -Feed os pacotes .nupkg ja
    existentes em c:\software\<SolutionName>. Nao executa pack automaticamente.

.PARAMETER PublishCurrentVersionOnly
    Quando presente junto com -Publish, publica somente os pacotes da versao alvo
    calculada para a execucao atual (respeitando -Environment e -BuildNumber).
    Evita publicar pacotes antigos que ja estejam na pasta de saida.

.PARAMETER Feed
    Feed de destino da publicacao (relevante somente com -Publish). Valores aceitos:
      Local       -> pasta local  c:\software\nugetfeed                    (sem credenciais)
      NugetTest   -> https://apiint.nugettest.org/v3/index.json            (env:NUGETTEST_API_KEY, via --api-key)
      Nuget       -> https://api.nuget.org/v3/index.json                  (env:NUGET_API_KEY, via --api-key)
      Artifactory -> https://artifacts.cat.com/artifactory/api/nuget/v3/cat-brazil-jfrog-nuget
                     (autenticacao basica: env:ARTIFACTORY_USERNAME + env:ARTIFACTORY_PASSWORD como senha/token;
                     o script registra/atualiza localmente a fonte NuGet "Artifactory" antes de publicar)

.PARAMETER DryRun
    Simula as acoes do script (limpeza, pack, publish) sem executar nada destrutivo,
    sem chamar "dotnet pack" e sem chamar "dotnet nuget push". Apenas exibe o que seria feito.

.PARAMETER Help
    Exibe esta ajuda e encerra. Equivalente a passar --help, -h ou /? como argumento.

.EXAMPLE
    ./CreatePack.ps1
    Gera os pacotes localmente com versao de DEV (sufixo "-dev.<timestamp>").

.EXAMPLE
    ./CreatePack.ps1 -Environment PRD -Publish -Feed Nuget
    Publica no NuGet.org os pacotes ja existentes em c:\software\<SolutionName>
    (requer $env:NUGET_API_KEY definido).

.EXAMPLE
    ./CreatePack.ps1 -Environment PRD -Pack -Publish -Feed Nuget
    Gera os pacotes com a versao exata definida em Directory.Build.props e publica
    no NuGet.org (requer $env:NUGET_API_KEY definido).

.EXAMPLE
    ./CreatePack.ps1 -Publish -Feed Artifactory -PublishCurrentVersionOnly
    Publica apenas os pacotes da versao alvo da execucao atual no Artifactory,
    ignorando pacotes de outras versoes presentes em c:\software\<SolutionName>.

.EXAMPLE
    ./CreatePack.ps1 -DryRun -Publish -Feed Artifactory
    Simula pack + publish no Artifactory, sem executar nenhum comando real.

.EXAMPLE
    ./CreatePack.ps1 --help
    Exibe esta ajuda (equivalente a -Help).

.NOTES
    Uso local apenas. Nao substitui a pipeline oficial de release/publish.
#>
[CmdletBinding(PositionalBinding = $false)]
param(
    [ValidateSet('DEV', 'QAS', 'PRD')]
    [string]$Environment = 'DEV',

    [string]$BuildNumber,

    [string]$Configuration = 'Release',

    [switch]$Pack,

    [switch]$Publish,

    [switch]$PublishCurrentVersionOnly,

    [ValidateSet('Local', 'NugetTest', 'Nuget', 'Artifactory')]
    [string]$Feed = 'Local',

    [switch]$DryRun,

    [Alias('h')]
    [switch]$Help,

    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$RemainingArgs
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ($Help -or ($RemainingArgs | Where-Object { $_ -in @('--help', '-h', '/?', 'help') })) {
    Get-Help -Full $PSCommandPath
    exit 0
}

function Get-RepoRoot {
    param([Parameter(Mandatory)][string]$ScriptRoot)
    (Get-Item -Path $ScriptRoot).Parent.Parent.FullName
}

function Get-SolutionName {
    param([Parameter(Mandatory)][string]$RepoRoot)
    Split-Path -Leaf $RepoRoot
}

function Get-PackageBaseVersion {
    param([Parameter(Mandatory)][string]$RepoRoot)

    $propsPath = Join-Path $RepoRoot 'src\Directory.Build.props'
    if (-not (Test-Path $propsPath)) {
        throw "Arquivo nao encontrado: $propsPath"
    }

    [xml]$xml = Get-Content -Raw -Path $propsPath
    $versionNode = $xml.Project.PropertyGroup | Where-Object { $_.Version } | Select-Object -First 1
    if (-not $versionNode -or [string]::IsNullOrWhiteSpace($versionNode.Version)) {
        throw "Tag <Version> nao encontrada em $propsPath"
    }

    return $versionNode.Version.Trim()
}

function Get-PackageVersion {
    param(
        [Parameter(Mandatory)][string]$BaseVersion,
        [Parameter(Mandatory)][string]$Environment,
        [string]$BuildNumber
    )

    if ($Environment -eq 'PRD') {
        return $BaseVersion
    }

    if ([string]::IsNullOrWhiteSpace($BuildNumber)) {
        $BuildNumber = if ($env:BUILD_BUILDID) { $env:BUILD_BUILDID } else { Get-Date -Format 'yyyyMMddHHmm' }
    }

    return "$BaseVersion-dev.$BuildNumber"
}

function Get-LatestPackageVersionFromOutput {
    param(
        [Parameter(Mandatory)][string]$OutputPath,
        [Parameter(Mandatory)][string]$BaseVersion,
        [Parameter(Mandatory)][ValidateSet('DEV', 'QAS', 'PRD')][string]$Environment
    )

    if (-not (Test-Path $OutputPath)) {
        return $null
    }

    $pattern = if ($Environment -eq 'PRD') {
        "*.$BaseVersion.nupkg"
    }
    else {
        "*.$BaseVersion-dev.*.nupkg"
    }

    $latest = Get-ChildItem -Path $OutputPath -Filter '*.nupkg' -File -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -like $pattern } |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if (-not $latest) {
        return $null
    }

    if ($Environment -eq 'PRD') {
        if ($latest.BaseName -match "\.(?<version>$([regex]::Escape($BaseVersion)))$") {
            return $Matches.version
        }
        return $null
    }

    if ($latest.BaseName -match "\.(?<version>$([regex]::Escape($BaseVersion))-dev\..+)$") {
        return $Matches.version
    }

    return $null
}

function Test-PackagingTargetsFiles {
    param(
        [Parameter(Mandatory)][string]$RepoRoot
    )

    $srcPath = Join-Path $RepoRoot 'src'
    $csprojFiles = Get-ChildItem -Path $srcPath -Filter '*.csproj' -Recurse -File |
        Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }

    $missing = @()
    foreach ($csproj in $csprojFiles) {
        $content = Get-Content -Raw -Path $csproj.FullName
        if ($content -match '<IsPackable>\s*false\s*</IsPackable>') {
            continue
        }

        $projectName = [System.IO.Path]::GetFileNameWithoutExtension($csproj.Name)
        $targetsPath = Join-Path $csproj.DirectoryName "$projectName.targets"
        if (-not (Test-Path $targetsPath)) {
            $missing += $targetsPath
        }
    }

    if ($missing.Count -gt 0) {
        $missing | ForEach-Object { Write-Warning "Arquivo .targets ausente: $_" }
        throw "Existem $($missing.Count) projeto(s) empacotavel(is) sem o arquivo .targets exigido por Directory.Build.props. Crie-o antes de gerar o pacote."
    }

    Write-Host 'OK: todos os projetos empacotaveis possuem o arquivo .targets correspondente.' -ForegroundColor Green
}

function Invoke-PackageBuild {
    param(
        [Parameter(Mandatory)][string]$RepoRoot,
        [Parameter(Mandatory)][string]$SolutionName,
        [Parameter(Mandatory)][string]$PackageVersion,
        [Parameter(Mandatory)][string]$Configuration,
        [Parameter(Mandatory)][string]$OutputPath,
        [switch]$DryRun
    )

    $solutionFile = Get-ChildItem -Path $RepoRoot -Filter "$SolutionName.sln" -File | Select-Object -First 1
    if (-not $solutionFile) {
        throw "Nao foi possivel localizar $SolutionName.sln em $RepoRoot"
    }

    $previousVersionPattern = Join-Path $OutputPath "$SolutionName.*.$PackageVersion"
    $nugetCachePattern = Join-Path $env:USERPROFILE ".nuget\packages\$SolutionName.*\$PackageVersion"
    $cleanupTargets = @(
        (Join-Path $RepoRoot 'src\*\bin'),
        (Join-Path $RepoRoot 'src\*\obj'),
        (Join-Path $RepoRoot 'test\*\bin'),
        (Join-Path $RepoRoot 'test\*\obj')
    )

    if ($DryRun) {
        Write-Host "[DryRun] Removeria pacotes previos em: $previousVersionPattern" -ForegroundColor Yellow
        Write-Host "[DryRun] Removeria cache local em: $nugetCachePattern" -ForegroundColor Yellow
        foreach ($target in $cleanupTargets) {
            Write-Host "[DryRun] Removeria: $target" -ForegroundColor Yellow
        }
        Write-Host "[DryRun] Executaria: dotnet pack `"$($solutionFile.FullName)`" -c $Configuration -p:PackageVersion=$PackageVersion -o `"$OutputPath`"" -ForegroundColor Yellow
        return
    }

    if (Test-Path $OutputPath) {
        Get-Item -Path $previousVersionPattern -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force
    }
    Get-Item -Path $nugetCachePattern -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force

    foreach ($target in $cleanupTargets) {
        Get-Item -Path $target -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force
    }

    if (-not (Test-Path $OutputPath)) {
        New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    }

    dotnet pack $solutionFile.FullName -c $Configuration -p:PackageVersion=$PackageVersion -o $OutputPath
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet pack falhou com codigo de saida $LASTEXITCODE"
    }

    $generated = Get-ChildItem -Path $OutputPath -Filter '*.nupkg' -File
    if (-not $generated) {
        throw "Nenhum pacote .nupkg foi gerado em $OutputPath"
    }

    Write-Host "Pacotes gerados em $OutputPath :" -ForegroundColor Green
    $generated | ForEach-Object { Write-Host " - $($_.Name)" }
}

function Resolve-NuGetFeed {
    param(
        [Parameter(Mandatory)][ValidateSet('Local', 'NugetTest', 'Nuget', 'Artifactory')][string]$Feed
    )

    switch ($Feed) {
        'Local' {
            [PSCustomObject]@{
                Name            = 'Local'
                Source          = 'c:\software\nugetfeed'
                SourceName      = $null
                ApiKeyEnvVar    = $null
                UsernameEnvVar  = $null
                RequiresApiKey  = $false
                RequiresUsername = $false
            }
        }
        'NugetTest' {
            [PSCustomObject]@{
                Name            = 'NugetTest'
                Source          = 'https://apiint.nugettest.org/v3/index.json'
                SourceName      = $null
                ApiKeyEnvVar    = 'NUGETTEST_API_KEY'
                UsernameEnvVar  = $null
                RequiresApiKey  = $true
                RequiresUsername = $false
            }
        }
        'Nuget' {
            [PSCustomObject]@{
                Name            = 'Nuget'
                Source          = 'https://api.nuget.org/v3/index.json'
                SourceName      = $null
                ApiKeyEnvVar    = 'NUGET_API_KEY'
                UsernameEnvVar  = $null
                RequiresApiKey  = $true
                RequiresUsername = $false
            }
        }
        'Artifactory' {
            [PSCustomObject]@{
                Name            = 'Artifactory'
                Source          = 'https://artifacts.cat.com/artifactory/api/nuget/v3/cat-brazil-jfrog-nuget'
                SourceName      = 'Artifactory'
                ApiKeyEnvVar    = 'ARTIFACTORY_PASSWORD'
                UsernameEnvVar  = 'ARTIFACTORY_USERNAME'
                RequiresApiKey  = $true
                RequiresUsername = $true
            }
        }
    }
}

function Test-FeedCredential {
    param([Parameter(Mandatory)]$FeedInfo)

    if (-not $FeedInfo.RequiresApiKey) {
        return $null
    }

    $apiKey = [System.Environment]::GetEnvironmentVariable($FeedInfo.ApiKeyEnvVar)
    if ([string]::IsNullOrWhiteSpace($apiKey)) {
        throw "Variavel de ambiente '$($FeedInfo.ApiKeyEnvVar)' nao definida. Configure-a antes de publicar no feed '$($FeedInfo.Name)'."
    }

    $username = $null
    if ($FeedInfo.RequiresUsername) {
        $username = [System.Environment]::GetEnvironmentVariable($FeedInfo.UsernameEnvVar)
        if ([string]::IsNullOrWhiteSpace($username)) {
            throw "Variavel de ambiente '$($FeedInfo.UsernameEnvVar)' nao definida. Configure-a antes de publicar no feed '$($FeedInfo.Name)'."
        }
    }

    return [PSCustomObject]@{
        ApiKey   = $apiKey
        Username = $username
    }
}

function Register-NuGetSourceCredential {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$Source,
        [Parameter(Mandatory)][string]$Username,
        [Parameter(Mandatory)][string]$Password
    )

    # Tenta atualizar uma fonte existente primeiro; se nao existir, cria uma nova.
    # A senha e armazenada criptografada (DPAPI) no NuGet.Config do usuario, pois
    # --store-password-in-clear-text nao e usado.
    dotnet nuget update source $Name --source $Source --username $Username --password $Password --protocol-version 3 2>$null | Out-Null
    if ($LASTEXITCODE -ne 0) {
        dotnet nuget add source $Source --name $Name --username $Username --password $Password --protocol-version 3
        if ($LASTEXITCODE -ne 0) {
            throw "Falha ao registrar a fonte NuGet '$Name' (codigo $LASTEXITCODE)."
        }
    }

    Write-Host "Fonte NuGet '$Name' configurada ($Source)." -ForegroundColor Green
}

function Invoke-PackagePublish {
    param(
        [Parameter(Mandatory)][string]$PackagesPath,
        [Parameter(Mandatory)]$FeedInfo,
        $Credential,
        [string]$PackageVersion,
        [switch]$DryRun
    )

    # Feeds com usuario (Artifactory) autenticam via fonte NuGet registrada localmente
    # (basic auth); feeds so-com-api-key (NugetTest/Nuget) enviam --api-key no push direto.
    $usesApiKeyHeader = $FeedInfo.RequiresApiKey -and -not $FeedInfo.RequiresUsername
    $apiKey = if ($usesApiKeyHeader -and $Credential) { $Credential.ApiKey } else { $null }
    $pushSource = $FeedInfo.Source

    if ($FeedInfo.RequiresUsername) {
        if ($DryRun) {
            Write-Host "[DryRun] Garantiria a fonte NuGet '$($FeedInfo.SourceName)' ($($FeedInfo.Source)) com autenticacao basica (env:$($FeedInfo.UsernameEnvVar) + env:$($FeedInfo.ApiKeyEnvVar))." -ForegroundColor Yellow
        }
        else {
            Register-NuGetSourceCredential -Name $FeedInfo.SourceName -Source $FeedInfo.Source -Username $Credential.Username -Password $Credential.ApiKey
        }
        $pushSource = $FeedInfo.SourceName
    }

    $packages = Get-ChildItem -Path $PackagesPath -Filter '*.nupkg' -File -ErrorAction SilentlyContinue
    if (-not [string]::IsNullOrWhiteSpace($PackageVersion)) {
        $packages = $packages | Where-Object { $_.BaseName -like "*.$PackageVersion" }
    }

    if (-not $packages) {
        if ($DryRun) {
            $maskedKey = if ($usesApiKeyHeader) { ' --api-key ***' } else { '' }
            if (-not [string]::IsNullOrWhiteSpace($PackageVersion)) {
                Write-Host "[DryRun] Nenhum pacote encontrado para a versao '$PackageVersion' em $PackagesPath." -ForegroundColor Yellow
                Write-Host "[DryRun] Executaria: dotnet nuget push `"$PackagesPath\*.$PackageVersion.nupkg`" --source $pushSource --skip-duplicate$maskedKey" -ForegroundColor Yellow
            }
            else {
                Write-Host "[DryRun] Nenhum pacote presente ainda em $PackagesPath; publicaria todos os *.nupkg gerados pelo pack." -ForegroundColor Yellow
                Write-Host "[DryRun] Executaria: dotnet nuget push `"$PackagesPath\*.nupkg`" --source $pushSource --skip-duplicate$maskedKey" -ForegroundColor Yellow
            }
            return
        }

        if (-not [string]::IsNullOrWhiteSpace($PackageVersion)) {
            throw "Nenhum pacote .nupkg encontrado para a versao '$PackageVersion' em $PackagesPath. Informe -BuildNumber correto, rode com -Pack ou remova -PublishCurrentVersionOnly."
        }

        throw "Nenhum pacote .nupkg encontrado em $PackagesPath para publicar."
    }

    if ($FeedInfo.Name -eq 'Local' -and -not $DryRun -and -not (Test-Path $FeedInfo.Source)) {
        New-Item -ItemType Directory -Path $FeedInfo.Source -Force | Out-Null
    }

    foreach ($package in $packages) {
        $pushArgs = @('nuget', 'push', $package.FullName, '--source', $pushSource, '--skip-duplicate')
        if ($apiKey) {
            $pushArgs += @('--api-key', $apiKey)
        }

        if ($DryRun) {
            $displayArgs = if ($apiKey) { $pushArgs | ForEach-Object { if ($_ -eq $apiKey) { '***' } else { $_ } } } else { $pushArgs }
            Write-Host "[DryRun] Executaria: dotnet $($displayArgs -join ' ')" -ForegroundColor Yellow
            continue
        }

        dotnet @pushArgs
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet nuget push falhou para $($package.Name) com codigo de saida $LASTEXITCODE"
        }
    }

    Write-Host "Publicacao concluida no feed '$($FeedInfo.Name)' ($pushSource)." -ForegroundColor Green
}

# ---------------------------------------------------------------------------
# Fluxo principal
# ---------------------------------------------------------------------------

$repoRoot = Get-RepoRoot -ScriptRoot $PSScriptRoot
$solutionName = Get-SolutionName -RepoRoot $repoRoot
$outputPath = Join-Path 'c:\software' $solutionName
$shouldPack = $Pack -or -not $Publish
$needsVersion = $shouldPack -or $PublishCurrentVersionOnly
$baseVersion = $null
$packageVersion = $null
$publishVersionFilter = $null

Write-Host "Repositorio : $repoRoot"
Write-Host "Solucao     : $solutionName"
Write-Host "Ambiente    : $Environment"
Write-Host "Saida       : $outputPath"
if ($shouldPack -and $Publish) {
    Write-Host 'Operacao    : PACK + PUBLISH'
}
elseif ($shouldPack) {
    Write-Host 'Operacao    : PACK'
}
else {
    Write-Host 'Operacao    : PUBLISH'
}
if ($DryRun) {
    Write-Host 'Modo        : DRY-RUN (nenhum comando destrutivo ou de rede sera executado)' -ForegroundColor Yellow
}

if ($needsVersion) {
    $baseVersion = Get-PackageBaseVersion -RepoRoot $repoRoot
    $packageVersion = Get-PackageVersion -BaseVersion $baseVersion -Environment $Environment -BuildNumber $BuildNumber
    Write-Host "Versao base : $baseVersion"
    Write-Host "Versao pkg  : $packageVersion"
}

if ($shouldPack) {
    Test-PackagingTargetsFiles -RepoRoot $repoRoot

    Invoke-PackageBuild -RepoRoot $repoRoot -SolutionName $solutionName -PackageVersion $packageVersion `
        -Configuration $Configuration -OutputPath $outputPath -DryRun:$DryRun
}

if ($Publish) {
    if ($PublishCurrentVersionOnly) {
        $publishVersionFilter = $packageVersion

        if (-not $shouldPack -and $Environment -ne 'PRD' -and [string]::IsNullOrWhiteSpace($BuildNumber)) {
            $inferredVersion = Get-LatestPackageVersionFromOutput -OutputPath $outputPath -BaseVersion $baseVersion -Environment $Environment
            if (-not [string]::IsNullOrWhiteSpace($inferredVersion)) {
                $publishVersionFilter = $inferredVersion
                Write-Host "Filtro pub. : versao inferida da pasta de saida ($publishVersionFilter)."
            }
            else {
                Write-Warning "Nao foi possivel inferir versao existente para filtro em $outputPath. Mantendo versao alvo calculada ($publishVersionFilter)."
            }
        }

        Write-Host "Filtro pub. : somente versao $publishVersionFilter"
    }

    $feedInfo = Resolve-NuGetFeed -Feed $Feed
    $credential = $null

    if ($feedInfo.RequiresApiKey) {
        if ($DryRun) {
            $existingKey = [System.Environment]::GetEnvironmentVariable($feedInfo.ApiKeyEnvVar)
            if ([string]::IsNullOrWhiteSpace($existingKey)) {
                Write-Warning "[DryRun] Variavel de ambiente '$($feedInfo.ApiKeyEnvVar)' nao esta definida (necessaria para publicar de verdade)."
            }
            if ($feedInfo.RequiresUsername) {
                $existingUser = [System.Environment]::GetEnvironmentVariable($feedInfo.UsernameEnvVar)
                if ([string]::IsNullOrWhiteSpace($existingUser)) {
                    Write-Warning "[DryRun] Variavel de ambiente '$($feedInfo.UsernameEnvVar)' nao esta definida (necessaria para publicar de verdade)."
                }
            }
        }
        else {
            $credential = Test-FeedCredential -FeedInfo $feedInfo
        }
    }

    Invoke-PackagePublish -PackagesPath $outputPath -FeedInfo $feedInfo -Credential $credential -PackageVersion $publishVersionFilter -DryRun:$DryRun
}

Write-Host 'Concluido.' -ForegroundColor Cyan
