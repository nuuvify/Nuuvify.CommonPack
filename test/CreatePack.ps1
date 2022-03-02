#!/bin/pwsh

####################################################################
# Utilizar esse script apenas localmente para testar os pacotes
####################################################################

Clear-Host

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

if (Test-Path "$pathPublish\*.nupkg" -PathType leaf) {

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
