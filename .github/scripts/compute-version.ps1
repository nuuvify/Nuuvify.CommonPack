[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('stable', 'preview', 'dev')]
    [string]$Channel,

    [Parameter(Mandatory = $true)]
    [string]$VersionFile,

    [Parameter(Mandatory = $false)]
    [string]$RunNumber = '0'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-VersionPrefixFromFile {
    param([string]$FilePath)

    $content = Get-Content -Path $FilePath -Raw
    $match = [regex]::Match($content, '<VersionPrefix>(?<version>\d+\.\d+\.\d+)</VersionPrefix>')
    if (-not $match.Success) {
        throw "Nao foi possivel localizar <VersionPrefix> no arquivo $FilePath"
    }
    return $match.Groups['version'].Value
}

function Assert-StrictSemVer {
    param([string]$Version)
    if ($Version -notmatch '^\d+\.\d+\.\d+$') {
        throw "Versao '$Version' nao e SemVer estrita (X.Y.Z)."
    }
}

function ConvertTo-VersionObject {
    param([string]$VersionText)
    return [System.Version]::Parse($VersionText)
}

function Get-LatestStableTag {
    $ErrorActionPreference = 'Continue'
    $tags = (git tag --list 'v[0-9]*.[0-9]*.[0-9]*' 2>$null) -split "`n" |
    Where-Object { $_ -match '^v\d+\.\d+\.\d+$' }
    $ErrorActionPreference = 'Stop'

    if (-not $tags) { return $null }

    return $tags |
    ForEach-Object { [PSCustomObject]@{ Tag = $_; Version = ConvertTo-VersionObject ($_ -replace '^v', '') } } |
    Sort-Object -Property Version -Descending |
    Select-Object -First 1
}

function Get-StableTagForCommit {
    param([string]$CommitSha)
    $ErrorActionPreference = 'Continue'
    $tags = (git tag --points-at $CommitSha --list 'v[0-9]*.[0-9]*.[0-9]*' 2>$null) -split "`n" |
    Where-Object { $_ -match '^v\d+\.\d+\.\d+$' }
    $ErrorActionPreference = 'Stop'
    return $tags | Select-Object -First 1
}

function Get-CurrentSha {
    $ErrorActionPreference = 'Continue'
    $sha = (git rev-parse HEAD 2>$null | Out-String).Trim()
    $ErrorActionPreference = 'Stop'
    return $sha
}

# ── Leitura e validação do VersionPrefix ────────────────────────────────────
$versionPrefix = Get-VersionPrefixFromFile -FilePath $VersionFile
Assert-StrictSemVer -Version $versionPrefix

$prefixObj = ConvertTo-VersionObject -VersionText $versionPrefix
$latestTag = Get-LatestStableTag
$currentSha = Get-CurrentSha
$major = $prefixObj.Major

# ── Validação de monotonicidade para stable ──────────────────────────────────
if ($Channel -eq 'stable' -and $null -ne $latestTag) {
    # Permite reusar se a tag já aponta para o commit atual (reexecução idempotente)
    $existingTagOnHead = Get-StableTagForCommit -CommitSha $currentSha
    $isRerun = $null -ne $existingTagOnHead -and $existingTagOnHead -eq "v$versionPrefix"

    if (-not $isRerun -and $prefixObj -le $latestTag.Version) {
        throw ("VersionPrefix $versionPrefix nao e maior que a ultima tag estavel " +
            "$($latestTag.Tag). Atualize o VersionPrefix via prepare-release.yml.")
    }
}

# ── Derivação dos outputs ────────────────────────────────────────────────────
$assemblyVersion = "$major.0.0.0"
$versionSuffix = ''
$fileVersionRevision = '0'

switch ($Channel) {
    'stable' {
        $existingTagOnHead = Get-StableTagForCommit -CommitSha $currentSha
        if ($null -ne $existingTagOnHead) {
            $versionPrefix = $existingTagOnHead -replace '^v', ''
        }
        $version = $versionPrefix
        $versionSuffix = ''
        $fileVersionRevision = '0'
        $tag = "v$version"
        $isPrerelease = 'false'
        $createRelease = 'true'
        $nugetSource = 'https://api.nuget.org/v3/index.json'
        $environmentName = 'production'
    }
    'preview' {
        $versionSuffix = "preview.$RunNumber"
        $version = "$versionPrefix-$versionSuffix"
        $fileVersionRevision = $RunNumber
        $tag = "v$version"
        $isPrerelease = 'true'
        $createRelease = 'true'
        $nugetSource = 'https://api.nuget.org/v3/index.json'
        $environmentName = 'preview'
    }
    'dev' {
        $versionSuffix = "dev.$RunNumber"
        $version = "$versionPrefix-$versionSuffix"
        $fileVersionRevision = $RunNumber
        $tag = "v$version"
        $isPrerelease = 'true'
        $createRelease = 'false'
        $nugetSource = 'https://int.nugettest.org/'
        $environmentName = 'nugettest'
    }
}

$fileVersion = "$versionPrefix.$fileVersionRevision"
$informationalVersion = if ($currentSha) { "$version+$($currentSha.Substring(0, [Math]::Min(12, $currentSha.Length)))" } else { $version }

# ── Emissão dos outputs ──────────────────────────────────────────────────────
@(
    "version=$version"
    "version_prefix=$versionPrefix"
    "version_suffix=$versionSuffix"
    "assembly_version=$assemblyVersion"
    "file_version=$fileVersion"
    "file_version_revision=$fileVersionRevision"
    "informational_version=$informationalVersion"
    "tag=$tag"
    "is_prerelease=$isPrerelease"
    "create_release=$createRelease"
    "nuget_source=$nugetSource"
    "environment_name=$environmentName"
) | ForEach-Object { $_ | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8 }
