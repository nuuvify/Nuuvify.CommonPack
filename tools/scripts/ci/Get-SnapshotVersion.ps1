#!/usr/bin/env pwsh

<#
.SYNOPSIS
Calcula a versão preview correspondente à próxima release estável.

.DESCRIPTION
Aplica as regras de Conventional Commits usadas pelo release-please aos commits
desde a tag estável registrada. Não gera snapshot para commits sem impacto de release
nem para o commit de merge do Release PR.
#>
#Requires -Version 7.0
[CmdletBinding(PositionalBinding = $false)]
param(
    [string] $RunNumber = '0',
    [string] $ConfigPath = '.github/ci-cd.json',
    [string] $OutputPath = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'GroundworkRelease.psm1') -Force

try {
    if ($RunNumber -notmatch '^\d{1,9}$' -or [int]$RunNumber -lt 1 -or [int]$RunNumber -gt 65535) {
        throw 'RunNumber deve ser inteiro entre 1 e 65535.'
    }

    $config = Read-CiCdConfig -Path $ConfigPath
    $recorded = Get-VersionFromFile -Path @(Get-ProjectVersionFiles -Config $config)[0]
    if (-not (Test-SemVer $recorded) -or $recorded.Contains('-')) {
        throw "A versão registrada '$recorded' não é uma versão estável SemVer."
    }

    $headCommit = (& git log -1 --format='%s%x1f%b' 2>&1 | Out-String)
    if ($LASTEXITCODE -ne 0) { throw "Não foi possível ler o commit atual: $headCommit" }
    $headSeparator = $headCommit.IndexOf([char]0x1f)
    if ($headSeparator -lt 0) { throw 'Não foi possível interpretar o commit atual.' }
    $headSubject = $headCommit.Substring(0, $headSeparator).Trim()
    $headBody = $headCommit.Substring($headSeparator + 1)
    if ($headSubject -match '^chore\(main\): release \d+\.\d+\.\d+(?:\s|$)') {
        Write-Output 'Snapshot ignorado: o commit atual é o merge de um Release PR.'
        Write-GitHubOutput -Path $OutputPath -Values @{ publish_snapshot = 'false' }
        exit 0
    }

    $headConventional = [regex]::Match($headSubject, '^(?<type>[a-z]+)(?:\([^)]*\))?(?<breaking>!)?: .+$')
    $headReleaseType = $headConventional.Success -and
        $headConventional.Groups['type'].Value -in @('feat', 'fix', 'deps', 'perf', 'revert')
    $headBreaking = $headConventional.Groups['breaking'].Success -or
        $headBody -match '(?m)^BREAKING[ -]CHANGE:\s'
    $headReleaseAs = $headBody -match '(?mi)^Release-As:\s*\S+\s*$'
    if (-not ($headReleaseType -or $headBreaking -or $headReleaseAs)) {
        Write-Output 'Snapshot ignorado: o commit atual não altera a próxima versão.'
        Write-GitHubOutput -Path $OutputPath -Values @{ publish_snapshot = 'false' }
        exit 0
    }

    $tag = "v$recorded"
    $tagCommit = (& git rev-parse --verify "refs/tags/$tag^{commit}" 2>&1 | Out-String).Trim()
    if ($LASTEXITCODE -ne 0) {
        throw "A tag estável $tag não foi encontrada; não é seguro calcular o snapshot."
    }
    & git merge-base --is-ancestor $tagCommit HEAD
    if ($LASTEXITCODE -ne 0) { throw "A tag estável $tag não é ancestral do commit atual." }

    $commitLog = (& git log --format='%s%x1f%b%x1e' "$tag..HEAD" 2>&1 | Out-String)
    if ($LASTEXITCODE -ne 0) { throw "Não foi possível ler os commits desde $tag." }

    $releaseAsVersions = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $bump = 'none'
    $bumpRank = @{ none = 0; patch = 1; minor = 2; major = 3 }
    $currentParts = $recorded.Split('.')

    foreach ($commit in ($commitLog -split [char]0x1e)) {
        $separator = $commit.IndexOf([char]0x1f)
        if ($separator -lt 0) { continue }
        $subject = $commit.Substring(0, $separator).Trim()
        $body = $commit.Substring($separator + 1)

        foreach ($releaseAs in [regex]::Matches($body, '(?mi)^Release-As:\s*(?<version>\S+)\s*$')) {
            $version = $releaseAs.Groups['version'].Value
            if (-not (Test-SemVer $version) -or $version.Contains('-')) {
                throw "Release-As deve indicar uma versão estável SemVer: '$version'."
            }
            [void]$releaseAsVersions.Add($version)
        }

        $conventional = [regex]::Match($subject, '^(?<type>[a-z]+)(?:\([^)]*\))?(?<breaking>!)?: .+$')
        if (-not $conventional.Success) { continue }

        $type = $conventional.Groups['type'].Value
        $isBreaking = $conventional.Groups['breaking'].Success -or
            $body -match '(?m)^BREAKING[ -]CHANGE:\s'
        $candidate = if ($isBreaking) {
            if ([bigint]$currentParts[0] -eq 0) { 'minor' } else { 'major' }
        }
        else {
            switch ($type) {
                'feat' { 'minor' }
                { $_ -in @('fix', 'deps', 'perf', 'revert') } { 'patch' }
                default { 'none' }
            }
        }
        if ($bumpRank[$candidate] -gt $bumpRank[$bump]) { $bump = $candidate }
    }

    if ($releaseAsVersions.Count -gt 1) {
        throw 'Há mais de uma versão Release-As diferente desde a última release estável.'
    }
    if ($releaseAsVersions.Count -eq 1) {
        $targetVersion = @($releaseAsVersions)[0]
        if ((Compare-SemVer -Left $targetVersion -Right $recorded) -le 0) {
            throw "Release-As $targetVersion deve ser maior que a versão atual $recorded."
        }
        $prefix = $targetVersion
    }
    elseif ($bump -eq 'none') {
        Write-Output "Snapshot ignorado: não há commits releasable desde $tag."
        Write-GitHubOutput -Path $OutputPath -Values @{ publish_snapshot = 'false' }
        exit 0
    }
    else {
        $major = [bigint]$currentParts[0]
        $minor = [bigint]$currentParts[1]
        $patch = [bigint]$currentParts[2]
        $prefix = switch ($bump) {
            'major' { "$($major + 1).0.0" }
            'minor' { "$major.$($minor + 1).0" }
            'patch' { "$major.$minor.$($patch + 1)" }
        }
    }

    $suffix = "preview.$RunNumber"
    Write-GitHubOutput -Path $OutputPath -Values @{
        publish_snapshot = 'true'
        version          = "$prefix-$suffix"
        version_prefix   = $prefix
        version_suffix   = $suffix
        assembly_version = "$($prefix.Split('.')[0]).0.0.0"
        file_version     = "$prefix.$RunNumber"
        file_revision    = $RunNumber
    }
}
catch {
    Write-Output "::error title=Versão de snapshot::$($_.Exception.Message)"
    exit 1
}

exit 0
