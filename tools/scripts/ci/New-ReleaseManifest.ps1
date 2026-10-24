#!/usr/bin/env pwsh
# Gerenciado pelo Altivy.Groundwork (componente release). Não edite: mudanças vão para o catálogo.

<#
.SYNOPSIS
Gera o release-manifest.json a partir das entradas emitidas pelo publish.

.DESCRIPTION
Lê os arquivos JSON de EntriesPath (cada um com uma entrada ou uma lista de
entradas { kind, name, reference, digest }) e grava o manifesto da release com
versão, tag, commit, rollbackMinimumVersion de .github/ci-cd.json e artefatos.
Quando a release já possui release-manifest.json (republish), não grava nada:
exige que toda entrada nova já esteja listada com o mesmo digest e emite
upload=false. Nunca sobrescreve um manifesto publicado.

.PARAMETER Tag
Tag da release.

.PARAMETER Version
Versão da release (sem o prefixo v).

.PARAMETER Commit
SHA completo do commit da tag.

.PARAMETER Repository
Repositório owner/nome.

.PARAMETER EntriesPath
Diretório com os JSON de entradas baixados dos artifacts release-entries-*.

.PARAMETER OutputPath
Caminho do manifesto gerado.

.PARAMETER ConfigPath
Configuração do pipeline. Padrão: .github/ci-cd.json.

.PARAMETER Attestation
github quando o manifesto será atestado pelo gw-release.yml; none quando
"attestations": false em .github/ci-cd.json. O valor fica gravado no manifesto e
releases com none nunca são aceitas pelo guard de deploy.

.PARAMETER GitHubOutputPath
Arquivo de saída chave=valor (por exemplo $env:GITHUB_OUTPUT).

.PARAMETER Offline
Não consulta a release existente (testes locais).

.EXAMPLE
./tools/scripts/ci/New-ReleaseManifest.ps1 -Tag v1.4.0 -Version 1.4.0 -Commit <sha> -Repository owner/repo -EntriesPath artifacts/release-entries -OutputPath artifacts/release-manifest.json

.EXAMPLE
./tools/scripts/ci/New-ReleaseManifest.ps1 --help

.NOTES
Requer PowerShell 7+ e GitHub CLI autenticado (exceto com -Offline). Código de saída 0 sucesso, 1 erro e 2 argumento desconhecido.

.LINK
../../../docs/runbooks/release.md
#>
#Requires -Version 7.0
[CmdletBinding(PositionalBinding = $false)]
param(
    [string] $Tag,
    [string] $Version,
    [string] $Commit,
    [string] $Repository = $env:GITHUB_REPOSITORY,
    [string] $EntriesPath = 'artifacts/release-entries',
    [string] $OutputPath = 'artifacts/release-manifest.json',
    [string] $ConfigPath = '.github/ci-cd.json',
    [ValidateSet('github', 'none')]
    [string] $Attestation = 'github',
    [string] $GitHubOutputPath = '',
    [switch] $Offline,
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
    if (-not (Test-ReleaseTagName ($Tag ?? '')) -or $Tag -ne "v$Version") { throw 'Tag e versão inconsistentes.' }
    if ($Commit -notmatch '^[0-9a-f]{40}$') { throw 'Commit inválido.' }
    if ($Repository -notmatch '^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$') { throw 'Repositório inválido.' }

    $entries = [System.Collections.Generic.List[object]]::new()
    if (Test-Path -LiteralPath $EntriesPath -PathType Container) {
        foreach ($file in Get-ChildItem -LiteralPath $EntriesPath -Filter '*.json' -File -Recurse | Sort-Object FullName) {
            $content = Get-Content -LiteralPath $file.FullName -Raw | ConvertFrom-Json -AsHashtable -NoEnumerate
            foreach ($entry in @($content)) { $entries.Add($entry) }
        }
    }

    if ($entries.Count -eq 0) {
        throw 'Nenhuma entrada de artefato publicada. O publish.yml deve enviar artifacts release-entries-* (ver .github/CI-CD.md).'
    }

    Assert-ReleaseEntries -Entries $entries.ToArray()
    $config = Read-CiCdConfig -Path $ConfigPath
    $minimum = [string]($config['rollbackMinimumVersion'] ?? '0.0.0')
    if (-not (Test-SemVer $minimum) -or (Compare-SemVer $minimum $Version) -gt 0) {
        throw "rollbackMinimumVersion '$minimum' inválido ou maior que a versão $Version."
    }

    $existing = $null
    if (-not $Offline) {
        $release = Find-GitHubRelease -Repository $Repository -Tag $Tag
        if ($null -eq $release) { throw "Release $Tag inexistente." }
        $text = Get-ReleaseAssetText -Repository $Repository -Release $release -Name 'release-manifest.json'
        if ($text) { $existing = ConvertFrom-Json -InputObject $text -AsHashtable -Depth 32 }
    }

    if ($null -ne $existing) {
        Assert-ReleaseManifest -Manifest $existing -ExpectedTag $Tag -ExpectedCommit $Commit
        $listed = @{}
        foreach ($artifact in $existing['artifacts']) { $listed["$($artifact['kind'])/$($artifact['name'])"] = $artifact['digest'] }
        foreach ($entry in $entries) {
            $key = "$($entry['kind'])/$($entry['name'])"
            if (-not $listed.ContainsKey($key)) { throw "O manifesto publicado não lista $key; um manifesto publicado nunca é alterado." }
            if ($listed[$key] -ne $entry['digest']) { throw "$key difere do digest publicado; artefatos publicados são imutáveis." }
        }

        Write-GitHubOutput -Path $GitHubOutputPath -Values @{ upload = 'false' } | Out-Null
        Write-Output "release-manifest.json já publicado em $Tag e consistente com $($entries.Count) artefato(s)."
        exit 0
    }

    $manifest = [ordered]@{
        schemaVersion          = 1
        repository             = $Repository
        version                = $Version
        tag                    = $Tag
        commit                 = $Commit
        rollbackMinimumVersion = $minimum
        attestation            = $Attestation
        createdAt              = [DateTimeOffset]::UtcNow.ToString('yyyy-MM-ddTHH:mm:ssZ')
        artifacts              = @($entries | Sort-Object { $_['kind'] }, { $_['name'] } | ForEach-Object {
                [ordered]@{ kind = $_['kind']; name = $_['name']; reference = $_['reference']; digest = $_['digest'] }
            })
    }

    $directory = Split-Path -Parent ([System.IO.Path]::GetFullPath($OutputPath))
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
    ($manifest | ConvertTo-Json -Depth 8) + "`n" | Set-Content -LiteralPath $OutputPath -Encoding utf8NoBOM -NoNewline
    Write-GitHubOutput -Path $GitHubOutputPath -Values @{ upload = 'true' } | Out-Null
    Write-Output "release-manifest.json gerado para $Tag com $($entries.Count) artefato(s)."
}
catch {
    Write-Output "::error title=Manifesto da release::$($_.Exception.Message)"
    exit 1
}

exit 0
