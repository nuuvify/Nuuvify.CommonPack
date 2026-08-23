#!/usr/bin/env pwsh

$dotnet = 'C:\Program Files\dotnet\dotnet.exe'
$basePath = 'c:\Users\lzob8c1\projetos\Nuuvify.CommonPack'
$output = 'c:\Users\lzob8c1\projetos\Nuuvify.CommonPack\test-results.txt'

"Starting tests..." | Tee-Object $output

"Testing Middleware.xTest..." | Tee-Object $output -Append
& $dotnet test "$basePath\test\Nuuvify.CommonPack.Middleware.xTest\Nuuvify.CommonPack.Middleware.xTest.csproj" `
    -c Release `
    --logger "console;verbosity=minimal" `
    --no-restore 2>&1 | Tee-Object $output -Append

$midResult = $LASTEXITCODE
"Middleware test exit code: $midResult" | Tee-Object $output -Append

"Testing Security.xTest..." | Tee-Object $output -Append
& $dotnet test "$basePath\test\Nuuvify.CommonPack.Security.xTest\Nuuvify.CommonPack.Security.xTest.csproj" `
    -c Release `
    --logger "console;verbosity=minimal" `
    --no-restore 2>&1 | Tee-Object $output -Append

$secResult = $LASTEXITCODE
"Security test exit code: $secResult" | Tee-Object $output -Append

$finalResult = $midResult + $secResult
"FINAL RESULT: $finalResult" | Tee-Object $output -Append

exit $finalResult
