#!/usr/bin/env pwsh
# Gerenciado pelo Altivy.Groundwork (componente release). Não edite: mudanças vão para o catálogo.

<#
.SYNOPSIS
Consulta o estado da GitHub Release de uma tag e, com -Publish, publica o rascunho.

.DESCRIPTION
Localiza a release pela tag (inclusive rascunhos) via GitHub CLI. -Expect falha
fechado quando o estado não corresponde. -Publish publica um rascunho somente
depois que o publish teve sucesso e confirma a publicação com nova leitura; uma
release já publicada é aceita sem alteração (idempotente). Requer GH_TOKEN com
contents:read (ou contents:write para -Publish).

.PARAMETER Tag
Tag da release.

.PARAMETER Repository
Repositório owner/nome. Padrão: $env:GITHUB_REPOSITORY.

.PARAMETER Expect
Exists, Draft ou Published.

.PARAMETER Publish
Publica a release em rascunho e confirma.

.EXAMPLE
./tools/scripts/ci/Test-PublishedRelease.ps1 -Tag v1.4.0 -Expect Published

.EXAMPLE
./tools/scripts/ci/Test-PublishedRelease.ps1 --help

.NOTES
Requer PowerShell 7+ e GitHub CLI autenticado. Código de saída 0 conforme, 1 divergente ou erro, 2 argumento desconhecido.

.LINK
../../../docs/runbooks/release.md
#>
#Requires -Version 7.0
[CmdletBinding(PositionalBinding = $false)]
param(
    [string] $Tag,
    [string] $Repository = $env:GITHUB_REPOSITORY,
    [ValidateSet('', 'Exists', 'Draft', 'Published')]
    [string] $Expect = '',
    [switch] $Publish,
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

Import-Module (Join-Path $PSScriptRoot 'GroundworkRelease.psm1') -Force

try {
    if (-not (Test-ReleaseTagName ($Tag ?? ''))) { throw "Tag inválida: '$Tag'." }
    if ($Repository -notmatch '^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$') { throw 'Repositório inválido.' }

    $release = Find-GitHubRelease -Repository $Repository -Tag $Tag
    $state = $null -eq $release ? 'Missing' : ($release['draft'] ? 'Draft' : 'Published')
    if ($Publish) {
        if ($state -eq 'Missing') { throw "Release $Tag inexistente; nada a publicar." }
        if ($state -eq 'Draft') {
            Invoke-GhJson -Path "repos/$Repository/releases/$($release['id'])" -Method PATCH -Body @{ draft = $false } | Out-Null
            $release = Find-GitHubRelease -Repository $Repository -Tag $Tag
            $state = $null -eq $release ? 'Missing' : ($release['draft'] ? 'Draft' : 'Published')
            if ($state -ne 'Published') { throw "Publicação de $Tag não confirmada (estado: $state)." }
            Write-Output "Release $Tag publicada."
        }
        else {
            Write-Output "Release $Tag já estava publicada; nada alterado."
        }
    }

    $conforms = switch ($Expect) {
        '' { $true }
        'Exists' { $state -ne 'Missing' }
        default { $state -eq $Expect }
    }

    if (-not $conforms) { throw "Release $Tag em estado $state; esperado $Expect." }
    Write-Output "Release ${Tag}: $state."
}
catch {
    Write-Output "::error title=GitHub Release::$($_.Exception.Message)"
    exit 1
}

exit 0
