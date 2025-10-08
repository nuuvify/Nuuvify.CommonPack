# Nuuvify.CommonPack.BackgroundService

[![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.BackgroundService.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.BackgroundService/)
[![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.BackgroundService.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.BackgroundService/)

Biblioteca para criação de serviços de background que processam mensagens do Azure Service Bus de forma robusta e com telemetria integrada.

## Características

- ✅ Classe base abstrata para implementação de workers
- ✅ Integração completa com Azure Service Bus
- ✅ Telemetria e observabilidade com OpenTelemetry
- ✅ Tratamento robusto de erros com diagnóstico avançado
- ✅ **Propriedades de diagnóstico** para Dead Letter Queue e Abandon
- ✅ **Rastreamento detalhado** de falhas com metadados contextuais
- ✅ Configuração flexível via connection strings ou Azure credentials
- ✅ Suporte a dead letter queue e message abandonment
- ✅ **Arquitetura modular** com separação de responsabilidades
- ✅ **Complexidade cognitiva reduzida** para melhor manutenibilidade
- ✅ Logging estruturado
- ✅ Testes unitários incluídos

## Instalação

```bash
dotnet add package Nuuvify.CommonPack.BackgroundService
```

## Uso Básico

### 1. Implementação da Classe Concreta

```csharp
public class OrderProcessingService : BackgroundServiceAbstract<OrderProcessingService>
{
    public OrderProcessingService(
        ILogger<OrderProcessingService> logger,
        IConfigurationCustom configurationCustom,
        RequestConfiguration requestConfiguration,
        ActivitySource activitySource)
        : base(logger, configurationCustom, requestConfiguration, activitySource,
               maxConcurrentCalls: 10, maxAutoLockRenewalDuration: TimeSpan.FromMinutes(10))
    {
        // Configurar comportamento em caso de falha
        AbandonMessageIfFailed = true; // ou false para usar dead letter queue
    }

    public override async Task<bool> ExecuteRule(
        ServiceBusReceivedMessage message,
        ActivitySource activitySource,
        CancellationToken cancellationToken)
    {
        using var activity = activitySource.StartActivity("ProcessOrder");

        try
        {
            // Sua lógica de processamento aqui
            var messageBody = message.Body.ToString();
            var orderData = JsonSerializer.Deserialize<OrderData>(messageBody);

            // Adicionar telemetria
            activity?.SetTag("order.id", orderData.OrderId);

            Logger.LogInformation("Processando pedido {OrderId}", orderData.OrderId);

            // Processar o pedido
            await ProcessOrderAsync(orderData, cancellationToken);

            return true; // Sucesso
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao processar mensagem {MessageId}", message.MessageId);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return false; // Falha
        }
    }

    private async Task ProcessOrderAsync(OrderData orderData, CancellationToken cancellationToken)
    {
        // Implementar lógica de negócio
        await Task.Delay(1000, cancellationToken);
    }
}
```

### 2. Configuração no appsettings.json

```json
{
  "AppConfig": {
    "ServiceBus": {
      "Cnn": "ServiceBusConnection",
      "Topic": "orders-topic",
      "Subscription": "order-processing-subscription"
    }
  },
  "ConnectionStrings": {
    "ServiceBusConnection": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key"
  }
}
```

### 3. Registro no DI Container

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Registrar dependências
    services.AddSingleton<IConfigurationCustom, ConfigurationCustom>();
    services.AddScoped<RequestConfiguration>();
    services.AddSingleton(new ActivitySource("YourApp"));

    // Registrar o background service
    services.AddHostedService<OrderProcessingService>();
}
```

## Configuração com Azure Credentials

Para ambientes de produção, recomenda-se usar Azure Managed Identity:

```csharp
public override void ConfigureServiceBus(
    ServiceBusClientOptions serviceBusClientOptions,
    TokenCredential credential,
    ServiceBusProcessorOptions serviceBusProcessorOptions)
{
    base.ConfigureServiceBus(serviceBusClientOptions, credential, serviceBusProcessorOptions);
}
```

Configure no appsettings.json:

```json
{
  "AppConfig": {
    "ServiceBus": {
      "FullyQualifiedNamespace": "your-namespace.servicebus.windows.net",
      "Topic": "orders-topic",
      "Subscription": "order-processing-subscription"
    }
  }
}
```

## Interface IBackgroundServiceAbstract

A biblioteca fornece uma interface que define o contrato:

```csharp
public interface IBackgroundServiceAbstract<T>
{
    Task<bool> ExecuteRule(
        ServiceBusReceivedMessage message,
        ActivitySource activitySource,
        CancellationToken cancellationToken);

    bool AbandonMessageIfFailed { get; set; }
}
```

## Tratamento de Erros

A classe base trata automaticamente diferentes tipos de erros com **diagnóstico avançado**:

- **ServiceBusException**: Erros específicos do Service Bus (lock perdido, quota excedida, etc.)
- **OperationCanceledException**: Cancelamento de operação
- **Exception**: Erros gerais

### Comportamento de Falhas

Configure o comportamento quando uma mensagem falha:

```csharp
// Abandonar mensagem (retorna para a fila)
AbandonMessageIfFailed = true;

// Enviar para dead letter queue (padrão)
AbandonMessageIfFailed = false;
```

### 🆕 Propriedades de Diagnóstico

Todas as mensagens que falham agora incluem **propriedades de diagnóstico contextuais**:

#### Dead Letter Queue Properties:
```json
{
  "ErrorDetails": "Unhandled exception: Database connection failed",
  "FailureTime": "2025-10-07T19:26:30.123Z",
  "WorkerVersion": "1.2.3.0",
  "CorrelationId": "abc-123-def-456",
  "DeliveryAttempt": 3,
  "MessageId": "msg-789",
  "ExceptionType": "SqlException",
  "WorkerInstance": "SERVER01",
  "ProcessedBy": "OrderProcessingService"
}
```

#### Abandon Message Properties:
```json
{
  "AbandonReason": "Operation was cancelled",
  "AbandonTime": "2025-10-07T19:26:30.123Z",
  "RetryCount": 2,
  "CorrelationId": "abc-123-def-456",
  "MessageId": "msg-789",
  "WorkerInstance": "SERVER01",
  "ProcessedBy": "OrderProcessingService",
  "NextRetryHint": "2025-10-07T19:27:30.123Z"
}
```

### Benefícios do Diagnóstico Avançado:

- 🔍 **Troubleshooting Melhorado**: Informações detalhadas sobre falhas
- 📊 **Observabilidade**: Rastreamento de padrões de erro
- 🎯 **Root Cause Analysis**: Identificação rápida de problemas sistêmicos
- 📈 **Métricas**: Criação de dashboards baseados nas propriedades

## Telemetria e Observabilidade

A classe integra automaticamente com OpenTelemetry:

- **Traces**: Cada processamento de mensagem gera um trace
- **Tags**: CorrelationId, dados da mensagem
- **Status**: Success/Error com detalhes

## Logs Estruturados

A biblioteca gera logs estruturados em vários níveis:

- `Information`: Início/fim do processamento
- `Warning`: Operações canceladas
- `Error`: Falhas no processamento
- `Debug`: Detalhes internos (quando habilitado)

## 🆕 Arquitetura Modular

A partir desta versão, o código foi **refatorado** para melhor manutenibilidade:

### Separação de Responsabilidades

- **`ServiceBusBackgroundService.cs`**: Classe principal com lógica core
- **`ServiceBusBackgroundService.ExceptionHandling.cs`**: Tratamento especializado de exceções

### Métodos de Tratamento de Exceções

```csharp
// Métodos especializados para cada tipo de erro
protected virtual async Task HandleServiceBusSpecificExceptionAsync(...)
protected virtual async Task HandleServiceBusCommunicationExceptionAsync(...)
protected virtual async Task HandleOperationCanceledExceptionAsync(...)
protected virtual async Task HandleGenericExceptionAsync(...)
protected virtual async Task HandleBusinessLogicFailureAsync(...)
```

### Métodos de Diagnóstico

```csharp
// Criação de propriedades contextuais
protected virtual Dictionary<string, object> CreateDeadLetterProperties(...)
protected virtual Dictionary<string, object> CreateAbandonProperties(...)
```

### Benefícios da Refatoração:

- ✅ **Complexidade Cognitiva Reduzida**: Métodos menores e mais focados
- ✅ **Manutenibilidade**: Mais fácil de modificar e estender
- ✅ **Testabilidade**: Métodos especializados podem ser testados individualmente
- ✅ **Reutilização**: Métodos podem ser sobrescritos em classes derivadas
- ✅ **Clean Code**: Seguindo princípios SOLID

## Configurações Avançadas

### Parâmetros do Construtor

```csharp
protected BackgroundServiceAbstract(
    ILogger<BackgroundServiceAbstract<T>> logger,
    IConfigurationCustom configurationCustom,
    RequestConfiguration requestConfiguration,
    ActivitySource activitySourceCustom,
    int maxConcurrentCalls = 10,                    // Máximo de mensagens processadas simultaneamente
    TimeSpan maxAutoLockRenewalDuration = TimeSpan.FromMinutes(5)) // Tempo de renovação do lock
```

### Propriedades Protegidas

- `Logger`: Instância do logger
- `ConfigurationCustom`: Configuração da aplicação
- `RequestConfiguration`: Configuração da requisição
- `ActivitySourceCustom`: Source para telemetria

### Métodos Personalizáveis

Você pode sobrescrever os métodos de tratamento de exceções para comportamento customizado:

```csharp
protected override async Task HandleGenericExceptionAsync(
    ProcessMessageEventArgs args,
    Exception ex,
    CancellationToken cancellationToken)
{
    // Seu tratamento customizado
    Logger.LogError(ex, "Erro customizado: {Message}", ex.Message);

    // Chamar implementação base se necessário
    await base.HandleGenericExceptionAsync(args, ex, cancellationToken);
}
```

## Exemplo Completo

Veja o arquivo `Examples/OrderProcessingBackgroundService.cs` para um exemplo completo de implementação.

## Testes

A biblioteca inclui testes unitários abrangentes. Para executar:

```bash
dotnet test test/Nuuvify.CommonPack.BackgroundService.xTest/
```

## Dependências

- Azure.Messaging.ServiceBus
- Microsoft.Extensions.Hosting.Abstractions
- Azure.Monitor.OpenTelemetry.AspNetCore
- Nuuvify.CommonPack.Middleware.Abstraction

## Licença

Este projeto está licenciado sob a licença MIT. Veja o arquivo LICENSE para detalhes.

## Contribuição

Contribuições são bem-vindas! Por favor, veja o arquivo CONTRIBUTING.md para diretrizes.

## Changelog

Veja o arquivo CHANGELOG.md para histórico de versões e mudanças.
