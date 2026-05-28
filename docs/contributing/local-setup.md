# Setup local

Este guia descreve o setup mínimo para contribuir com segurança no repositório.

## Pré-requisitos

- .NET SDK `8.0.303`
- Git atualizado
- PowerShell 7 recomendado
- Visual Studio 2022 ou VS Code

O SDK obrigatório é definido em [global.json](../../global.json).

```powershell
dotnet --version
```

## Clonando e restaurando

```powershell
git clone <seu-fork>
cd Nuuvify.CommonPack-policy
dotnet restore
```

## Build local

Para validar a solução completa:

```powershell
dotnet build Nuuvify.CommonPack.sln
```

Para uma validação mais próxima do pipeline principal:

```powershell
dotnet build Nuuvify.CommonPack.sln -c Release
```

## Tasks úteis no workspace

O workspace já possui tasks prontas:

- `build`
- `build-clean`
- `rebuild`

Essas tasks ajudam principalmente em contribuições feitas via VS Code.

## Observações importantes

- O CI principal do projeto roda em Azure DevOps.
- O arquivo `src/Directory.Build.props` centraliza metadados de pacote, versão, documentação XML e empacotamento.
- O arquivo `test/Directory.Build.props` centraliza dependências e comportamento dos projetos de teste.

## Limitações conhecidas do script de testes

O script `test/run-tests.ps1` deve ser executado a partir do diretório `test/`.

Exemplo correto:

```powershell
cd test
pwsh ./run-tests.ps1
```

Se ele for executado fora desse diretório, pode encerrar sem executar os testes.
