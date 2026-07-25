<#
.SYNOPSIS
    Remove recursivamente as pastas de build e documentacao do workspace.

.DESCRIPTION
    Script PowerShell que limpa artefatos de compilacao (.NET) e documentacao gerada em um workspace.

    O script procura e remove recursivamente as seguintes pastas em todo o workspace:
    - bin: Pasta de saida compilada do .NET (Debug/Release)
    - obj: Pasta de objetos intermediarios do .NET (arquivos compilados temporarios)
    - Docs: Pasta de documentacao gerada (exceto a pasta docs raiz do workspace)

    Nota especial: A pasta "docs" na raiz do workspace e preservada, pois contem
    documentacao manual do projeto. Apenas pastas "Docs" aninhadas dentro de projetos
    sao removidas.

.EXAMPLE
    PS C:\Users\user\projetos\Inventory360Api\.tools\scripts> .\cleanup.ps1

    Executa a limpeza e exibe relatorio detalhado de pastas encontradas e removidas.

.EXAMPLE
    & "C:\Users\user\projetos\Inventory360Api\.tools\scripts\cleanup.ps1"

    Alternativa com o operador de chamada (&) para executar o script.

.EXAMPLE
    # Executar a partir da raiz do workspace
    Invoke-Expression "& '.\\.tools\\scripts\\cleanup.ps1'"

    Executa o script usando Invoke-Expression (util em scripts automation).

.NOTES
    Informacoes Tecnicas:
        - Workspace root e detectado automaticamente a partir do caminho do script,
            procurando arquivos de solution .sln e .slnx na raiz.
    - O script usa Get-ChildItem com -Recurse para busca profunda no diretorio.
    - Remocoes sao executadas com o sinalizador -Force, mesmo para pastas com conteudo.
    - Mensagens de cor indicam status:
        * Amarelo: Acoes gerais em progresso
        * Verde: Sucesso, pastas encontradas
        * Vermelho: Remocao de pastas
        * Cinza: Nenhuma pasta encontrada
        * Vermelho escuro: Erros durante a remocao
        * Amarelo escuro: Pastas skipped

    Tratamento de Erros:
    - Se uma pasta nao puder ser removida, o erro e capturado e exibido.
    - O script continua processando outras pastas mesmo apos falhas.
    - O numero total de pastas removidas com sucesso e exibido ao final.

    Proximos Passos:
    - Se voce removeu pacotes NuGet (bin/obj), execute "dotnet restore" para restaura-los.
    - Se mantem caches de build, considere executar este script antes de compilacoes limpas.

.LINK
    https://docs.microsoft.com/powershell/

.LINK
    https://dotnet.microsoft.com/

.PARAMETER
    Este script nao aceita parametros. Execucao e sempre no workspace root detectado.

#>

function Get-WorkspaceRoot {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string]$StartPath
    )

    $current = [System.IO.DirectoryInfo]::new($StartPath)

    while ($null -ne $current) {
        $solutionFiles = Get-ChildItem -Path $current.FullName -File -ErrorAction SilentlyContinue |
        Where-Object { $_.Extension -in @('.sln', '.slnx') }

        if ($solutionFiles.Count -gt 0) {
            return $current.FullName
        }

        $current = $current.Parent
    }

    throw "Nao foi possivel determinar a raiz do workspace a partir de '$StartPath'."
}

function Test-IsRootDocsFolder {
    param (
        [Parameter(Mandatory = $true)]
        [System.IO.DirectoryInfo]$Directory,

        [Parameter(Mandatory = $true)]
        [string]$RootPath
    )

    return $Directory.Name -ieq "docs" -and $Directory.Parent.FullName -ieq $RootPath
}

$WorkspaceRoot = Get-WorkspaceRoot -StartPath $PSScriptRoot

Write-Host "Iniciando limpeza de pastas bin, obj e Docs..." -ForegroundColor Yellow
Write-Host "Workspace: $WorkspaceRoot" -ForegroundColor Cyan

# Pastas a serem removidas durante o processo de limpeza
$FoldersToRemove = @("bin", "obj", "Docs")
$TotalRemoved = 0

# Loop principal: percorre cada tipo de pasta para limpeza
foreach ($FolderName in $FoldersToRemove) {
    Write-Host "Procurando pastas $FolderName..." -ForegroundColor White

    # Busca recursivamente todas as pastas com o nome especificado
    $FoundFolders = Get-ChildItem -Path $WorkspaceRoot -Recurse -Directory | Where-Object { $_.Name -eq $FolderName }

    if ($FoundFolders.Count -gt 0) {
        Write-Host "Encontradas $($FoundFolders.Count) pasta(s) $FolderName" -ForegroundColor Green

        # Processa cada pasta encontrada
        foreach ($Dir in $FoundFolders) {
            try {
                # Verifica se e a pasta docs raiz (deve ser preservada)
                if (Test-IsRootDocsFolder -Directory $Dir -RootPath $WorkspaceRoot) {
                    Write-Host "Pulando pasta docs raiz: $($Dir.FullName)" -ForegroundColor DarkYellow
                    continue
                }

                # Remove a pasta recursivamente
                Write-Host "Removendo: $($Dir.FullName)" -ForegroundColor Red
                Remove-Item -Path $Dir.FullName -Recurse -Force -ErrorAction Stop
                $TotalRemoved++
            }
            catch {
                # Captura e exibe erros durante remocao, mas continua o processamento
                Write-Host "Falha ao remover: $($Dir.FullName)" -ForegroundColor DarkRed
                Write-Host "Erro: $($_.Exception.Message)" -ForegroundColor DarkRed
            }
        }
    }
    else {
        Write-Host "Nenhuma pasta $FolderName encontrada" -ForegroundColor Gray
    }
}

# Relatorio final de conclusao
Write-Host "Limpeza concluida!" -ForegroundColor Green
Write-Host "Total de pastas removidas: $TotalRemoved" -ForegroundColor Cyan

# Dica util se pacotes foram removidos
if ($TotalRemoved -gt 0) {
    Write-Host "Dica: Execute 'dotnet restore' para restaurar pacotes NuGet se necessario" -ForegroundColor Yellow
}
