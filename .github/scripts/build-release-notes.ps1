[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [Parameter(Mandatory = $true)]
    [ValidateSet('stable', 'preview', 'dev')]
    [string]$Channel,

    [Parameter(Mandatory = $true)]
    [string]$BaseBranch,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath,

    [Parameter(Mandatory = $false)]
    [string]$ChangelogPath = 'CHANGELOG.md'
)

function Get-ChangelogExcerpt {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath
    )

    $content = Get-Content -Path $FilePath -Raw
    $match = [regex]::Match($content, '## \[Não Lançado\](?<section>[\s\S]*?)(\r?\n## \[|$)')

    if ($match.Success) {
        return $match.Groups['section'].Value.Trim()
    }

    return "Sem secao 'Nao Lancado' no changelog."
}

$channelLabel = switch ($Channel) {
    'stable' { 'Stable' }
    'preview' { 'Preview' }
    'dev' { 'Dev' }
}

$notes = @()
$notes += "# Release $Version"
$notes += ""
$notes += "- Canal: $channelLabel"
$notes += "- Branch base: $BaseBranch"
$notes += "- Gerado por GitHub Actions"
$notes += ""
$notes += "## Resumo do changelog"
$notes += ""
$notes += (Get-ChangelogExcerpt -FilePath $ChangelogPath)

$notes -join [Environment]::NewLine | Set-Content -Path $OutputPath -Encoding utf8
