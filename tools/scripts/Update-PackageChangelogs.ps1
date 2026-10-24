#!/usr/bin/env pwsh

<#
.SYNOPSIS
Fecha a seção "## [Não Lançado]" dos CHANGELOGs por pacote na versão da release.

.DESCRIPTION
Usado no Release PR do release-please (workflow package-changelog-release.yml). Para cada
src/<pacote>/CHANGELOG.md cuja seção "## [Não Lançado]" contém itens, renomeia a seção para
"## [X.Y.Z] - AAAA-MM-DD", remove subseções vazias e recria "## [Não Lançado]" vazia acima.
Pacotes sem itens não são alterados. É idempotente: se o CHANGELOG já tiver a seção da versão,
o arquivo é ignorado. Sem -Version, lê a versão de .release-please-manifest.json.

.PARAMETER Version
Versão da release (X.Y.Z[-pre]). Padrão: valor de "." em .release-please-manifest.json.

.PARAMETER Date
Data da release no formato AAAA-MM-DD. Padrão: data atual em UTC.

.PARAMETER RepositoryRoot
Raiz do repositório. Padrão: diretório atual.

.PARAMETER DryRun
Mostra os arquivos que seriam alterados sem gravar. Também aceito como --dry-run.

.EXAMPLE
./tools/scripts/Update-PackageChangelogs.ps1 --dry-run

.EXAMPLE
./tools/scripts/Update-PackageChangelogs.ps1 -Version 2.10.0 -Date 2026-10-24

.NOTES
Requer PowerShell 7+. Não acessa a rede. Código de saída 0 sucesso, 1 erro e 2 argumento
desconhecido. Emite a lista de arquivos alterados na saída padrão, um por linha.

.LINK
../../docs/maintainers/release-process.md
#>
#Requires -Version 7.0
[CmdletBinding(PositionalBinding = $false)]
param(
    [string] $Version = '',
    [string] $Date = '',
    [string] $RepositoryRoot = '.',
    [switch] $DryRun,
    [Alias('h')]
    [switch] $Help,
    [Parameter(ValueFromRemainingArguments)]
    [string[]] $RemainingArguments
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$unknown = @()
foreach ($argument in @($RemainingArguments | Where-Object { $_ })) {
    switch ($argument) {
        { $_ -in @('--help', '-h', '/?', 'help') } { $Help = $true }
        '--dry-run' { $DryRun = $true }
        default { $unknown += $argument }
    }
}

if ($Help) {
    Get-Help -Full $PSCommandPath
    exit 0
}

if ($unknown) {
    [Console]::Error.WriteLine("Argumento desconhecido: $($unknown -join ' '). Use --help.")
    exit 2
}

$unreleasedHeading = '## [Não Lançado]'

function Get-ReleaseVersion {
    param([string] $Root)

    $manifestPath = Join-Path $Root '.release-please-manifest.json'
    if (-not (Test-Path -LiteralPath $manifestPath)) {
        throw "Informe -Version ou crie $manifestPath."
    }
    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json -AsHashtable
    if (-not $manifest.ContainsKey('.')) {
        throw ".release-please-manifest.json não contém a versão do pacote raiz ('.')."
    }
    return [string] $manifest['.']
}

function Close-UnreleasedSection {
    param(
        [string[]] $Lines,
        [string] $ReleaseVersion,
        [string] $ReleaseDate
    )

    $start = [array]::IndexOf($Lines, $unreleasedHeading)
    if ($start -lt 0) {
        return $null
    }

    $end = $Lines.Count
    for ($i = $start + 1; $i -lt $Lines.Count; $i++) {
        if ($Lines[$i] -match '^## ') {
            $end = $i
            break
        }
    }

    $body = if ($end -gt $start + 1) { $Lines[($start + 1)..($end - 1)] } else { @() }
    if (-not ($body | Where-Object { $_ -match '^\s*[-*] ' })) {
        return $null
    }

    # Remove subseções sem itens; cada bloco mantido perde as linhas em branco das bordas.
    $kept = [System.Collections.Generic.List[string]]::new()
    $pending = [System.Collections.Generic.List[string]]::new()
    $pendingHasItems = $false
    foreach ($line in @($body) + '### ') {
        if ($line -match '^### ') {
            if ($pendingHasItems) {
                $first = 0
                $last = $pending.Count - 1
                while ($pending[$first].Trim() -eq '') { $first++ }
                while ($pending[$last].Trim() -eq '') { $last-- }
                if ($kept.Count -gt 0) { $kept.Add('') }
                $kept.AddRange([string[]] $pending.GetRange($first, $last - $first + 1))
            }
            $pending.Clear()
            $pending.Add($line)
            $pendingHasItems = $false
            continue
        }
        $pending.Add($line)
        if ($line -match '^\s*[-*] ') { $pendingHasItems = $true }
    }

    $section = @($unreleasedHeading, '', "## [$ReleaseVersion] - $ReleaseDate", '') + $kept + ''
    $before = if ($start -gt 0) { $Lines[0..($start - 1)] } else { @() }
    $after = if ($end -lt $Lines.Count) { $Lines[$end..($Lines.Count - 1)] } else { @() }
    return @($before) + $section + @($after)
}

try {
    $root = (Resolve-Path -LiteralPath $RepositoryRoot).Path
    if (-not $Version) { $Version = Get-ReleaseVersion -Root $root }
    if ($Version -notmatch '^\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?$') { throw "Versão inválida: '$Version'." }
    if (-not $Date) { $Date = [DateTime]::UtcNow.ToString('yyyy-MM-dd', [Globalization.CultureInfo]::InvariantCulture) }
    if ($Date -notmatch '^\d{4}-\d{2}-\d{2}$') { throw "Data inválida: '$Date'." }

    $srcPath = Join-Path $root 'src'
    $changelogs = Get-ChildItem -LiteralPath $srcPath -Directory | ForEach-Object { Join-Path $_.FullName 'CHANGELOG.md' } |
        Where-Object { Test-Path -LiteralPath $_ } | Sort-Object

    $versionHeading = "## [$Version] - "
    foreach ($path in $changelogs) {
        $relative = [IO.Path]::GetRelativePath($root, $path).Replace('\', '/')
        $bytes = [IO.File]::ReadAllBytes($path)
        $hasBom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
        $content = [Text.UTF8Encoding]::new($false).GetString($bytes, ($hasBom ? 3 : 0), $bytes.Length - ($hasBom ? 3 : 0))
        $newline = $content.Contains("`r`n") ? "`r`n" : "`n"
        $lines = $content -split '\r?\n'

        if ($lines | Where-Object { $_.StartsWith($versionHeading, [StringComparison]::Ordinal) }) {
            Write-Verbose "$relative já contém a seção $Version; ignorado."
            continue
        }

        $updated = Close-UnreleasedSection -Lines $lines -ReleaseVersion $Version -ReleaseDate $Date
        if ($null -eq $updated) {
            continue
        }

        Write-Output $relative
        if (-not $DryRun) {
            $text = ($updated -join $newline)
            [IO.File]::WriteAllText($path, $text, [Text.UTF8Encoding]::new($hasBom))
        }
    }
    exit 0
}
catch {
    [Console]::Error.WriteLine("Erro: $($_.Exception.Message)")
    exit 1
}
