#!/usr/bin/env pwsh
# Gerenciado pelo Altivy.Groundwork (componente publish-nuget). Não edite: mudanças vão para o catálogo.

<#
.SYNOPSIS
Publica pacotes NuGet de forma idempotente e registra o digest publicado de cada um.

.DESCRIPTION
Para cada .nupkg, consulta o PackageBaseAddress do feed. Pacote já existente não
é reenviado: o script baixa a versão publicada e registra o SHA-256 dela, de modo
que o manifesto da release sempre descreve o que está no feed. Pacote ausente é
enviado com dotnet nuget push e registrado com o SHA-256 local.
Feed github usa GitHub Packages do owner com GITHUB_TOKEN; nugetorg usa
NUGET_API_KEY. Os tokens são lidos do ambiente e nunca exibidos.

.PARAMETER ArtifactsPath
Diretório com os .nupkg (e .snupkg) gerados.

.PARAMETER Feed
github ou nugetorg.

.PARAMETER Owner
Owner do GitHub Packages (feed github).

.PARAMETER EntriesPath
Arquivo JSON de saída com as entradas do manifesto da release.

.PARAMETER DryRun
Mostra o que seria enviado sem publicar.

.EXAMPLE
./tools/scripts/ci/Publish-NuGet.ps1 -ArtifactsPath artifacts/nupkg -Feed github -Owner minha-org -EntriesPath artifacts/release-entries/nuget.json

.EXAMPLE
./tools/scripts/ci/Publish-NuGet.ps1 --help

.NOTES
Requer PowerShell 7+ e SDK .NET. Código de saída 0 sucesso, 1 erro e 2 argumento desconhecido.

.LINK
../../../docs/runbooks/release.md
#>
#Requires -Version 7.0
[CmdletBinding(PositionalBinding = $false)]
param(
    [string] $ArtifactsPath = 'artifacts/nupkg',
    [ValidateSet('github', 'nugetorg')]
    [string] $Feed = 'github',
    [string] $Owner = $env:GITHUB_REPOSITORY_OWNER,
    [string] $EntriesPath = 'artifacts/release-entries/nuget.json',
    [switch] $DryRun,
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

Add-Type -AssemblyName System.IO.Compression.FileSystem

function Get-PackageIdentity([string] $Path) {
    $zip = [System.IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $entry = $zip.Entries | Where-Object { $_.FullName -notmatch '/' -and $_.Name -like '*.nuspec' } | Select-Object -First 1
        if ($null -eq $entry) { throw "nuspec ausente em $Path." }
        $reader = [System.IO.StreamReader]::new($entry.Open())
        try { [xml]$nuspec = $reader.ReadToEnd() } finally { $reader.Dispose() }
        return @{ Id = [string]$nuspec.package.metadata.id; Version = [string]$nuspec.package.metadata.version }
    }
    finally {
        $zip.Dispose()
    }
}

try {
    if ($Feed -eq 'github') {
        if ($Owner -notmatch '^[A-Za-z0-9-]+$') { throw 'Owner inválido para o GitHub Packages.' }
        $source = "https://nuget.pkg.github.com/$Owner/index.json"
        $apiKey = $env:GITHUB_TOKEN
        $headers = @{ Authorization = "Bearer $apiKey" }
    }
    else {
        $source = 'https://api.nuget.org/v3/index.json'
        $apiKey = $env:NUGET_API_KEY
        $headers = @{}
    }

    if (-not $DryRun -and [string]::IsNullOrWhiteSpace($apiKey)) {
        throw "Credencial ausente para o feed $Feed (GITHUB_TOKEN ou secret NUGET_API_KEY)."
    }

    $packages = @(Get-ChildItem -LiteralPath $ArtifactsPath -Filter '*.nupkg' -File | Sort-Object Name)
    if ($packages.Count -eq 0) { throw "Nenhum .nupkg em $ArtifactsPath." }

    $index = Invoke-RestMethod -Uri $source -Headers $headers
    $baseAddress = @($index.resources | Where-Object { $_.'@type' -like 'PackageBaseAddress/3.0.0*' } | Select-Object -First 1).'@id'
    if (-not $baseAddress) { throw "Feed $Feed sem PackageBaseAddress." }
    $baseAddress = $baseAddress.TrimEnd('/') + '/'

    $entries = [System.Collections.Generic.List[object]]::new()
    foreach ($package in $packages) {
        $identity = Get-PackageIdentity $package.FullName
        $id = $identity.Id.ToLowerInvariant()
        $version = $identity.Version.ToLowerInvariant()
        $published = Join-Path ([System.IO.Path]::GetTempPath()) "$id.$version.$([guid]::NewGuid().ToString('N')).nupkg"
        $exists = $false
        try {
            $response = Invoke-WebRequest -Uri "$baseAddress$id/$version/$id.$version.nupkg" -Headers $headers -OutFile $published -SkipHttpErrorCheck -PassThru
            $status = [int]$response.StatusCode
            if ($status -eq 200) { $exists = $true }
            elseif ($status -ne 404) { throw "Consulta de $($identity.Id) $($identity.Version) retornou HTTP $status." }

            if ($exists) {
                $digest = (Get-FileHash -LiteralPath $published -Algorithm SHA256).Hash.ToLowerInvariant()
                Write-Output "$($identity.Id) $($identity.Version) já publicado em $Feed; não reenviado."
            }
            else {
                $digest = (Get-FileHash -LiteralPath $package.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
                if ($DryRun) {
                    Write-Output "[dry-run] enviaria $($package.Name) para $Feed."
                }
                else {
                    $pushArguments = @('nuget', 'push', $package.FullName, '--source', $source, '--api-key', $apiKey, '--skip-duplicate')
                    if ($Feed -eq 'github') { $pushArguments += '--no-symbols' }
                    & dotnet @pushArguments | Out-Null
                    if ($LASTEXITCODE -ne 0) { throw "Falha ao publicar $($package.Name) (código $LASTEXITCODE)." }
                    Write-Output "$($identity.Id) $($identity.Version) publicado em $Feed."
                }
            }
        }
        finally {
            Remove-Item -LiteralPath $published -Force -ErrorAction SilentlyContinue
        }

        $entries.Add([ordered]@{
                kind      = 'nuget'
                name      = $identity.Id
                reference = "$Feed/$($identity.Id)@$($identity.Version)"
                digest    = "sha256:$digest"
            })
    }

    $directory = Split-Path -Parent ([System.IO.Path]::GetFullPath($EntriesPath))
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
    ConvertTo-Json -InputObject $entries.ToArray() -Depth 4 | Set-Content -LiteralPath $EntriesPath -Encoding utf8NoBOM
}
catch {
    Write-Output "::error title=Publicação NuGet::$($_.Exception.Message)"
    exit 1
}

exit 0
