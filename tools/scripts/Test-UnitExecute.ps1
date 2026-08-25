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
.PARAMETER TestProject
    Caminho opcional, absoluto ou relativo à raiz, de um projeto de testes específico.
    Quando informado, substitui a solution e limita a cobertura ao pacote exercitado.
.PARAMETER TestCategory
    Categoria de testes a executar (All, Unit ou Integration)
.PARAMETER IntegrationContainers
    Controla os containers auxiliares dos testes de integração:
    Auto usa Testcontainers quando Docker está disponível e preserva o fallback dos fixtures;
    Required exige Docker e impede fallback silencioso; Disabled desabilita Testcontainers.
.PARAMETER NoBuild
    Se especificado, não executa o build antes dos testes
.PARAMETER Verbosity
    Nível de detalhamento do output (quiet, minimal, normal, detailed, diagnostic)
.PARAMETER MinimumCoverage
    Percentual mínimo de cobertura aceitável (padrão: 95)
.PARAMETER Clean
    Se especificado, limpa o diretório de output antes da execução (padrão: $true)
.PARAMETER OpenReport
    Abre o relatório de cobertura no navegador ao final da execução.
    Padrão: $true. Quando executado sem console interativo (CI, pipelines), o relatório não é aberto independentemente deste parâmetro.
.PARAMETER LogOnlyFailedTests
    Define se o arquivo de log persistente deve conter apenas testes com falha.
    Padrão: $true. Quando $false, o log completo da execução é persistido.
.PARAMETER RecreateTestResults
    Se especificado, remove e recria completamente a pasta TestResults do script (test-automation\TestResults)
    antes de qualquer outra operação. Use quando houver arquivos corrompidos ou travados.
    Este parâmetro tem precedência sobre -Clean para a pasta raiz de TestResults.
.NOTES
    Requisitos:
    - .NET 8 SDK instalado
    - Pacote coverlet.collector nos projetos de teste
    - Docker em execução quando -IntegrationContainers Required for usado
    - Ferramentas locais restauráveis pelo manifesto .config/dotnet-tools.json

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
    .\Test-UnitExecute.ps1 -TestCategory Unit
    Executa apenas testes com Trait Category=Unit
.EXAMPLE
    .\Test-UnitExecute.ps1 -TestCategory Integration
    Executa apenas testes com Trait Category=Integration e usa Testcontainers quando Docker está disponível
.EXAMPLE
    .\Test-UnitExecute.ps1 -TestCategory Integration -IntegrationContainers Required
    Exige Docker e executa os testes de integração com os containers auxiliares
.EXAMPLE
    .\Test-UnitExecute.ps1 -NoBuild
    Executa testes sem fazer rebuild do projeto
.EXAMPLE
    .\Test-UnitExecute.ps1 -RecreateTestResults
    Remove e recria completamente a pasta TestResults antes de executar os testes
.EXAMPLE
    .\Test-UnitExecute.ps1 -OpenReport:$false
    Executa os testes sem abrir o relatório no navegador ao final
