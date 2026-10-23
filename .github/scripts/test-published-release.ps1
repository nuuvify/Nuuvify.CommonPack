[CmdletBinding()]
param(
    [switch]$Help
)

if ($Help -or $args -contains '--help') {
    Write-Output 'Uso: pwsh -File .github/scripts/test-published-release.ps1'
    Write-Output 'Executa testes locais da detecção de releases estáveis já publicadas.'
    exit 0
}

$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'PublishedRelease.psm1') -Force

function Assert-Equal {
    param(
        [object]$Expected,
        [object]$Actual,
        [string]$Scenario
    )

    if ($Expected -ne $Actual) {
        throw "$Scenario`: esperado '$Expected', recebido '$Actual'."
    }
}

$requestCount = 0
$publishedRequest = {
    param($Uri, $Headers)
    $script:requestCount++
    Assert-Equal 'https://api.github.com/repos/nuuvify/Nuuvify.CommonPack/releases/tags/v2.9.0' $Uri 'URI da release'
    Assert-Equal 'Bearer test-token' $Headers.Authorization 'Cabeçalho de autorização'
    return @{ id = 42 }
}

$published = Test-GitHubReleasePublished `
    -Channel stable `
    -Version '2.9.0' `
    -Repository 'nuuvify/Nuuvify.CommonPack' `
    -Token 'test-token' `
    -Request $publishedRequest
Assert-Equal $true $published 'Release existente'
Assert-Equal 1 $requestCount 'Quantidade de consultas da release existente'

$missingRequest = {
    param($Uri, $Headers)
    $exception = [System.Exception]::new('not found')
    $exception | Add-Member -NotePropertyName Response -NotePropertyValue ([pscustomobject]@{ StatusCode = 404 })
    throw $exception
}

$missing = Test-GitHubReleasePublished `
    -Channel stable `
    -Version '2.9.1' `
    -Repository 'nuuvify/Nuuvify.CommonPack' `
    -Token 'test-token' `
    -Request $missingRequest
Assert-Equal $false $missing 'Release inexistente'

$forbiddenRequest = {
    param($Uri, $Headers)
    $exception = [System.Exception]::new('forbidden')
    $exception | Add-Member -NotePropertyName Response -NotePropertyValue ([pscustomobject]@{ StatusCode = 403 })
    throw $exception
}

$apiErrorPropagated = $false
try {
    Test-GitHubReleasePublished `
        -Channel stable `
        -Version '2.9.1' `
        -Repository 'nuuvify/Nuuvify.CommonPack' `
        -Token 'test-token' `
        -Request $forbiddenRequest | Out-Null
}
catch {
    if ($_.Exception.Message -ne 'forbidden') {
        throw
    }

    $apiErrorPropagated = $true
}
Assert-Equal $true $apiErrorPropagated 'Erro da API diferente de 404'

$previewRequest = {
    param($Uri, $Headers)
    throw 'A consulta não deve ocorrer no canal preview.'
}
$preview = Test-GitHubReleasePublished `
    -Channel preview `
    -Version '2.9.0' `
    -Repository 'nuuvify/Nuuvify.CommonPack' `
    -Token 'test-token' `
    -Request $previewRequest
Assert-Equal $false $preview 'Canal preview'

Write-Output 'Todos os testes de releases publicadas passaram.'
