#!/bin/pwsh

####################################################################
# Utilizar esse script apenas localmente para testar os pacotes
####################################################################

Clear-Host

# /home/lincoln/projs/Nuuvify.CommonPack/test

$isOsWindows = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform($([System.Runtime.InteropServices.OSPlatform]::Windows))
$soluctionName = "CommonPack"

if ($isOsWindows) {
    $pathPublish = "c:\software\$soluctionName"
}
else {
    $pathPublish = "~/software/$soluctionName"
}

$version = "3.0.1-alpha.1" #VersionPack.ps1 1 $False ../

if ($null -eq $version) {
    Write-Error "Não foi possivel determinar a versão !" 
    Exit 1
}


if (Test-Path $pathPublish) {
    Remove-Item -Path $pathPublish -Recurse -Force
}

Remove-Item -Recurse "../src/*/bin" ; Remove-Item -Recurse "../src/*/obj" ; Remove-Item -Recurse "../test/*/bin" ; Remove-Item -Recurse "../test/*/obj"

dotnet pack "../$soluctionName.sln" -c Release -p:PackageVersion=$version -o $pathPublish 

$pacotes = Get-ChildItem -Filter *.nupkg -LiteralPath $pathPublish 
if ($pacotes.Count -gt 0) {

    # Set-Location $pathPublish
    # # http://timestamp.sectigo.com
    # # https://sectigo.com/resource-library/time-stamping-server

    # foreach ($pacote in $pacotes) {
    #     Write-Host "Signing: $($pacote.Name)"
    #     C:/Users/Lincoln/nuget.exe sign $pacote.Name -CertificatePath C:/Projetos/SecurityFiles/nuget-sign-package.pkt -Timestamper "http://timestamp.sectigo.com" 
    #     Start-Sleep -Seconds 15
    # }
   
    
    #dotnet nuget push --source "TesteLocal" $pathPublish\*.nupkg

    #dotnet nuget push --source "nugetvsts" --api-key PrivateFeed $pathPublish\*.nupkg
    #dotnet nuget push --source https://api.nuget.org/v3/index.json --api-key SuaApiKeyDoNuget $pathPublish\*.nupkg

    Get-ChildItem -Path $pathPublish
    Write-Host "Concluido"
}
else {
    Write-Error "Pacotes nupkg não foram gerados !" 
    Exit 1
}
