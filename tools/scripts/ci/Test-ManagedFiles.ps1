#!/usr/bin/env pwsh
# Gerenciado pelo Altivy.Groundwork (componente ci). Não edite: mudanças vão para o catálogo.

<#
.SYNOPSIS
Reprova edições locais em arquivos gerenciados pelo Altivy.Groundwork.

.DESCRIPTION
Compara o SHA-256 de cada arquivo gerenciado dos componentes de CI/CD,
registrado em .groundwork/manifest.json, com o conteúdo atual. Esses arquivos
não admitem exceção local: a mudança deve ser feita no catálogo do Groundwork e
chegar ao projeto por `groundwork update`. Arquivos de core, secret-scanning e
speckit admitem personalização reconciliada pelo `groundwork update` e não são
verificados. Arquivos semente não constam no manifesto e pertencem ao projeto.
Somente lê arquivos; não acessa rede.

.PARAMETER Root
Raiz do repositório. Padrão: diretório atual.

.PARAMETER Components
Componentes cujos arquivos gerenciados são verificados. Padrão: ci, release,
publish-nuget, publish-containers, publish-github-assets, deploy,
dependency-updates e repository-policy.

.PARAMETER SummaryPath
Arquivo Markdown em que o resultado é acrescentado (por exemplo $env:GITHUB_STEP_SUMMARY).

.EXAMPLE
./tools/scripts/ci/Test-ManagedFiles.ps1

.EXAMPLE
./tools/scripts/ci/Test-ManagedFiles.ps1 --help

.NOTES
Requer PowerShell 7+. Código de saída 0 quando todos estão íntegros, 1 em desvio ou manifesto inválido e 2 para argumento desconhecido.

.LINK
../../../.github/CI-CD.md
#>
#Requires -Version 7.0
[CmdletBinding(PositionalBinding = $false)]
param(
    [string] $Root = '.',
    [string] $SummaryPath = '',
    [string[]] $Components = @('ci', 'release', 'publish-nuget', 'publish-containers', 'publish-github-assets', 'deploy', 'dependency-updates', 'repository-policy'),
    [Alias('h')]
    [switch] $Help,
    [Parameter(ValueFromRemainingArguments)]
    [string[]] $RemainingArguments
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ($Help -or ($RemainingArguments | Where-Object { $_ -in @('--help', '-h', '/?', 'help') })) {
    Get-Help -Full $PSCommandPath
    exit 0
}

if ($RemainingArguments) {
    [Console]::Error.WriteLine("Argumento desconhecido: $($RemainingArguments -join ' '). Use --help.")
    exit 2
}

$rootPath = [System.IO.Path]::GetFullPath($Root)
$manifestPath = Join-Path $rootPath '.groundwork/manifest.json'
if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
    Write-Output '::error title=Arquivos gerenciados::.groundwork/manifest.json ausente; versione o manifesto gerado pelo Groundwork.'
    exit 1
}

try {
    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json -AsHashtable
}
catch {
    Write-Output '::error title=Arquivos gerenciados::.groundwork/manifest.json inválido.'
    exit 1
}

$files = $manifest['files']
if ($files -isnot [System.Collections.IDictionary]) {
    Write-Output '::error title=Arquivos gerenciados::.groundwork/manifest.json sem a lista de arquivos.'
    exit 1
}

$drift = [System.Collections.Generic.List[string]]::new()
$checked = 0
$rootPrefix = $rootPath.TrimEnd([System.IO.Path]::DirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
foreach ($relative in @($files.Keys | Sort-Object)) {
    $entry = $files[$relative]
    if ([string]$entry['component'] -notin $Components) {
        continue
    }

    $checked++
    $fullPath = [System.IO.Path]::GetFullPath((Join-Path $rootPath $relative))
    if ([System.IO.Path]::IsPathRooted($relative) -or -not $fullPath.StartsWith($rootPrefix, [System.StringComparison]::Ordinal)) {
        $drift.Add("caminho inválido no manifesto: $relative")
        continue
    }

    if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
        $drift.Add("ausente: $relative")
        continue
    }

    $hash = (Get-FileHash -LiteralPath $fullPath -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($hash -ne ([string]$entry['sha256']).ToLowerInvariant()) {
        $drift.Add("modificado: $relative ($($entry['component']))")
    }
}

if ($drift.Count -gt 0) {
    foreach ($item in $drift) {
        Write-Output "::error title=Arquivo gerenciado alterado::$item"
    }

    $message = 'Arquivos gerenciados não admitem edição local. Reverta a alteração e proponha a mudança no catálogo do Altivy.Groundwork; depois aplique com groundwork update.'
    Write-Output $message
    if ($SummaryPath) {
        @('## Arquivos gerenciados', '', $message, '') + ($drift | ForEach-Object { "- $_" }) |
            Add-Content -LiteralPath $SummaryPath -Encoding utf8
    }

    exit 1
}

Write-Output "Todos os $checked arquivos gerenciados de CI/CD conferem com .groundwork/manifest.json (catálogo $($manifest['catalogVersion']))."
if ($SummaryPath) {
    "## Arquivos gerenciados`n`n$checked arquivos de CI/CD íntegros." | Add-Content -LiteralPath $SummaryPath -Encoding utf8
}

exit 0
