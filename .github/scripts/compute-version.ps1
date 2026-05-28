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

function ConvertTo-VersionObject {
    param(
        [Parameter(Mandatory = $true)]
        [string]$VersionText
    )

    return [System.Version]::Parse($VersionText)
}

function Get-StableVersionFromFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath
    )

    $content = Get-Content -Path $FilePath -Raw
    $match = [regex]::Match($content, '<Version>(?<version>\d+\.\d+\.\d+)</Version>')

    if (-not $match.Success) {
        throw "Nao foi possivel localizar <Version> no arquivo $FilePath"
    }

    return $match.Groups['version'].Value
}

function Get-LatestStableTag {
    $tags = git tag --list 'v*' 2>$null

    if ([string]::IsNullOrWhiteSpace(($tags | Out-String))) {
        return $null
    }

    $stableTags = foreach ($tag in $tags) {
        if ($tag -match '^v(?<version>\d+\.\d+\.\d+)$') {
            [PSCustomObject]@{
                Tag     = $tag
                Version = ConvertTo-VersionObject -VersionText $Matches.version
            }
        }
    }

    return $stableTags |
    Sort-Object -Property Version -Descending |
    Select-Object -First 1
}

function Get-StableTagForHead {
    $tags = git tag --points-at HEAD --list 'v*' 2>$null

    if ([string]::IsNullOrWhiteSpace(($tags | Out-String))) {
        return $null
    }

    foreach ($tag in $tags) {
        if ($tag -match '^v(?<version>\d+\.\d+\.\d+)$') {
            return $Matches.version
        }
    }

    return $null
}

function Get-NextPatchVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string]$VersionText
    )

    $version = ConvertTo-VersionObject -VersionText $VersionText
    return '{0}.{1}.{2}' -f $version.Major, $version.Minor, ($version.Build + 1)
}

$fileVersion = Get-StableVersionFromFile -FilePath $VersionFile
$latestStableTag = Get-LatestStableTag

if ($null -eq $latestStableTag) {
    $nextStableVersion = $fileVersion
}
else {
    $fileVersionObject = ConvertTo-VersionObject -VersionText $fileVersion
    if ($latestStableTag.Version -gt $fileVersionObject) {
        $nextStableVersion = Get-NextPatchVersion -VersionText $latestStableTag.Version.ToString()
    }
    elseif ($latestStableTag.Version -eq $fileVersionObject) {
        $nextStableVersion = Get-NextPatchVersion -VersionText $fileVersion
    }
    else {
        $nextStableVersion = $fileVersion
    }
}

switch ($Channel) {
    'stable' {
        $existingHeadTag = Get-StableTagForHead
        $version = if ($null -ne $existingHeadTag) { $existingHeadTag } else { $nextStableVersion }
        $tag = "v$version"
        $isPrerelease = 'false'
        $createRelease = 'true'
        $nugetSource = 'https://api.nuget.org/v3/index.json'
        $environmentName = 'production'
    }
    'preview' {
        $version = "$nextStableVersion-preview.$RunNumber"
        $tag = "v$version"
        $isPrerelease = 'true'
        $createRelease = 'true'
        $nugetSource = 'https://api.nuget.org/v3/index.json'
        $environmentName = 'preview'
    }
    'dev' {
        $version = "$nextStableVersion-dev.$RunNumber"
        $tag = "v$version"
        $isPrerelease = 'true'
        $createRelease = 'false'
        $nugetSource = 'https://int.nugettest.org/'
        $environmentName = 'nugettest'
    }
}

"version=$version" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
"tag=$tag" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
"is_prerelease=$isPrerelease" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
"create_release=$createRelease" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
"nuget_source=$nugetSource" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
"environment_name=$environmentName" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
