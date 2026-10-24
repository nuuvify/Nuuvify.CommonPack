#!/usr/bin/env pwsh
# Gerenciado pelo Altivy.Groundwork (componente release). Não edite: mudanças vão para o catálogo.

<#
.SYNOPSIS
Audita versões de pacotes NuGet gerados antes da publicação.

.DESCRIPTION
Para cada .nupkg, confere a versão do nuspec e, no assembly próprio do pacote,
AssemblyVersion, FileVersion e ProductVersion (InformationalVersion) contra os
valores esperados calculados por Get-BuildVersion.ps1. Somente leitura.

.PARAMETER ArtifactsPath
Diretório com os .nupkg.

.PARAMETER ExpectedVersion
Versão NuGet esperada (X.Y.Z[-pre]).

.PARAMETER ExpectedAssemblyVersion
AssemblyVersion esperado; vazio valida apenas o major.

.PARAMETER ExpectedFileVersion
FileVersion esperado; vazio não valida.

.EXAMPLE
./tools/scripts/ci/Assert-ArtifactVersion.ps1 -ArtifactsPath artifacts/nupkg -ExpectedVersion 1.4.0

.EXAMPLE
./tools/scripts/ci/Assert-ArtifactVersion.ps1 --help

.NOTES
Requer PowerShell 7+. Código de saída 0 válido, 1 divergente e 2 argumento desconhecido.

.LINK
../../../docs/runbooks/release.md
#>
#Requires -Version 7.0
[CmdletBinding(PositionalBinding = $false)]
param(
    [string] $ArtifactsPath,
    [string] $ExpectedVersion,
    [string] $ExpectedAssemblyVersion = '',
    [string] $ExpectedFileVersion = '',
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

if (-not $ArtifactsPath -or -not $ExpectedVersion) {
    [Console]::Error.WriteLine('Informe -ArtifactsPath e -ExpectedVersion. Use --help.')
    exit 2
}

Add-Type -AssemblyName System.IO.Compression.FileSystem

$nupkgs = @(Get-ChildItem -Path $ArtifactsPath -Filter '*.nupkg' -Recurse)
if ($nupkgs.Count -eq 0) {
    throw "Nenhum .nupkg encontrado em $ArtifactsPath"
}

$failures = [System.Collections.Generic.List[string]]::new()

foreach ($pkg in $nupkgs) {
    Write-Host "`n== $($pkg.Name) =="

    # Abre o zip e lê o nuspec
    $zip = [System.IO.Compression.ZipFile]::OpenRead($pkg.FullName)
    try {
        $nuspecEntry = $zip.Entries | Where-Object { $_.FullName -like '*.nuspec' } | Select-Object -First 1
        if ($null -eq $nuspecEntry) {
            $failures.Add("[$($pkg.Name)] .nuspec nao encontrado.")
            continue
        }

        $nuspecReader = New-Object System.IO.StreamReader($nuspecEntry.Open())
        $nuspecContent = $nuspecReader.ReadToEnd()
        $nuspecReader.Close()

        $nuspecMatch = [regex]::Match($nuspecContent, '<version>([^<]+)</version>')
        if (-not $nuspecMatch.Success) {
            $failures.Add("[$($pkg.Name)] <version> ausente no nuspec.")
            continue
        }
        $nuspecVersion = $nuspecMatch.Groups[1].Value.Trim()
        $packageId = [regex]::Match($nuspecContent, '<id>([^<]+)</id>').Groups[1].Value.Trim()

        if ($nuspecVersion -ne $ExpectedVersion) {
            $failures.Add("[$($pkg.Name)] nuspec version='$nuspecVersion' != esperado='$ExpectedVersion'.")
        }
        else {
            Write-Host "  nuspec version: $nuspecVersion OK"
        }

        # dotnet tool empacota em tools/<tfm>/any/ junto com as dependências; só o assembly próprio é auditado.
        $libEntries = @($zip.Entries | Where-Object {
                ($_.FullName -like 'lib/*/*.dll' -or $_.FullName -like 'tools/*/*/*.dll') -and $_.Name -eq "$packageId.dll"
            })
        if ($libEntries.Count -eq 0) {
            $failures.Add("[$($pkg.Name)] assembly '$packageId.dll' nao encontrado em lib/ ou tools/.")
        }

        foreach ($entry in $libEntries) {
            $tmp = [System.IO.Path]::GetTempFileName() + '.dll'
            try {
                $stream = $entry.Open()
                $fs = [System.IO.File]::Create($tmp)
                $stream.CopyTo($fs)
                $fs.Close()
                $stream.Close()

                $fvi = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($tmp)

                # AssemblyVersion via reflection
                $assemblyName = [System.Reflection.AssemblyName]::GetAssemblyName($tmp)
                $assemblyVersion = $assemblyName.Version.ToString()

                Write-Host "  [$($entry.FullName)]"
                Write-Host "    AssemblyVersion:      $assemblyVersion"
                Write-Host "    FileVersion:          $($fvi.FileVersion)"
                Write-Host "    ProductVersion:       $($fvi.ProductVersion)"

                if (-not [string]::IsNullOrWhiteSpace($ExpectedAssemblyVersion)) {
                    if ($assemblyVersion -ne $ExpectedAssemblyVersion) {
                        $failures.Add("[$($pkg.Name) / $($entry.Name)] AssemblyVersion='$assemblyVersion' != esperado='$ExpectedAssemblyVersion'.")
                    }
                }
                else {
                    # Sem valor esperado: valida que major bate com VersionPrefix
                    $expectedMajor = ([System.Version]($ExpectedVersion -split '-')[0]).Major
                    $actualMajor = $assemblyName.Version.Major
                    if ($actualMajor -ne $expectedMajor) {
                        $failures.Add("[$($pkg.Name) / $($entry.Name)] AssemblyVersion major='$actualMajor' != major esperado='$expectedMajor'.")
                    }
                }

                if (-not [string]::IsNullOrWhiteSpace($ExpectedFileVersion)) {
                    if ($fvi.FileVersion -ne $ExpectedFileVersion) {
                        $failures.Add("[$($pkg.Name) / $($entry.Name)] FileVersion='$($fvi.FileVersion)' != esperado='$ExpectedFileVersion'.")
                    }
                }

                # InformationalVersion/ProductVersion deve conter a versão SemVer
                $semverPart = ($ExpectedVersion -split '-')[0]
                if ($fvi.ProductVersion -notlike "*$semverPart*") {
                    $failures.Add("[$($pkg.Name) / $($entry.Name)] ProductVersion='$($fvi.ProductVersion)' nao contem '$semverPart'.")
                }
            }
            finally {
                if (Test-Path $tmp) { Remove-Item $tmp -Force }
            }
        }
    }
    finally {
        $zip.Dispose()
    }
}

if ($failures.Count -gt 0) {
    foreach ($failure in $failures) {
        Write-Output "::error title=Versão do artefato::$failure"
    }

    exit 1
}

Write-Host "`nTodos os $($nupkgs.Count) pacote(s) validados com sucesso."
