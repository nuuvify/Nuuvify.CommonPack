<#
.SYNOPSIS
    Script para executar testes de unidade e gerar relatório de cobertura de código
.DESCRIPTION
    Este script executa todos os testes de unidade do projeto localizado na pasta test,
    coleta dados de cobertura de código usando coverlet, e gera um relatório HTML
    detalhado utilizando ReportGenerator.
.PARAMETER Configuration
    Configuração de build a ser utilizada (Debug ou Release)
.PARAMETER OutputPath
    Caminho para salvar o relatório de cobertura (padrão: .\TestResults\Coverage)
.PARAMETER Filter
    Filtro para executar apenas testes específicos (opcional)
.PARAMETER NoBuild
    Se especificado, não executa o build antes dos testes
.PARAMETER Verbosity
    Nível de detalhamento do output (quiet, minimal, normal, detailed, diagnostic)
.PARAMETER MinimumCoverage
    Percentual mínimo de cobertura aceitável (padrão: 95)
.PARAMETER Clean
    Se especificado, limpa o diretório de output antes da execução (padrão: $true)
.PARAMETER RecreateTestResults
    Se especificado, remove e recria completamente a pasta TestResults do script (test-automation\TestResults)
    antes de qualquer outra operação. Use quando houver arquivos corrompidos ou travados.
    Este parâmetro tem precedência sobre -Clean para a pasta raiz de TestResults.
.NOTES
    Requisitos:
    - .NET 8 SDK instalado
    - Pacote coverlet.collector nos projetos de teste
    - ReportGenerator tool instalado (dotnet tool install -g dotnet-reportgenerator-globaltool)

    O script verifica automaticamente se o ReportGenerator está instalado e oferece instalá-lo.

    IMPORTANTE: Este arquivo deve ser salvo com encoding UTF-8 with BOM para garantir
    a correta exibição de caracteres especiais no console do PowerShell.
    No VS Code: File > Save with Encoding > UTF-8 with BOM
.EXAMPLE
    .\Test-UnitExecute.ps1
    Executa todos os testes com configuração padrão
.EXAMPLE
    .\Test-UnitExecute.ps1 -Configuration Release -MinimumCoverage 85
    Executa testes em Release exigindo 85% de cobertura mínima
.EXAMPLE
    .\Test-UnitExecute.ps1 -Filter "CBL.MqClient.Domain.Tests" -Verbosity detailed
    Executa apenas testes do projeto Domain com output detalhado
.EXAMPLE
    .\Test-UnitExecute.ps1 -NoBuild
    Executa testes sem fazer rebuild do projeto
.EXAMPLE
    .\Test-UnitExecute.ps1 -RecreateTestResults
    Remove e recria completamente a pasta TestResults antes de executar os testes
#>

