function Test-GitHubReleasePublished {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('stable', 'preview', 'dev')]
        [string]$Channel,

        [Parameter(Mandatory = $true)]
        [string]$Version,

        [Parameter(Mandatory = $true)]
        [string]$Repository,

        [Parameter(Mandatory = $true)]
        [string]$Token,

        [Parameter()]
        [scriptblock]$Request = {
            param($Uri, $Headers)
            Invoke-RestMethod -Method Get -Uri $Uri -Headers $Headers
        }
    )

    if ($Channel -ne 'stable') {
        return $false
    }

    if ($Version -notmatch '^\d+\.\d+\.\d+$') {
        throw "Versao '$Version' nao e SemVer estrita (X.Y.Z)."
    }

    if ([string]::IsNullOrWhiteSpace($Repository) -or [string]::IsNullOrWhiteSpace($Token)) {
        throw 'Repository e Token sao obrigatorios para consultar releases estaveis.'
    }

    $uri = "https://api.github.com/repos/$Repository/releases/tags/v$Version"
    $headers = @{
        Accept = 'application/vnd.github+json'
        Authorization = "Bearer $Token"
        'X-GitHub-Api-Version' = '2022-11-28'
    }

    try {
        $release = & $Request $uri $headers
        if (-not $release.id) {
            throw "A resposta da API para a release v$Version nao contem um identificador."
        }

        return $true
    }
    catch {
        $statusCode = if ($null -ne $_.Exception.Response) {
            [int]$_.Exception.Response.StatusCode
        }
        else {
            0
        }

        if ($statusCode -eq 404) {
            return $false
        }

        throw
    }
}

Export-ModuleMember -Function Test-GitHubReleasePublished
