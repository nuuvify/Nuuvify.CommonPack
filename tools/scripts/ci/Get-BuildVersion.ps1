#!/usr/bin/env pwsh
# Gerenciado pelo Altivy.Groundwork (componente release). Não edite: mudanças vão para o catálogo.

<#
.SYNOPSIS
Calcula as versões de build de uma release ou de um snapshot da branch padrão.

.DESCRIPTION
release: usa a versão da tag (vX.Y.Z[-pre]), que precisa coincidir com o primeiro
arquivo de versionFiles em .github/ci-cd.json.
snapshot: usa o próximo patch da versão registrada e o sufixo preview.<RunNumber>;
snapshots nunca geram tag, GitHub Release nem são aceitos pelo deploy.
Emite version, version_prefix, version_suffix, assembly_version (Major.0.0.0) e
file_version (X.Y.Z.Revisão). Somente leitura; não acessa a rede.

.PARAMETER Channel
release ou snapshot.

.PARAMETER Tag
Tag da release (obrigatória no canal release).

.PARAMETER RunNumber
Número da execução para snapshots e revisão do FileVersion.

.PARAMETER ConfigPath
Configuração do pipeline. Padrão: .github/ci-cd.json.

.PARAMETER OutputPath
Arquivo de saída no formato chave=valor (por exemplo $env:GITHUB_OUTPUT).

.EXAMPLE
./tools/scripts/ci/Get-BuildVersion.ps1 -Channel release -Tag v1.4.0

.EXAMPLE
./tools/scripts/ci/Get-BuildVersion.ps1 -Channel snapshot -RunNumber 42

.NOTES
Requer PowerShell 7+. Código de saída 0 sucesso, 1 erro e 2 argumento desconhecido.

.LINK
../../../docs/runbooks/release.md
#>
#Requires -Version 7.0
[CmdletBinding(PositionalBinding = $false)]
param(
    [ValidateSet('release', 'snapshot')]
    [string] $Channel = 'release',
    [string] $Tag = '',
    [string] $RunNumber = '0',
    [string] $ConfigPath = '.github/ci-cd.json',
    [string] $OutputPath = '',
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
    if ($RunNumber -notmatch '^\d{1,9}$' -or [int]$RunNumber -gt 65535) { throw 'RunNumber deve ser inteiro entre 0 e 65535.' }
    $config = Read-CiCdConfig -Path $ConfigPath
    $recorded = Get-VersionFromFile -Path @(Get-ProjectVersionFiles -Config $config)[0]
    if ($Channel -eq 'release') {
        if (-not (Test-ReleaseTagName $Tag)) { throw "Tag inválida: '$Tag'." }
        $version = $Tag.Substring(1)
        if ($version -ne $recorded) { throw "A tag $Tag não corresponde à versão registrada $recorded." }
        $prefix, $suffix = $version.Split('-', 2)
        $suffix = $suffix ?? ''
        $revision = '0'
    }
    else {
        $core = $recorded.Split('-', 2)[0].Split('.')
        $prefix = "$($core[0]).$($core[1]).$([int]$core[2] + 1)"
        $suffix = "preview.$RunNumber"
        $version = "$prefix-$suffix"
        $revision = $RunNumber
    }

    Write-GitHubOutput -Path $OutputPath -Values @{
        version          = $version
        version_prefix   = $prefix
        version_suffix   = $suffix
        assembly_version = "$($prefix.Split('.')[0]).0.0.0"
        file_version     = "$prefix.$revision"
        file_revision    = $revision
    }
}
catch {
    Write-Output "::error title=Versão de build::$($_.Exception.Message)"
    exit 1
}

exit 0