[CmdletBinding()]
param (
    [Parameter(Position = 0)]
    [string][ValidateScript(
        {
            if ($_ -eq "--help") {
                $PSDefaultParameterValues.Remove("*:Configuration")
                $PSDefaultParameterValues.Remove("*:OutputPath")
                $PSDefaultParameterValues.Remove("*:Filter")
                $PSDefaultParameterValues.Remove("*:Verbosity")
                $PSDefaultParameterValues.Remove("*:MinimumCoverage")
                $PSDefaultParameterValues.Remove("*:Clean")
                $PSDefaultParameterValues.Remove("*:RecreateTestResults")

                $PSDefaultParameterValues.Add("*:Configuration", "Debug")
                $PSDefaultParameterValues.Add("*:OutputPath", ".\TestResults\Coverage")
                $PSDefaultParameterValues.Add("*:Filter", "")
                $PSDefaultParameterValues.Add("*:Verbosity", "normal")
                $PSDefaultParameterValues.Add("*:MinimumCoverage", 95)
                $PSDefaultParameterValues.Add("*:Clean", $true)
                $PSDefaultParameterValues.Add("*:RecreateTestResults", $false)
                return $true
            }
            return $true
        })] $help,

    [Parameter(Position = 1)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    [Parameter(Position = 2)]
    [string]$OutputPath = ".\TestResults\Coverage",

    [Parameter(Position = 3)]
    [string]$Filter = "",

    [Parameter(Position = 4)]
    [switch]$NoBuild,

    [Parameter(Position = 5)]
    [ValidateSet("quiet", "minimal", "normal", "detailed", "diagnostic")]
    [string]$Verbosity = "normal",

    [Parameter(Position = 6)]
    [ValidateRange(0, 100)]
    [int]$MinimumCoverage = 95,

    [Parameter(Position = 7)]
    [bool]$Clean = $true,

    [Parameter(Position = 8)]
    [switch]$RecreateTestResults
)

# Verificar se --help foi passado como argumento
if ($help -eq "--help") {
    Get-Help $MyInvocation.MyCommand.Definition -Full
    exit 0
}

# Configuração de encoding e console (UTF-8 with BOM)
# IMPORTANTE: Este arquivo deve ser salvo com UTF-8 with BOM
$utf8WithBom = New-Object System.Text.UTF8Encoding $true
[Console]::OutputEncoding = $utf8WithBom
[Console]::InputEncoding = $utf8WithBom
$PSDefaultParameterValues['*:Encoding'] = 'utf8'
$OutputEncoding = $utf8WithBom

# Configuração de cores e comportamento
$ErrorActionPreference = "Continue"
$ProgressPreference = "SilentlyContinue"

# Banner
Write-Host ""
Write-Host "════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "           Execução de Testes de Unidade com Cobertura              " -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Função para exibir mensagens coloridas
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White",
        [switch]$NoNewLine
    )
    if ($NoNewLine) {
        Write-Host $Message -ForegroundColor $Color -NoNewline
    }
    else {
        Write-Host $Message -ForegroundColor $Color
    }
}

# Função para verificar se uma ferramenta está instalada
function Test-CommandExists {
    param([string]$Command)

    $oldPreference = $ErrorActionPreference
    $ErrorActionPreference = 'stop'

    try {
        if (Get-Command $Command -ErrorAction Stop) {
            return $true
        }
    }
    catch {
        return $false
    }
    finally {
        $ErrorActionPreference = $oldPreference
    }
}

# Verificar se dotnet está instalado
if (-not (Test-CommandExists "dotnet")) {
    Write-ColorOutput "ERRO: .NET SDK não está instalado ou não está no PATH." "Red"
    Write-ColorOutput "Baixe e instale o .NET 8 SDK de: https://dotnet.microsoft.com/download" "Yellow"
    exit 1
}

# Verificar se ReportGenerator está instalado
Write-ColorOutput "Verificando ReportGenerator..." "Cyan"
if (-not (Test-CommandExists "reportgenerator")) {
    Write-ColorOutput "ReportGenerator não encontrado." "Yellow"
    $install = Read-Host "Deseja instalar o ReportGenerator agora? (S/N)"

    if ($install -eq "S" -or $install -eq "s") {
        Write-ColorOutput "Instalando ReportGenerator..." "Cyan"
        dotnet tool install -g dotnet-reportgenerator-globaltool

        if ($LASTEXITCODE -ne 0) {
            Write-ColorOutput "ERRO: Falha ao instalar ReportGenerator." "Red"
            exit 1
        }

        Write-ColorOutput "ReportGenerator instalado com sucesso!" "Green"
    }
    else {
        Write-ColorOutput "ERRO: ReportGenerator é necessário para gerar o relatório de cobertura." "Red"
        Write-ColorOutput "Execute: dotnet tool install -g dotnet-reportgenerator-globaltool" "Yellow"
        exit 1
    }
}

# Verificar diretório raiz do projeto
$projectRoot = Split-Path -Parent $PSScriptRoot

# Buscar arquivo .sln na raiz do projeto
Write-ColorOutput "Procurando arquivo de solution (.sln)..." "Cyan"
$solutionFiles = Get-ChildItem -Path $projectRoot -Filter "*.sln" -File

if ($solutionFiles.Count -eq 0) {
    Write-ColorOutput "ERRO: Nenhum arquivo .sln encontrado em $projectRoot" "Red"
    exit 1
}

