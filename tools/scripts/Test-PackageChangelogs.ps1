#!/usr/bin/env pwsh

<#
.SYNOPSIS
Exige a atualização do CHANGELOG de cada pacote com código alterado no pull request.

.DESCRIPTION
Compara a branch do PR com a base e lista os pacotes com arquivos .cs ou .csproj alterados em
src/<pacote>/. Falha se algum desses pacotes não alterou src/<pacote>/CHANGELOG.md. O CHANGELOG
raiz não é exigido: ele é gerado pelo release-please a partir dos Conventional Commits.

.PARAMETER BaseRef
Referência da base do PR (por exemplo origin/main). Ignorado quando -ChangedFile é informado.

.PARAMETER HeadRef
Referência da branch do PR. Padrão: HEAD.

.PARAMETER ChangedFile
Lista explícita de arquivos alterados (caminhos relativos à raiz), em vez de consultar o git.

.EXAMPLE
./tools/scripts/Test-PackageChangelogs.ps1 -BaseRef origin/main

.EXAMPLE
./tools/scripts/Test-PackageChangelogs.ps1 -ChangedFile src/Nuuvify.CommonPack.Domain/Cnpj.cs

.NOTES
Requer PowerShell 7+ e, sem -ChangedFile, histórico suficiente para "git diff BaseRef...HeadRef".
Somente leitura. Código de saída 0 sucesso, 1 CHANGELOG ausente ou erro e 2 argumento desconhecido.

.LINK
../../docs/maintainers/release-process.md
#>
#Requires -Version 7.0
[CmdletBinding(PositionalBinding = $false)]
param(
    [string] $BaseRef = '',
    [string] $HeadRef = 'HEAD',
    [string[]] $ChangedFile = @(),
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

try {
    $files = @($ChangedFile | Where-Object { $_ })
    if (-not $files) {
        if (-not $BaseRef) { throw 'Informe -BaseRef ou -ChangedFile.' }
        $files = @(git diff --name-only "$BaseRef...$HeadRef")
        if ($LASTEXITCODE -ne 0) { throw "git diff $BaseRef...$HeadRef falhou (código $LASTEXITCODE)." }
    }
    $files = @($files | ForEach-Object { $_.Replace('\', '/') })

    $packages = @($files | Where-Object { $_ -match '^src/([^/]+)/.*\.(cs|csproj)$' } |
            ForEach-Object { ($_ -split '/')[1] } | Sort-Object -Unique)
    $missing = @($packages | Where-Object { $files -notcontains "src/$_/CHANGELOG.md" })

    if ($missing) {
        Write-Output "::error title=CHANGELOG do pacote::Atualize a seção '## [Não Lançado]' de: $(($missing | ForEach-Object { "src/$_/CHANGELOG.md" }) -join ', ')"
        exit 1
    }

    if ($packages) {
        Write-Output "CHANGELOGs atualizados para: $($packages -join ', ')."
    }
    else {
        Write-Output 'Nenhum pacote com código alterado.'
    }
    exit 0
}
catch {
    Write-Output "::error title=CHANGELOG do pacote::$($_.Exception.Message)"
    exit 1
}
