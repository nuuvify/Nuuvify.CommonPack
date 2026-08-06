[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ArtifactsPath,

    [Parameter(Mandatory = $true)]
    [string]$VersionFile,

    [Parameter(Mandatory = $false)]
    [string]$ExpectedVersion = '',

    [Parameter(Mandatory = $false)]
    [string]$ExpectedAssemblyVersion = '',

    [Parameter(Mandatory = $false)]
    [string]$ExpectedFileVersion = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.IO.Compression.FileSystem

function Get-VersionPrefixFromFile {
    param([string]$FilePath)
    $content = Get-Content -Path $FilePath -Raw
    $match = [regex]::Match($content, '<VersionPrefix>(?<v>\d+\.\d+\.\d+)</VersionPrefix>')
    if (-not $match.Success) {
        throw "VersionPrefix ausente em $FilePath"
    }
    return $match.Groups['v'].Value
}

if ([string]::IsNullOrWhiteSpace($ExpectedVersion)) {
    $ExpectedVersion = Get-VersionPrefixFromFile -FilePath $VersionFile
}

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

        if ($nuspecVersion -ne $ExpectedVersion) {
            $failures.Add("[$($pkg.Name)] nuspec version='$nuspecVersion' != esperado='$ExpectedVersion'.")
        }
        else {
            Write-Host "  nuspec version: $nuspecVersion OK"
        }

        # Inspeciona assemblies em lib/**
        $libEntries = $zip.Entries | Where-Object { $_.FullName -like 'lib/*/*.dll' }
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
                    $expectedMajor = ([System.Version]$ExpectedVersion).Major
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
    Write-Error ("Auditoria de artefatos falhou com $($failures.Count) problema(s):`n" + ($failures -join "`n"))
    exit 1
}

Write-Host "`nTodos os $($nupkgs.Count) pacote(s) validados com sucesso."