.EXAMPLE
    .\Test-UnitExecute.ps1 -LogOnlyFailedTests:$false
    Persiste o log completo da execução, em vez de somente falhas
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
                $PSDefaultParameterValues.Remove("*:TestProject")
                $PSDefaultParameterValues.Remove("*:TestCategory")
                $PSDefaultParameterValues.Remove("*:IntegrationContainers")
                $PSDefaultParameterValues.Remove("*:Verbosity")
                $PSDefaultParameterValues.Remove("*:MinimumCoverage")
                $PSDefaultParameterValues.Remove("*:Clean")
                $PSDefaultParameterValues.Remove("*:RecreateTestResults")

                $PSDefaultParameterValues.Add("*:Configuration", "Debug")
                $PSDefaultParameterValues.Add("*:OutputPath", ".\TestResults\Coverage")
                $PSDefaultParameterValues.Add("*:Filter", "")
                $PSDefaultParameterValues.Add("*:TestProject", "")
                $PSDefaultParameterValues.Add("*:TestCategory", "All")
                $PSDefaultParameterValues.Add("*:IntegrationContainers", "Auto")
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
    [string]$TestProject = "",

    [Parameter(Position = 5)]
    [ValidateSet("All", "Unit", "Integration")]
    [string]$TestCategory = "Unit",

    [Parameter(Position = 6)]
    [switch]$NoBuild,

    [Parameter(Position = 7)]
    [ValidateSet("quiet", "minimal", "normal", "detailed", "diagnostic")]
    [string]$Verbosity = "normal",

    [Parameter(Position = 8)]
    [ValidateRange(0, 100)]
    [int]$MinimumCoverage = 95,

    [Parameter(Position = 9)]
    [bool]$Clean = $true,

    [Parameter(Position = 10)]
    [switch]$RecreateTestResults,

    [Parameter(Position = 11)]
    [bool]$OpenReport = $true,

    [Parameter(Position = 12)]
    [bool]$LogOnlyFailedTests = $true,

    [Parameter(Position = 13)]
    [ValidateSet("Auto", "Required", "Disabled")]
    [string]$IntegrationContainers = "Auto"
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

function Get-ProjectRoot {
    param(
        [Parameter(Mandatory = $true)]
        [string]$StartPath
    )

    $currentPath = (Resolve-Path -LiteralPath $StartPath).Path

    while ($true) {
        $slnFiles = @(Get-ChildItem -LiteralPath $currentPath -Filter "*.sln" -ErrorAction SilentlyContinue)
        $slxFiles = @(Get-ChildItem -LiteralPath $currentPath -Filter "*.slx" -ErrorAction SilentlyContinue)

        if ($slnFiles.Count -gt 0 -or $slxFiles.Count -gt 0) {
            return $currentPath
        }

        $parentPath = Split-Path -Parent $currentPath
        if ([string]::IsNullOrWhiteSpace($parentPath) -or $parentPath -eq $currentPath) {
            break
        }

        $currentPath = $parentPath
    }

    throw "Não foi possível localizar a raiz do projeto a partir de '$StartPath'."
}

