#!/usr/bin/env pwsh

$dotnet = 'C:\Program Files\dotnet\dotnet.exe'
$basePath = 'c:\Users\lzob8c1\projetos\Nuuvify.CommonPack'

Write-Host "Testing Middleware.xTest..." -ForegroundColor Cyan
& $dotnet test "$basePath\test\Nuuvify.CommonPack.Middleware.xTest\Nuuvify.CommonPack.Middleware.xTest.csproj" `
    -c Release `
    --logger "console;verbosity=minimal" `
    --no-restore

$midResult = $LASTEXITCODE
Write-Host "`nMiddleware test exit code: $midResult" -ForegroundColor $(if ($midResult -eq 0) { 'Green' } else { 'Red' })

Write-Host "`nTesting Security.xTest..." -ForegroundColor Cyan
& $dotnet test "$basePath\test\Nuuvify.CommonPack.Security.xTest\Nuuvify.CommonPack.Security.xTest.csproj" `
    -c Release `
    --logger "console;verbosity=minimal" `
    --no-restore

$secResult = $LASTEXITCODE
Write-Host "`nSecurity test exit code: $secResult" -ForegroundColor $(if ($secResult -eq 0) { 'Green' } else { 'Red' })

$finalResult = $midResult + $secResult
Write-Host "`n========== FINAL RESULT: $finalResult" -ForegroundColor $(if ($finalResult -eq 0) { 'Green' } else { 'Red' })
exit $finalResult
