# Testes

Este repositório utiliza xUnit como base de testes e centraliza suas dependências em `test/Directory.Build.props`.

## Tipos de teste

- `Unit`: valida comportamento isolado e rápido
- `Integration`: valida integração entre componentes e cenários mais completos

O padrão do repositório é classificar testes com `Trait`.

```csharp
[Trait("Category", "Unit")]
[Trait("Category", "Integration")]
```

## Comandos principais

Executar todos os testes:

```powershell
dotnet test
```

Executar somente testes unitários:

```powershell
dotnet test --filter "Category=Unit"
```

Executar somente testes de integração:

```powershell
dotnet test --filter "Category=Integration"
```

Gerar cobertura com o script do repositório:

```powershell
cd test
pwsh ./run-tests.ps1
```

## Convenções adotadas

- Projetos de teste usam o sufixo `*.xTest`
- Framework de teste: xUnit
- Mocks: Moq
- Geração de dados: Bogus
- Cobertura: `coverlet.collector`

## Boas práticas esperadas em contribuições

- Adicione ou atualize testes para toda mudança de comportamento.
- Prefira testes pequenos e determinísticos.
- Use `Unit` por padrão; suba para `Integration` apenas quando houver motivo claro.
- Evite dependência externa não controlada no teste.

## Quando atualizar testes existentes

Atualize testes existentes quando:

- O contrato público mudar
- A semântica de erro ou validação mudar
- O comportamento documentado mudar
- Um bug corrigido precisar de regressão automatizada

## Cobertura e revisão

O objetivo da cobertura não é maximizar números artificialmente, mas impedir regressões nas áreas tocadas.

Inclua no PR evidências suficientes de que a mudança foi validada localmente.