function Resolve-RelativePathFromBase {
    param(
        [Parameter(Mandatory = $true)]
        [string]$BasePath,

        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return $Path
    }

    $normalizedPath = $Path.TrimStart('.').TrimStart('\').TrimStart('/')
    return Join-Path $BasePath $normalizedPath
}

function Get-FailedTestsLogContent {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RawOutput
    )

    if ([string]::IsNullOrWhiteSpace($RawOutput)) {
        return "Sem saída de execução para processar."
    }

    $lines = $RawOutput -split "`r?`n"
    $filteredLines = New-Object System.Collections.Generic.List[string]
    $capturedAny = $false
    $captureFailureDetails = $false
    $captureBuildErrors = $false

    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]

        # Padrões de erros de compilação/build
        $isBuildErrorLine = $line -match ":\s*error\s+[A-Z]+\d+:"
        $isBuildFailureLine = $line -match "(FALHA da compila[çc][ãa]o|Build FAILED|\d+\s+Erro\(s\))"

        if ($isBuildErrorLine) {
            if (-not $captureBuildErrors) {
                if ($filteredLines.Count -gt 0 -and $filteredLines[$filteredLines.Count - 1] -ne "") {
                    $filteredLines.Add("")
                }
                $filteredLines.Add("[Erros de compilação]")
                # Capturar até 10 linhas anteriores para contexto do projeto/destino
                $contextStart = [Math]::Max(0, $i - 10)
                for ($k = $contextStart; $k -lt $i; $k++) {
                    if ($lines[$k] -match '"[^"]+\.(?:csproj|sln)"' -or $lines[$k] -match '->\s*$' -or $lines[$k] -match '\(\w[\w\s]+destino\)') {
                        $filteredLines.Add($lines[$k])
                    }
                }
            }
            $filteredLines.Add($line)
            $capturedAny = $true
            $captureBuildErrors = $true
            $captureFailureDetails = $false
            continue
        }

        if ($isBuildFailureLine) {
            if ($filteredLines.Count -gt 0 -and $filteredLines[$filteredLines.Count - 1] -ne "") {
                $filteredLines.Add("")
            }
            $filteredLines.Add($line)
            $capturedAny = $true
            $captureBuildErrors = $false
            continue
        }

        if ($captureBuildErrors) {
            # Capturar linhas de contagem de avisos/erros (ex.: "    4 Aviso(s)")
            if ($line -match "^\s+\d+\s+(Aviso|Warning|Erro|Error)") {
                $filteredLines.Add($line)
                continue
            }
            # Parar captura em linha em branco
            if ([string]::IsNullOrWhiteSpace($line)) {
                $filteredLines.Add("")
                $captureBuildErrors = $false
                continue
            }
            # Continuar capturando linhas de erro adicionais do mesmo bloco
            if ($line -match ":\s*error\s+[A-Z]+\d+:" -or $line -match "^\s+") {
                $filteredLines.Add($line)
                continue
            }
            $captureBuildErrors = $false
        }

        $isFailedTestLine = $line -match "^\s*(Com falha|Failed|FALHOU)\s+.+\[[^\]]+\]\s*$"
        $isFailureSummaryLine = $line -match "(Com falha!|Failed!)\s*[–-]\s*(Com falha|Failed):\s*\d+"

        if ($isFailedTestLine) {
            if ($filteredLines.Count -gt 0 -and $filteredLines[$filteredLines.Count - 1] -ne "") {
                $filteredLines.Add("")
            }

            $filteredLines.Add($line)
            $capturedAny = $true
            $captureFailureDetails = $true
            continue
        }

        if ($isFailureSummaryLine) {
            if ($filteredLines.Count -gt 0 -and $filteredLines[$filteredLines.Count - 1] -ne "") {
                $filteredLines.Add("")
            }

            $filteredLines.Add($line)
            $capturedAny = $true
            continue
        }

        if ($captureFailureDetails) {
            $isDetailLine =
            ($line -match "^\s+") -or
            ($line -match "^(Error Message:|Mensagem de erro:|Stack Trace:|Rastreamento de pilha:|Expected:|Actual:|Esperado:|Atual:)")

            if ($isDetailLine) {
                $filteredLines.Add($line)
                continue
            }

            if ([string]::IsNullOrWhiteSpace($line)) {
                $filteredLines.Add("")
                $captureFailureDetails = $false
                continue
            }

            $captureFailureDetails = $false
        }
    }

    if (-not $capturedAny) {
        if ($RawOutput -match "(Com falha!|Failed!)\s*[–-]\s*(Com falha|Failed):\s*0") {
            return "Nenhum teste com falha foi encontrado nesta execução."
        }

        return "Nenhum bloco de falha pôde ser extraído do output. Para persistir saída completa, use -LogOnlyFailedTests `$false."
    }

    return ($filteredLines -join [Environment]::NewLine).Trim()
}

# Verificar se dotnet está instalado
if (-not (Test-CommandExists "dotnet")) {
    Write-ColorOutput "ERRO: .NET SDK não está instalado ou não está no PATH." "Red"
    Write-ColorOutput "Baixe e instale o .NET 8 SDK de: https://dotnet.microsoft.com/download" "Yellow"
    exit 1
}

# Verificar diretório raiz do projeto
$projectRoot = Get-ProjectRoot -StartPath $PSScriptRoot
$testRoot = Join-Path $projectRoot "test"

# Restaurar ferramentas locais sem prompt interativo
Write-ColorOutput "Restaurando ferramentas locais..." "Cyan"
& dotnet tool restore --tool-manifest (Join-Path $projectRoot ".config\dotnet-tools.json")
if ($LASTEXITCODE -ne 0) {
    Write-ColorOutput "ERRO: Falha ao restaurar as ferramentas locais. Execute novamente com acesso ao feed NuGet." "Red"
    exit 1
}

