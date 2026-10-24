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

Executar os testes unitários rápidos, como o CI faz:

```powershell
dotnet test --filter "Category=Unit&Category!=ServiceBusLongRunning"
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

## Testes longos do Service Bus

Alguns testes de `Nuuvify.CommonPack.AzureServiceBus.xTest` dependem das tentativas de conexão do
Azure SDK e podem levar mais de 15 minutos. Eles têm, além de `Unit`, a categoria
`ServiceBusLongRunning`:

```csharp
[Trait("Category", "ServiceBusLongRunning")]
[Fact]
public async Task SendMessageToTopicAsync_WithOperationOptions_ShouldSucceed()
```

- O CI exclui essa categoria dos testes unitários e do SonarCloud.
- O workflow [servicebus-long-running.yml](../../.github/workflows/servicebus-long-running.yml)
  executa a categoria, dentro do `Result`, somente quando o PR altera
  `src/Nuuvify.CommonPack.AzureServiceBus/CHANGELOG.md` ou o projeto de testes do pacote. Também
  pode ser disparado manualmente em `Actions`.
- Marque com essa categoria todo teste novo do pacote que leve mais de 1 segundo por depender do
  Azure SDK.

Para executá-los localmente:

```powershell
dotnet test test/Nuuvify.CommonPack.AzureServiceBus.xTest --filter "Category=ServiceBusLongRunning"
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
