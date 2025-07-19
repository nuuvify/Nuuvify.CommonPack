#!/bin/pwsh


[CmdletBinding()]
param(
    [Parameter(Position = 0, ValueFromPipeline)]
    [string]$removeTests,
    [Parameter(Position = 1, ValueFromPipeline)]
    [bool] $GenerateReport = $true,
    [Parameter(Position = 2, ValueFromPipeline)]
    [string] $ProjectTest = "*"

)


function GenerateReportCoverage {

    Write-Host "****************** Generating report in: $dirTests\TestResults\Reports\ *********************"
    Set-Location $dirTests

    $pathReportExe = "dotnet tool run reportgenerator"
    Set-Alias aliasReport $pathReportExe
    
    
    $Instaled = dotnet tool list | Where-Object { $_ -like '*reportgenerator*' }
    
    if ([string]::IsNullOrWhiteSpace($Instaled)) {
        Write-Host "Instaling $pathReportExe"
        dotnet new tool-manifest -o $dirManifest
        dotnet tool install --local dotnet-reportgenerator-globaltool
    }
    else {
        dotnet tool restore
    }

    #aliasReport
    dotnet tool run reportgenerator "-reports:./*/TestResults/*/coverage.*.xml" `
        "-targetdir:TestResults/Reports" `
        "-reportTypes:Html;Badges;SonarQube" `
        "-assemblyfilters:-xunit.*" 

    Write-Host "********************* Report generated *********************"

    if ($env:OS -ilike "Windows*") {
        if (Test-Path $dirTests\TestResults\Reports\index.html -PathType leaf) {
            Start-Process -FilePath $dirTests\TestResults\Reports\index.html
        }
        else {
            Start-Process -FilePath $dirTests\TestResults\Reports\index.htm
        }
    }
    
}


Clear-Host
Write-Host "======================================================= Inicio => $(Get-Date)"
$dirTests = $pwd

if (!($pwd.Path.ToLower().EndsWith("test"))) {
    Write-Host "Não tem testes"
    exit 0
}

$dirManifest = $dirTests.ToString().Replace("test", "")

Get-ChildItem -Path . -Include @('bin', 'obj', 'TestResults') -Directory -Recurse | 
Remove-Item -Path { $_.FullName } -Recurse -Force -Confirm:$false
    
Remove-Item -Recurse "./*/coverage.*.xml"


if (![string]::IsNullOrWhiteSpace($removeTests)) {
    Set-Location $dirTests
    Write-Host "Concluido => $(Get-Date)"
    exit 0
}

Write-Host "Start tests path: $pwd"
Set-Location ..

if ($ProjectTest -eq "*") {

    dotnet test `
        --logger "trx" `
        --logger:"html" `
        --collect:"XPlat Code Coverage" `
        --settings $dirTests/runsettings.xml

}
else {

    $dirProjectTest = [System.IO.Path]::Combine($dirTests, $ProjectTest)

    dotnet test $dirProjectTest `
        --logger "trx" `
        --logger:"html" `
        --collect:"XPlat Code Coverage" `
        --settings $dirTests/runsettings.xml
   
}

Write-Host "Codigo de retorno dos testes: $LASTEXITCODE"

if ($LASTEXITCODE -eq 0) {
    if ($true -eq $GenerateReport) {
        GenerateReportCoverage
    }
}
else {
    Write-Host "Testes com erro !!!"
}


Set-Location $dirTests
Write-Host "Concluido => $(Get-Date)"