# Resolver a solution ou o projeto de testes informado
$solutionFiles = @()
$testTarget = $null
if (-not [string]::IsNullOrWhiteSpace($TestProject)) {
    $testTarget = Resolve-RelativePathFromBase -BasePath $projectRoot -Path $TestProject
    if (-not (Test-Path $testTarget -PathType Leaf) -or [IO.Path]::GetExtension($testTarget) -ne ".csproj") {
        Write-ColorOutput "ERRO: TestProject deve apontar para um arquivo .csproj existente: $testTarget" "Red"
        exit 1
    }
    Write-ColorOutput "Projeto de testes: $testTarget" "Cyan"
}
else {
    Write-ColorOutput "Procurando arquivo de solution (.sln)..." "Cyan"
    $solutionFiles = Get-ChildItem -Path $projectRoot -Filter "*.sln" -File

    if ($solutionFiles.Count -eq 0) {
        Write-ColorOutput "ERRO: Nenhum arquivo .sln encontrado em $projectRoot" "Red"
        exit 1
    }
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
if ($solutionFile) {
    Write-ColorOutput "Solution: $($solutionFiles | Where-Object { $_.FullName -eq $solutionFile } | Select-Object -ExpandProperty Name)" "Cyan"
}
Write-ColorOutput "Configuração: $Configuration" "Cyan"
Write-ColorOutput "Categoria de teste: $TestCategory" "Cyan"
Write-ColorOutput "Containers de integração: $IntegrationContainers" "Cyan"
Write-ColorOutput "Cobertura mínima: $MinimumCoverage%" "Cyan"
Write-Host ""

$runsIntegrationTests = $TestCategory -in @("All", "Integration")
$testcontainersModeVariable = "NUUVIFY_TESTCONTAINERS_MODE"
$previousTestcontainersMode = [Environment]::GetEnvironmentVariable($testcontainersModeVariable, "Process")

if ($runsIntegrationTests -and $IntegrationContainers -ne "Disabled") {
    $dockerAvailable = Test-CommandExists "docker"
    if ($dockerAvailable) {
        & docker info *> $null
        $dockerAvailable = $LASTEXITCODE -eq 0
    }

    if (-not $dockerAvailable -and $IntegrationContainers -eq "Required") {
        Write-ColorOutput "ERRO: Docker não está disponível. Inicie o Docker e tente novamente ou use -IntegrationContainers Auto/Disabled." "Red"
        exit 1
    }

    if ($dockerAvailable) {
        Write-ColorOutput "Docker disponível; os fixtures iniciarão e removerão os containers via Testcontainers." "Green"
    }
    else {
        Write-ColorOutput "AVISO: Docker indisponível; fixtures em modo Auto podem usar o fallback configurado." "Yellow"
    }
}

if ($runsIntegrationTests) {
    [Environment]::SetEnvironmentVariable($testcontainersModeVariable, $IntegrationContainers, "Process")
}

# Remover e recriar a pasta TestResults raiz do script se -RecreateTestResults for especificado
$testResultsRoot = Join-Path $testRoot "TestResults"
$runLockPath = Join-Path $testRoot ".coverage-run.lock"
$runLockStream = $null
try {
    $runLockStream = [System.IO.File]::Open($runLockPath, [System.IO.FileMode]::OpenOrCreate, [System.IO.FileAccess]::ReadWrite, [System.IO.FileShare]::None)
}
catch {
    Write-ColorOutput "ERRO: Já existe uma execução de cobertura em andamento. Aguarde a conclusão antes de iniciar outra." "Red"
    exit 3
}

function Release-RunLock {
    if ($script:runLockStream) {
        $script:runLockStream.Dispose()
        $script:runLockStream = $null
    }

    if (Test-Path -LiteralPath $runLockPath) {
        Remove-Item -LiteralPath $runLockPath -Force -ErrorAction SilentlyContinue
    }
}

try {
if ($RecreateTestResults) {
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

# Limpar diretório de output se Clean estiver habilitado
$fullOutputPath = Resolve-RelativePathFromBase -BasePath $testRoot -Path $OutputPath

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

# Criar diretório persistente para logs completos de execução
$executionLogPath = Join-Path $testRoot "TestResults\ExecutionLogs"
if (-not (Test-Path $executionLogPath)) {
    New-Item -ItemType Directory -Path $executionLogPath -Force | Out-Null
}

# Garantir que o arquivo temporário de log não existe de execuções anteriores
$tempLogFile = Join-Path $fullOutputPath "test-output.log"
if (Test-Path $tempLogFile) {
    Remove-Item $tempLogFile -Force -ErrorAction SilentlyContinue
}

# Construir comando de teste
$testTarget ??= $solutionFile
$testArguments = @(
    "test",
    $testTarget,
    "--configuration",
    $Configuration,
    "--verbosity",
    $Verbosity
)

if ($NoBuild) {
    $testArguments += "--no-build"
}

$effectiveFilter = $Filter

$categoryFilter = $null
switch ($TestCategory) {
    "Unit" { $categoryFilter = 'Category=Unit' }
    "Integration" { $categoryFilter = 'Category=Integration' }
    default { $categoryFilter = $null }
}

if (-not [string]::IsNullOrWhiteSpace($categoryFilter)) {
    if ([string]::IsNullOrWhiteSpace($effectiveFilter)) {
        $effectiveFilter = $categoryFilter
    }
    else {
        $effectiveFilter = "($effectiveFilter)&($categoryFilter)"
    }
}

if (-not [string]::IsNullOrWhiteSpace($effectiveFilter)) {
    $testArguments += @("--filter", $effectiveFilter)
}

# Adicionar coleta de cobertura
$coverageFile = Join-Path $fullOutputPath "coverage.cobertura.xml"
$testArguments += @(
    "--collect:XPlat Code Coverage",
    "--results-directory",
    $fullOutputPath,
    "--settings",
    (Join-Path $projectRoot "test.runsettings.xml")
)

Write-ColorOutput "════════════════════════════════════════════════════════════════" "Cyan"
Write-ColorOutput "Executando Testes..." "Cyan"
Write-ColorOutput "════════════════════════════════════════════════════════════════" "Cyan"
Write-Host ""

# Executar testes e capturar output em arquivo temporário
$startTime = Get-Date
$tempLogFile = Join-Path $fullOutputPath "test-output.log"

# Executar comando e salvar output em arquivo
# Redirecionar stderr para stdout (2>&1) garante que erros de build/compilação
# sejam capturados no log, não apenas exibidos no console.
$PSNativeCommandUseErrorActionPreference = $false
try {
    & dotnet @testArguments 2>&1 | Tee-Object -FilePath $tempLogFile
    $testExitCode = $LASTEXITCODE
}
finally {
    [Environment]::SetEnvironmentVariable($testcontainersModeVariable, $previousTestcontainersMode, "Process")
}

$rawExecutionOutput = ""
if (Test-Path $tempLogFile) {
    $rawExecutionOutput = Get-Content $tempLogFile -Raw -Encoding UTF8
}

# Persistir o log completo da execução em uma pasta estável para facilitar a investigação de falhas
$runTimestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$persistentLogFile = Join-Path $executionLogPath "test-output-$runTimestamp.log"
if (Test-Path $tempLogFile) {
    if ($LogOnlyFailedTests) {
        $failedTestsLogContent = Get-FailedTestsLogContent -RawOutput $rawExecutionOutput
        Set-Content -Path $persistentLogFile -Value $failedTestsLogContent -Encoding UTF8
    }
    else {
        Copy-Item -Path $tempLogFile -Destination $persistentLogFile -Force
    }
}

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
    $outputString = if ([string]::IsNullOrWhiteSpace($rawExecutionOutput)) {
        Get-Content $tempLogFile -Raw -Encoding UTF8
    }
    else {
        $rawExecutionOutput
    }

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
    if ($outputString -match "Aprovado!\s*[–-]\s*Com falha:\s*(\d+),\s*Aprovado:\s*(\d+),\s*Ignorado:\s*(\d+),\s*Total:\s*(\d+)") {
        $failedTests = [int]$matches[1]
        $passedTests = [int]$matches[2]
        $skippedTests = [int]$matches[3]
        $totalTests = [int]$matches[4]
    }
    elseif ($outputString -match "Com falha!\s*[–-]\s*Com falha:\s*(\d+),\s*Aprovado:\s*(\d+),\s*Ignorado:\s*(\d+),\s*Total:\s*(\d+)") {
        $failedTests = [int]$matches[1]
        $passedTests = [int]$matches[2]
        $skippedTests = [int]$matches[3]
        $totalTests = [int]$matches[4]
    }
    # Padrões para capturar estatísticas em inglês (formato completo)
    elseif ($outputString -match "Passed!\s*[–-]\s*Failed:\s*(\d+),\s*Passed:\s*(\d+),\s*Skipped:\s*(\d+),\s*Total:\s*(\d+)") {
        $failedTests = [int]$matches[1]
        $passedTests = [int]$matches[2]
        $skippedTests = [int]$matches[3]
        $totalTests = [int]$matches[4]
    }
    elseif ($outputString -match "Failed!\s*[–-]\s*Failed:\s*(\d+),\s*Passed:\s*(\d+),\s*Skipped:\s*(\d+),\s*Total:\s*(\d+)") {
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

# Manter o log persistente e remover apenas o arquivo temporário usado para parsing
if (Test-Path $tempLogFile) {
    Remove-Item $tempLogFile -Force -ErrorAction SilentlyContinue
}

Write-Host ""

# Encontrar arquivos de cobertura gerados
$coverageFiles = Get-ChildItem -Path $fullOutputPath -Filter "coverage.cobertura.xml" -Recurse
if ($coverageFiles.Count -eq 0) {
    Write-ColorOutput "AVISO: Nenhum arquivo de cobertura encontrado." "Yellow"
    Write-ColorOutput "Verifique se o pacote coverlet.collector está instalado nos projetos de teste." "Yellow"
    if ($testExitCode -ne 0) {
        exit $testExitCode
    }
    exit 1
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

& dotnet tool run reportgenerator -- $reportArgs

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

$coverageExitCode = 1

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
            $coverageExitCode = 0
        }
        else {
            Write-ColorOutput "✗ Cobertura de código: $coveragePercent% (Mínimo: $MinimumCoverage%)" "Red"
            Write-ColorOutput "AVISO: Cobertura abaixo do mínimo aceitável!" "Yellow"
            $coverageExitCode = 2
        }
    }
}

Write-Host ""
Write-ColorOutput "════════════════════════════════════════════════════════════════" "Cyan"
Write-ColorOutput "Execução Concluída!" "Green"
Write-ColorOutput "════════════════════════════════════════════════════════════════" "Cyan"
Write-Host ""

if (Test-Path $persistentLogFile) {
    if ($LogOnlyFailedTests) {
        Write-ColorOutput "Log de falhas da execução salvo em: $persistentLogFile" "Cyan"
    }
    else {
        Write-ColorOutput "Log completo da execução salvo em: $persistentLogFile" "Cyan"
    }
    Write-Host ""
}

# Abrir relatório no navegador
$indexFile = Join-Path $reportPath "index.html"
if (Test-Path $indexFile) {
    $hasInteractiveConsole = [Environment]::UserInteractive -and -not [Console]::IsInputRedirected
    if ($OpenReport -and $hasInteractiveConsole) {
        Start-Process $indexFile
    }
}

Write-ColorOutput "Para visualizar o relatório posteriormente, abra:" "Cyan"
Write-ColorOutput $indexFile "Yellow"
Write-Host ""

    if ($testExitCode -ne 0) {
        exit $testExitCode
    }

    exit $coverageExitCode
}
finally {
    Release-RunLock
}
