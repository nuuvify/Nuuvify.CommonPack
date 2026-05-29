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
    [string]$ChangelogPath = 'CHANGELOG.md',

    [Parameter(Mandatory = $false)]
    [ValidateSet('default', 'labels-fallback')]
    [string]$Mode = 'default',

    [Parameter(Mandatory = $false)]
    [string]$LastTagDate = ''
)

function Get-ChangelogExcerpt {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath
    )

    $content = Get-Content -Path $FilePath -Raw
    $match = [regex]::Match($content, '## \[(Não Lançado|Unreleased)\](?<section>[\s\S]*?)(\r?\n## \[|$)')

    if ($match.Success) {
        return $match.Groups['section'].Value.Trim()
    }

    return "Sem secao '[Nao Lancado]' ou '[Unreleased]' no changelog."
}

function Get-ChangelogFromLabels {
    param(
        [string]$Version,
        [string]$LastTagDate
    )

    $labelMap = @{
        'breaking-change' = 'Removido'
        'enhancement'     = 'Adicionado'
        'bug'             = 'Corrigido'
        'documentation'   = 'Documentação'
        'performance'     = 'Performance'
        'refactor'        = 'Alterado'
    }

    $ghArgs = @('pr', 'list', '--state', 'merged', '--json', 'title,labels,mergedAt,number', '--limit', '100')
    if ($LastTagDate -ne '') {
        $ghArgs += @('--search', "merged:>=$LastTagDate")
    }

    try {
        $prJson = & gh @ghArgs 2>&1
        $prs = $prJson | ConvertFrom-Json
    }
    catch {
        return "Nao foi possivel obter PRs mergeados via gh: $_"
    }

    $grouped = @{}
    foreach ($pr in $prs) {
        $prLabels = $pr.labels | ForEach-Object { $_.name }
        $matched = $false
        foreach ($label in $labelMap.Keys) {
            if ($prLabels -contains $label) {
                $category = $labelMap[$label]
                if (-not $grouped.ContainsKey($category)) { $grouped[$category] = @() }
                $grouped[$category] += "- $($pr.title) (#$($pr.number))"
                $matched = $true
                break
            }
        }
        if (-not $matched) {
            if (-not $grouped.ContainsKey('Alterado')) { $grouped['Alterado'] = @() }
            $grouped['Alterado'] += "- $($pr.title) (#$($pr.number))"
        }
    }

    $sections = @()
    $order = @('Adicionado', 'Alterado', 'Corrigido', 'Removido', 'Documentação', 'Performance')
    foreach ($cat in $order) {
        if ($grouped.ContainsKey($cat) -and $grouped[$cat].Count -gt 0) {
            $sections += "### $cat"
            $sections += $grouped[$cat]
            $sections += ''
        }
    }

    if ($sections.Count -eq 0) {
        return "Nenhum PR mergeado encontrado com labels mapeadas."
    }

    return $sections -join [Environment]::NewLine
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

if ($Mode -eq 'labels-fallback') {
    $notes += (Get-ChangelogFromLabels -Version $Version -LastTagDate $LastTagDate)
}
else {
    $notes += (Get-ChangelogExcerpt -FilePath $ChangelogPath)
}

$notes -join [Environment]::NewLine | Set-Content -Path $OutputPath -Encoding utf8
