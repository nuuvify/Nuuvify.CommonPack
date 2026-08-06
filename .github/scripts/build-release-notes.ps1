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

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-StableSection {
    param([string]$FilePath, [string]$Ver)

    if (-not (Test-Path -Path $FilePath -PathType Leaf)) {
        throw "Changelog nao encontrado em $FilePath."
    }
    $content = Get-Content -Path $FilePath -Raw
    if ([string]::IsNullOrWhiteSpace($content)) {
        throw "O changelog $FilePath esta vazio."
    }

    # Lê a seção fechada [X.Y.Z] correspondente à versão
    $escaped = [regex]::Escape($Ver)
    $match = [regex]::Match($content, "## \[$escaped\][^\r\n]*\r?\n(?<section>[\s\S]*?)(\r?\n## \[|$)")
    if (-not $match.Success) {
        throw "Changelog nao contem a secao fechada '## [$Ver]'. Execute prepare-release.yml antes de publicar."
    }
    $excerpt = $match.Groups['section'].Value.Trim()
    if ([string]::IsNullOrWhiteSpace($excerpt) -or $excerpt -notmatch '(?m)^[-#]') {
        throw "A secao '## [$Ver]' esta vazia ou sem itens."
    }
    return $excerpt
}

function Get-UnreleasedSection {
    param([string]$FilePath)

    if (-not (Test-Path -Path $FilePath -PathType Leaf)) {
        throw "Changelog nao encontrado em $FilePath."
    }
    $content = Get-Content -Path $FilePath -Raw
    if ([string]::IsNullOrWhiteSpace($content)) {
        throw "O changelog $FilePath esta vazio."
    }

    $match = [regex]::Match($content, '## \[Não Lançado\](?<section>[\s\S]*?)(\r?\n## \[|$)')
    if (-not $match.Success) {
        # Aceita também a variante em inglês durante transição
        $match = [regex]::Match($content, '## \[Unreleased\](?<section>[\s\S]*?)(\r?\n## \[|$)')
    }
    if (-not $match.Success) {
        throw "O changelog deve conter a secao '## [Nao Lancado]'."
    }
    $excerpt = $match.Groups['section'].Value.Trim()
    if ([string]::IsNullOrWhiteSpace($excerpt) -or $excerpt -notmatch '(?m)^[-#]') {
        throw "A secao '## [Nao Lancado]' nao possui itens de release."
    }
    return $excerpt
}

$channelLabel = switch ($Channel) {
    'stable' { 'Stable' }
    'preview' { 'Preview' }
    'dev' { 'Dev' }
}

$excerpt = if ($Channel -eq 'stable') {
    # Para stable: lê a seção fechada que foi criada pelo prepare-release.yml
    Get-StableSection -FilePath $ChangelogPath -Ver ($Version -split '-')[0]
}
else {
    # Para preview/dev: lê a seção Não Lançado ainda aberta
    Get-UnreleasedSection -FilePath $ChangelogPath
}

$notes = @(
    "# Release $Version"
    ""
    "- Canal: $channelLabel"
    "- Branch base: $BaseBranch"
    "- Gerado por GitHub Actions"
    ""
    "## Resumo do changelog"
    ""
    $excerpt
)

$notes -join [Environment]::NewLine | Set-Content -Path $OutputPath -Encoding utf8