if ($solutionFiles.Count -gt 1) {
    Write-ColorOutput "AVISO: Múltiplos arquivos .sln encontrados:" "Yellow"
    for ($i = 0; $i -lt $solutionFiles.Count; $i++) {
        Write-ColorOutput "  [$($i + 1)] $($solutionFiles[$i].Name)" "White"
    }

    do {
        $selection = Read-Host "Selecione o número da solution (1-$($solutionFiles.Count))"
        $selectedIndex = [int]$selection - 1
    } while ($selectedIndex -lt 0 -or $selectedIndex -ge $solutionFiles.Count)

    $solutionFile = $solutionFiles[$selectedIndex].FullName
}
else {
    $solutionFile = $solutionFiles[0].FullName
}

Write-ColorOutput "Diretório do projeto: $projectRoot" "Cyan"
Write-ColorOutput "Solution: $($solutionFiles | Where-Object { $_.FullName -eq $solutionFile } | Select-Object -ExpandProperty Name)" "Cyan"
Write-ColorOutput "Configuração: $Configuration" "Cyan"
Write-ColorOutput "Cobertura mínima: $MinimumCoverage%" "Cyan"
Write-Host ""

# Remover e recriar a pasta TestResults raiz do script se -RecreateTestResults for especificado
if ($RecreateTestResults) {
    $testResultsRoot = Join-Path $PSScriptRoot "TestResults"
    Write-ColorOutput "════════════════════════════════════════════════════════════════" "Cyan"
    Write-ColorOutput "Removendo e recriando pasta TestResults..." "Cyan"
    if (Test-Path $testResultsRoot) {
        try {
            Remove-Item -Path $testResultsRoot -Recurse -Force -ErrorAction Stop
            Write-ColorOutput "  ✓ Pasta removida: $testResultsRoot" "Green"
        }
        catch {
            Write-ColorOutput "  ✗ Não foi possível remover $testResultsRoot : $_" "Red"
            exit 1
        }
    }
    New-Item -ItemType Directory -Path $testResultsRoot -Force | Out-Null
    Write-ColorOutput "  ✓ Pasta recriada: $testResultsRoot" "Green"
    Write-ColorOutput "════════════════════════════════════════════════════════════════" "Cyan"
    Write-Host ""
}

# Limpar pastas TestResults do diretório /test/ se Clean estiver habilitado
if ($Clean) {
    $testDir = Join-Path (Split-Path $PSScriptRoot -Parent) "test"
    if (Test-Path $testDir) {
        Write-ColorOutput "Limpando pastas TestResults dos projetos de teste..." "Cyan"
        $testResultsFolders = Get-ChildItem -Path $testDir -Filter "TestResults" -Directory -Recurse -ErrorAction SilentlyContinue
        $removedCount = 0
        foreach ($folder in $testResultsFolders) {
            try {
                Remove-Item -Path $folder.FullName -Recurse -Force -ErrorAction Stop
                Write-ColorOutput "  ✓ Removido: $($folder.FullName)" "Green"
                $removedCount++
            }
            catch {
                Write-ColorOutput "  Aviso: Não foi possível remover $($folder.FullName): $_" "Yellow"
            }
        }
        if ($removedCount -gt 0) {
            Write-ColorOutput "✓ $removedCount pasta(s) TestResults removida(s) com sucesso!" "Green"
        }
        else {
            Write-ColorOutput "Nenhuma pasta TestResults encontrada." "Gray"
        }
        Write-Host ""
    }
}

# Limpar diretório de output se Clean estiver habilitado
$fullOutputPath = Join-Path $PSScriptRoot $OutputPath
if ($Clean -and (Test-Path $fullOutputPath)) {
    Write-ColorOutput "Limpando diretório de output anterior..." "Cyan"
    try {
        # Remover todos os arquivos e subdiretórios
        Get-ChildItem -Path $fullOutputPath -Recurse -Force | Remove-Item -Force -Recurse -ErrorAction Stop
        # Remover o diretório principal
        Remove-Item -Path $fullOutputPath -Force -ErrorAction Stop
        Write-ColorOutput "✓ Diretório limpo com sucesso!" "Green"
    }
    catch {
        Write-ColorOutput "Aviso: Não foi possível limpar completamente o diretório: $_" "Yellow"
        # Tentar limpar arquivos específicos que podem causar problemas
        Get-ChildItem -Path $fullOutputPath -Filter "*.log" -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue
        Get-ChildItem -Path $fullOutputPath -Filter "coverage.cobertura.xml" -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue
    }
    Write-Host ""
}

# Criar diretório de output se não existir
if (-not (Test-Path $fullOutputPath)) {
    New-Item -ItemType Directory -Path $fullOutputPath -Force | Out-Null
}

# Garantir que o arquivo temporário de log não existe de execuções anteriores
$tempLogFile = Join-Path $fullOutputPath "test-output.log"
if (Test-Path $tempLogFile) {
    Remove-Item $tempLogFile -Force -ErrorAction SilentlyContinue
}

# Construir comando de teste
$testCommand = "dotnet test `"$solutionFile`""
$testCommand += " --configuration $Configuration"
$testCommand += " --verbosity $Verbosity"

if ($NoBuild) {
    $testCommand += " --no-build"
}

if ($Filter) {
    $testCommand += " --filter `"$Filter`""
}

# Adicionar coleta de cobertura
$coverageFile = Join-Path $fullOutputPath "coverage.cobertura.xml"
$testCommand += " --collect:`"XPlat Code Coverage`""
$testCommand += " --results-directory `"$fullOutputPath`""
$testCommand += " --settings `"$(Join-Path $projectRoot 'test.runsettings')`""

Write-ColorOutput "════════════════════════════════════════════════════════════════" "Cyan"
Write-ColorOutput "Executando Testes de Unidade..." "Cyan"
Write-ColorOutput "════════════════════════════════════════════════════════════════" "Cyan"
Write-Host ""

# Executar testes e capturar output em arquivo temporário
$startTime = Get-Date
$tempLogFile = Join-Path $fullOutputPath "test-output.log"

# Executar comando e salvar output em arquivo
Invoke-Expression $testCommand | Tee-Object -FilePath $tempLogFile
$testExitCode = $LASTEXITCODE

$endTime = Get-Date
$duration = $endTime - $startTime

Write-Host ""
Write-ColorOutput "════════════════════════════════════════════════════════════════" "Cyan"

# Extrair estatísticas dos testes do arquivo de log
$totalTests = 0
$passedTests = 0
$failedTests = 0
$skippedTests = 0
$assemblyTestStats = @()

if (Test-Path $tempLogFile) {
    $outputString = Get-Content $tempLogFile -Raw -Encoding UTF8

    # Extrair estatísticas por assembly
    $lines = $outputString -split "`r?`n"

    # Primeiro, procurar pelo padrão de resumo inline com nome do assembly
    # Formato: "Aprovado! – Com falha: 0, Aprovado: 396, Ignorado: 0, Total: 396, Duração: 1 s - CBL.SynchroNfe.Domain.Tests.dll (net8.0)"
    foreach ($line in $lines) {
        if ($line -match "(Aprovado!|Com falha!)\s*[–-]\s*Com falha:\s*(\d+),\s*Aprovado:\s*(\d+),\s*Ignorado:\s*(\d+),\s*Total:\s*(\d+),.*?\s+-\s+([\w.]+)\.dll") {
            $assemblyName = $matches[6]
            $testStats = @{
                Assembly = $assemblyName
                Failed   = [int]$matches[2]
                Passed   = [int]$matches[3]
                Skipped  = [int]$matches[4]
                Total    = [int]$matches[5]
            }

            # Adicionar apenas se não foi adicionado ainda
            $exists = $assemblyTestStats | Where-Object { $_.Assembly -eq $assemblyName }
            if (-not $exists -and $testStats.Total -gt 0) {
                $assemblyTestStats += [PSCustomObject]$testStats
            }
        }
    }

    # Estratégia alternativa: Procurar por blocos de "Total de testes" seguidos de "Projeto de compilação pronto" com .Tests
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]

        # Procurar por "Total de testes" que não seja o resumo final
        if ($line -match "^Total de testes:\s*(\d+)|^Total tests:\s*(\d+)") {
            $total = if ($matches[1]) { [int]$matches[1] } else { [int]$matches[2] }

            # Verificar se NÃO é o resumo final (que aparece antes de "Tempo Decorrido")
            $isFinalSummary = $false
            for ($k = $i + 1; $k -lt [Math]::Min($i + 8, $lines.Count); $k++) {
                if ($lines[$k] -match "Tempo Decorrido|FALHA da compila[çc][ãa]o.*\.sln") {
                    $isFinalSummary = $true
                    break
                }
            }

            if ($isFinalSummary) {
                continue
            }

            # Capturar as estatísticas nas próximas linhas
            $testStats = @{
                Total    = $total
                Passed   = 0
                Failed   = 0
                Skipped  = 0
                Assembly = $null
            }

            # Ler próximas linhas para capturar aprovados, com falha, ignorados e assembly
            for ($j = $i + 1; $j -lt [Math]::Min($i + 10, $lines.Count); $j++) {
                $nextLine = $lines[$j]

                if ($nextLine -match "^\s+Aprovados:\s*(\d+)|^\s+Passed:\s*(\d+)") {
                    $testStats.Passed = if ($matches[1]) { [int]$matches[1] } else { [int]$matches[2] }
                }
                elseif ($nextLine -match "^\s+Com falha:\s*(\d+)|^\s+Failed:\s*(\d+)") {
                    $testStats.Failed = if ($matches[1]) { [int]$matches[1] } else { [int]$matches[2] }
                }
                elseif ($nextLine -match "^\s+Ignorado[s]?:\s*(\d+)|^\s+Skipped:\s*(\d+)") {
                    $testStats.Skipped = if ($matches[1]) { [int]$matches[1] } else { [int]$matches[2] }
                }
                elseif ($nextLine -match "Projeto de compila[çc][ãa]o pronto.*\\(([\w.]+\.Tests)\\).*\.csproj.*VSTest|Build completed.*\\(([\w.]+\.Tests)\\).*\.csproj.*VSTest") {
                    # Extrair nome do assembly (primeiro grupo de captura)
                    if ($matches[1]) {
                        $testStats.Assembly = $matches[1]
                    }
                    elseif ($matches[2]) {
                        $testStats.Assembly = $matches[2]
                    }

                    # Adicionar estatísticas
                    if ($testStats.Assembly -and $testStats.Total -gt 0) {
                        $assemblyTestStats += [PSCustomObject]@{
                            Assembly = $testStats.Assembly
                            Failed   = $testStats.Failed
                            Passed   = $testStats.Passed
                            Skipped  = $testStats.Skipped
                            Total    = $testStats.Total
                        }
                    }
                    break
                }
            }
        }
    }

    # Padrões para capturar estatísticas em português (formato completo)
    if ($outputString -match "Aprovado!\s*-\s*Com falha:\s*(\d+),\s*Aprovado:\s*(\d+),\s*Ignorado:\s*(\d+),\s*Total:\s*(\d+)") {
        $failedTests = [int]$matches[1]
        $passedTests = [int]$matches[2]
        $skippedTests = [int]$matches[3]
        $totalTests = [int]$matches[4]
    }
    elseif ($outputString -match "Com falha!\s*-\s*Com falha:\s*(\d+),\s*Aprovado:\s*(\d+),\s*Ignorado:\s*(\d+),\s*Total:\s*(\d+)") {
        $failedTests = [int]$matches[1]
        $passedTests = [int]$matches[2]
        $skippedTests = [int]$matches[3]
        $totalTests = [int]$matches[4]
    }
    # Padrões para capturar estatísticas em inglês (formato completo)
    elseif ($outputString -match "Passed!\s*-\s*Failed:\s*(\d+),\s*Passed:\s*(\d+),\s*Skipped:\s*(\d+),\s*Total:\s*(\d+)") {
        $failedTests = [int]$matches[1]
        $passedTests = [int]$matches[2]
        $skippedTests = [int]$matches[3]
        $totalTests = [int]$matches[4]
    }
    elseif ($outputString -match "Failed!\s*-\s*Failed:\s*(\d+),\s*Passed:\s*(\d+),\s*Skipped:\s*(\d+),\s*Total:\s*(\d+)") {
        $failedTests = [int]$matches[1]
        $passedTests = [int]$matches[2]
        $skippedTests = [int]$matches[3]
        $totalTests = [int]$matches[4]
    }
    # Padrão alternativo para formato de múltiplas linhas (português)
    else {
        # Capturar "Total de testes: 138" (não "Tempo total:")
        if ($outputString -match "Total de testes:\s*(\d+)") {
            $totalTests = [int]$matches[1]
        }
        # Se não encontrou, tentar padrão simples (mas evitar "Tempo total")
        elseif ($outputString -match "(?<!Tempo )\bTotal:\s*(\d+)") {
            $totalTests = [int]$matches[1]
        }

        # Tentar extrair aprovados
        if ($outputString -match "\s+Aprovado[s]?:\s*(\d+)") {
            $passedTests = [int]$matches[1]
        }
        elseif ($outputString -match "\s+Passed:\s*(\d+)") {
            $passedTests = [int]$matches[1]
        }

        # Tentar extrair falhas
        if ($outputString -match "\s+Com falha:\s*(\d+)") {
            $failedTests = [int]$matches[1]
        }
        elseif ($outputString -match "\s+Failed:\s*(\d+)") {
            $failedTests = [int]$matches[1]
        }

        # Tentar extrair ignorados
        if ($outputString -match "\s+Ignorado[s]?:\s*(\d+)") {
            $skippedTests = [int]$matches[1]
        }
        elseif ($outputString -match "\s+Skipped:\s*(\d+)") {
            $skippedTests = [int]$matches[1]
        }
    }
}

if ($testExitCode -eq 0) {
    Write-ColorOutput "✓ Testes executados com sucesso!" "Green"
}
else {
    Write-ColorOutput "✗ Alguns testes falharam (Exit Code: $testExitCode)" "Red"
}

Write-ColorOutput "Tempo de execução: $($duration.ToString('mm\:ss\.fff'))" "Cyan"

# Exibir estatísticas de testes se disponíveis
if ($totalTests -gt 0) {
    Write-Host ""
    Write-ColorOutput "Estatísticas dos Testes:" "Cyan"
    Write-ColorOutput "  Total de testes: $totalTests" "White"

    if ($passedTests -gt 0) {
        Write-ColorOutput "  Aprovados: $passedTests" "Green"
    }

    if ($failedTests -gt 0) {
        Write-ColorOutput "  Falharam: $failedTests" "Red"
    }

    if ($skippedTests -gt 0) {
        Write-ColorOutput "  Ignorados: $skippedTests" "Yellow"
    }
}

# Limpar arquivo temporário
if (Test-Path $tempLogFile) {
    Remove-Item $tempLogFile -Force -ErrorAction SilentlyContinue
}

Write-Host ""

# Encontrar arquivos de cobertura gerados
$coverageFiles = Get-ChildItem -Path $fullOutputPath -Filter "coverage.cobertura.xml" -Recurse
if ($coverageFiles.Count -eq 0) {
    Write-ColorOutput "AVISO: Nenhum arquivo de cobertura encontrado." "Yellow"
    Write-ColorOutput "Verifique se o pacote coverlet.collector está instalado nos projetos de teste." "Yellow"
    exit $testExitCode
}

Write-ColorOutput "Encontrados $($coverageFiles.Count) arquivo(s) de cobertura." "Cyan"

# Combinar todos os arquivos de cobertura
$allCoverageFiles = ($coverageFiles | ForEach-Object { $_.FullName }) -join ";"

Write-ColorOutput "════════════════════════════════════════════════════════════════" "Cyan"
Write-ColorOutput "Gerando Relatório de Cobertura..." "Cyan"
Write-ColorOutput "════════════════════════════════════════════════════════════════" "Cyan"
Write-Host ""

# Gerar relatório HTML
$reportPath = Join-Path $fullOutputPath "Report"

# Usar array de argumentos para evitar problemas com parsing do PowerShell
$reportArgs = @(
    "-reports:$allCoverageFiles",
    "-targetdir:$reportPath",
    "-reporttypes:Html;HtmlSummary;Badges;TextSummary",
    "-verbosity:Info"
)

& reportgenerator $reportArgs

if ($LASTEXITCODE -ne 0) {
    Write-ColorOutput "ERRO: Falha ao gerar relatório de cobertura." "Red"
    exit 1
}

Write-Host ""
Write-ColorOutput "✓ Relatório de cobertura gerado com sucesso!" "Green"
Write-ColorOutput "Localização: $reportPath" "Cyan"
Write-Host ""

# Adicionar estatísticas de testes ao Summary.txt
$summaryFile = Join-Path $reportPath "Summary.txt"

if ((Test-Path $summaryFile) -and ($assemblyTestStats.Count -gt 0)) {
    # Calcular totais gerais somando todos os assemblies
    $totalGeralTests = 0
    $totalGeralPassed = 0
    $totalGeralFailed = 0
    $totalGeralSkipped = 0

    # Construir string de estatísticas de testes
    $testSummaryLines = @()
    $testSummaryLines += ""
    $testSummaryLines += "Test Results by Assembly"
    $testSummaryLines += "========================"

    foreach ($stat in $assemblyTestStats) {
        $line = "  $($stat.Assembly): Total: $($stat.Total), Passed: $($stat.Passed), Failed: $($stat.Failed)"
        if ($stat.Skipped -gt 0) {
            $line += ", Skipped: $($stat.Skipped)"
        }
        $testSummaryLines += $line

        # Acumular totais
        $totalGeralTests += $stat.Total
        $totalGeralPassed += $stat.Passed
        $totalGeralFailed += $stat.Failed
        $totalGeralSkipped += $stat.Skipped
    }

    # Adicionar linha separadora e totais
    $testSummaryLines += "  " + ("=" * 80)
    $totalLine = "  Total: $totalGeralTests tests, Passed: $totalGeralPassed, Failed: $totalGeralFailed"
    if ($totalGeralSkipped -gt 0) {
        $totalLine += ", Skipped: $totalGeralSkipped"
    }
    $testSummaryLines += $totalLine
    $testSummaryLines += ""

    # Adicionar ao arquivo Summary.txt
    Add-Content -Path $summaryFile -Value $testSummaryLines -Encoding UTF8
}

# Ler o resumo de cobertura
if (Test-Path $summaryFile) {
    Write-ColorOutput "════════════════════════════════════════════════════════════════" "Cyan"
    Write-ColorOutput "Resumo de Cobertura:" "Cyan"
    Write-ColorOutput "════════════════════════════════════════════════════════════════" "Cyan"

    $summary = Get-Content $summaryFile -Raw
    Write-Host $summary

    # Extrair percentual de cobertura de linha
    if ($summary -match "Line coverage:\s+([\d.]+)%") {
        $coveragePercent = [double]$matches[1]
        Write-Host ""

        if ($coveragePercent -ge $MinimumCoverage) {
            Write-ColorOutput "✓ Cobertura de código: $coveragePercent% (Mínimo: $MinimumCoverage%)" "Green"
        }
        else {
            Write-ColorOutput "✗ Cobertura de código: $coveragePercent% (Mínimo: $MinimumCoverage%)" "Red"
            Write-ColorOutput "AVISO: Cobertura abaixo do mínimo aceitável!" "Yellow"
        }
    }
}

Write-Host ""
Write-ColorOutput "════════════════════════════════════════════════════════════════" "Cyan"
Write-ColorOutput "Execução Concluída!" "Green"
Write-ColorOutput "════════════════════════════════════════════════════════════════" "Cyan"
Write-Host ""

# Abrir relatório no navegador
$indexFile = Join-Path $reportPath "index.html"
if (Test-Path $indexFile) {
    $openReport = Read-Host "Deseja abrir o relatório no navegador? (S/N)"
    if ($openReport -eq "S" -or $openReport -eq "s") {
        Start-Process $indexFile
    }
}

Write-ColorOutput "Para visualizar o relatório posteriormente, abra:" "Cyan"
Write-ColorOutput $indexFile "Yellow"
Write-Host ""

exit $testExitCode
