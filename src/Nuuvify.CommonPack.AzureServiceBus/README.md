# Nuuvify.CommonPack.AzureServiceBus

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![Azure Service Bus](https://img.shields.io/badge/Azure-Service%20Bus-0078d4.svg)](https://azure.microsoft.com/services/service-bus/)
[![Thread-Safe](https://img.shields.io/badge/Thread--Safe-✓-green.svg)]()
[![NuGet Version](https://img.shields.io/nuget/v/Nuuvify.CommonPack.AzureServiceBus.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.AzureServiceBus/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Uma biblioteca .NET 8.0 robusta e thread-safe para integração com Azure Service Bus, projetada especificamente para aplicações empresariais, APIs REST e Worker Services.

## 🚀 Características Principais

- **Thread-Safe**: Totalmente thread-safe, ideal para uso como Singleton em APIs REST
- **Alta Performance**: Pool de conexões interno e reutilização eficiente de recursos
- **Operações Completas**: Suporte a filas, tópicos, operações em lote e agendamento
- **Retry Automático**: Retry exponencial com configuração flexível
- **Logging Integrado**: Observabilidade completa com Microsoft.Extensions.Logging
- **Configuração Flexível**: Suporte a IOptions pattern e configuração programática
- **Dispose Pattern**: Implementação correta de IDisposable e IAsyncDisposable
- **Validação Robusta**: Validação completa de parâmetros e configurações

## 📦 Instalação

```bash
dotnet add package Nuuvify.CommonPack.AzureServiceBus
dotnet add package Nuuvify.CommonPack.AzureServiceBus.Abstraction
```

## ⚙️ Configuração Rápida

### 1. Configuração no appsettings.json
- ServiceBus-SuaAplicacao--ConnectionString não deve ser incluido no appsettings.json, é um segredo e por isso estara no Vault
```json
{
  "ServiceBus-SuaAplicacao": {
    "ConnectionString": "Endpoint=sb://seu-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=sua-chave",
    "QueueName": "notifications",
    "TopicName": "events",
    "TopicSubscription": "pedidos",
    "OperationTimeoutSeconds": 30,
    "MaxRetryAttempts": 3,
    "RetryDelaySeconds": 5,
    "MaxBatchSize": 100,
    "DefaultMessageTtlMinutes": 60
  }
}
```

### 2. Registro no Container de DI

```csharp
// Program.cs (.NET 8)
using Nuuvify.CommonPack.AzureServiceBus.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Registra como Singleton (recomendado para APIs REST)
builder.Services.AddAzureServiceBus(builder.Configuration);

var app = builder.Build();
```

### 3. Uso em Controllers/Services

```csharp
[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly IServiceBusMessageSender _serviceBus;

    public NotificationController(IServiceBusMessageSender serviceBus)
    {
        _serviceBus = serviceBus;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
    {
        await _serviceBus.SendMessageToQueueAsync("notifications", request);
        return Ok(new { Status = "Message sent successfully" });
    }
}
```

## 🎯 Casos de Uso Comuns

### Envio para Múltiplas Filas (E-commerce)

```csharp
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IServiceBusMessageSender _serviceBus;

    public OrderController(IServiceBusMessageSender serviceBus)
    {
        _serviceBus = serviceBus;
    }

    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var order = new Order(request);

        // ✅ Uma única instância Singleton para múltiplas filas
        var tasks = new[]
        {
            // Processamento do pedido
            _serviceBus.SendMessageToQueueAsync("order-processing", order),

            // Email de confirmação
            _serviceBus.SendMessageToQueueAsync("email-notifications",
                new EmailNotification(order.CustomerEmail, "Order Confirmed")),

            // Atualização de estoque
            _serviceBus.SendMessageToQueueAsync("inventory-updates",
                new InventoryUpdate(order.Items)),

            // Análise de dados (usando tópico)
            _serviceBus.SendMessageToTopicAsync("analytics-events",
                new OrderCreatedEvent(order))
        };

        await Task.WhenAll(tasks); // Envio paralelo eficiente

        return Ok(new { OrderId = order.Id });
    }
}
```

### Operações em Lote para Alta Performance

```csharp
public class BulkNotificationService
{
    private readonly IServiceBusMessageSender _serviceBus;

    public BulkNotificationService(IServiceBusMessageSender serviceBus)
    {
        _serviceBus = serviceBus;
    }

    public async Task SendBulkNotificationsAsync(IEnumerable<User> users)
    {
        var notifications = users.Select(u => new NotificationMessage
        {
            UserId = u.Id,
            Email = u.Email,
            Message = $"Hello {u.Name}!"
        });

        // ✅ Envio em lote - muito mais eficiente
        await _serviceBus.SendBatchMessagesToQueueAsync("notifications", notifications);
    }
}
```

### Mensagens Agendadas

```csharp
public class ScheduledReminderService
{
    private readonly IServiceBusMessageSender _serviceBus;

    public ScheduledReminderService(IServiceBusMessageSender serviceBus)
    {
        _serviceBus = serviceBus;
    }

    public async Task ScheduleReminder(User user, DateTime reminderTime)
    {
        var reminder = new ReminderMessage
        {
            UserId = user.Id,
            Message = "Don't forget your appointment!",
            ScheduledFor = reminderTime
        };

        // ✅ Agendamento para entrega futura
        var sequenceNumber = await _serviceBus.ScheduleMessageToQueueAsync(
            "reminders",
            reminder,
            reminderTime
        );

        // Salvar sequenceNumber para possível cancelamento
        await SaveReminderSchedule(user.Id, sequenceNumber);
    }
}
```

## 🛠️ Configurações Avançadas

### Configuração Básica

```csharp
builder.Services.AddAzureServiceBus(config =>
{
    config.ConnectionString = GetConnectionStringFromKeyVault();
    config.MaxRetryAttempts = 5;
    config.OperationTimeoutSeconds = 60;
    config.EnablePartitioning = true;
});
```

### Configuração Avançada com ServiceBusClientOptions

Para cenários que requerem controle total sobre o cliente Service Bus:

```csharp
builder.Services.AddAzureServiceBusAdvanced(
    // Configuração básica
    basicConfig =>
    {
        basicConfig.ConnectionString = "Endpoint=sb://mynamespace.servicebus.windows.net/;...";
        basicConfig.OperationTimeoutSeconds = 60;
        basicConfig.MaxRetryAttempts = 5;
        basicConfig.RetryDelaySeconds = 2;
    },
    // Configuração avançada do cliente
    clientConfig =>
    {
        clientConfig.TransportType = ServiceBusTransportType.AmqpTcp; // TCP para melhor performance
        clientConfig.RetryOptions = new ServiceBusRetryOptions
        {
            MaxRetries = 5,
            Delay = TimeSpan.FromSeconds(2),
            MaxDelay = TimeSpan.FromSeconds(30),
            Mode = ServiceBusRetryMode.Exponential
        };
        // Otimização de performance: reutilizar conexões
        clientConfig.ReuseConnections = true; // Padrão: true

        // Para ambientes corporativos com proxy
        clientConfig.WebProxy = new System.Net.WebProxy("http://proxy.company.com:8080");
    });
```

### Configuração com Cliente Pré-configurado

Útil quando você já tem um ServiceBusClient configurado:

```csharp
// Criar cliente com configurações específicas
var serviceBusClient = new ServiceBusClient(
    "Endpoint=sb://mynamespace.servicebus.windows.net/;...",
    new ServiceBusClientOptions
    {
        TransportType = ServiceBusTransportType.AmqpTcp,
        RetryOptions = new ServiceBusRetryOptions
        {
            MaxRetries = 10,
            Delay = TimeSpan.FromSeconds(1)
        }
    });

builder.Services.AddAzureServiceBusWithClient(serviceBusClient, config =>
{
    config.OperationTimeoutSeconds = 45;
    config.MaxBatchSize = 200;
});
```

### Configuração com Factory Customizada

Para lógica de criação totalmente personalizada:

```csharp
builder.Services.AddAzureServiceBusWithFactory(
    "Endpoint=sb://mynamespace.servicebus.windows.net/;...",
    (connectionString, options) =>
    {
        // Lógica customizada para criar cliente
        var customOptions = new ServiceBusClientOptions
        {
            TransportType = DetermineTransportType(), // Lógica customizada
            RetryOptions = CreateCustomRetryOptions()  // Baseado em configurações externas
        };
        return new ServiceBusClient(connectionString, customOptions);
    });
```

### Configuração por Operação (Runtime)

Para cenários multi-tenant ou diferentes configurações por contexto:

```csharp
public class MultiTenantService
{
    private readonly IServiceBusMessageSender _serviceBusMessageSender;

    public MultiTenantService(IServiceBusMessageSender serviceBusMessageSender)
    {
        _serviceBusMessageSender = serviceBusMessageSender;
    }

    public async Task SendMessageForTenant(string tenantId, object message)
    {
        // Cliente customizado para operações de alta prioridade por tenant
        var operationOptions = new ServiceBusOperationOptions
        {
            CustomConnectionString = GetConnectionStringForTenant(tenantId),
            CustomClientOptions = new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpTcp,
                RetryOptions = new ServiceBusRetryOptions
                {
                    MaxRetries = 10,
                    Delay = TimeSpan.FromMilliseconds(500)
                }
            },
            UseTemporaryClient = true // Cliente será descartado após uso
        };

        var messageOptions = new ServiceBusMessageOptions
        {
            Subject = $"TenantMessage-{tenantId}",
            ApplicationProperties = { ["TenantId"] = tenantId, ["Priority"] = "High" }
        };

        await _serviceBusMessageSender.SendMessageToQueueAsync(
            $"tenant-{tenantId}-queue",
            message,
            messageOptions,
            operationOptions);
    }
}
```

### Exemplo Multi-Ambiente com Diferentes Prioridades

```csharp
public class PriorityAwareService
{
    private readonly IServiceBusMessageSender _serviceBusMessageSender;

    public PriorityAwareService(IServiceBusMessageSender serviceBusMessageSender)
    {
        _serviceBusMessageSender = serviceBusMessageSender;
    }

    public async Task SendMessage(object message, MessagePriority priority)
    {
        ServiceBusOperationOptions? operationOptions = null;

        if (priority == MessagePriority.High)
        {
            operationOptions = new ServiceBusOperationOptions
            {
                CustomClientOptions = new ServiceBusClientOptions
                {
                    RetryOptions = new ServiceBusRetryOptions
                    {
                        MaxRetries = 10,
                        Delay = TimeSpan.FromMilliseconds(100) // Retry mais rápido para alta prioridade
                    }
                },
                CustomConnectionString = GetHighPriorityConnectionString(),
                UseTemporaryClient = true
            };
        }

        var queueName = priority == MessagePriority.High ? "high-priority-queue" : "normal-queue";

        await _serviceBusMessageSender.SendMessageToQueueAsync(
            queueName,
            message,
            operationOptions: operationOptions);
    }
}
```

### Configuração via Factory (Múltiplas Connection Strings)

```csharp
builder.Services.AddAzureServiceBus(provider =>
{
    var keyVault = provider.GetRequiredService<IKeyVaultService>();
    return new ServiceBusConfiguration
    {
        ConnectionString = keyVault.GetSecret("ServiceBusConnectionString"),
        MaxRetryAttempts = 5
    };
});
```

### Configurações de Mensagem Personalizadas

```csharp
var options = new ServiceBusMessageOptions
{
    MessageId = Guid.NewGuid().ToString(),
    CorrelationId = correlationId,
    SessionId = sessionId,
    TimeToLive = TimeSpan.FromHours(2),
    Subject = "Order.Created",
    ContentType = "application/json",
    ApplicationProperties =
    {
        ["TenantId"] = "tenant-123",
        ["Priority"] = "High",
        ["Version"] = "2.0"
    }
};

await _serviceBus.SendMessageToQueueAsync("orders", order, options);
```

## 🏗️ Arquitetura e Design

### Por que Singleton é Ideal para APIs REST?

1. **Thread-Safety**: A implementação é completamente thread-safe
2. **Performance**: Reutilização de conexão TCP/TLS (~50-100x mais rápido)
3. **Recursos**: Economia de memory, CPU e conexões de rede
4. **Pool Interno**: Azure ServiceBusClient já implementa pool de conexões
5. **Múltiplas Filas**: Uma instância atende todas as filas eficientemente

```csharp
// ✅ CORRETO: Singleton para múltiplas filas
services.AddSingleton<IServiceBusMessageSender, ServiceBusMessageSender>();

// ❌ INCORRETO: Desperdiça recursos
services.AddScoped<IServiceBusMessageSender, ServiceBusMessageSender>();
services.AddTransient<IServiceBusMessageSender, ServiceBusMessageSender>(); // Muito problemático
```

### Comparação de Performance

| Lifetime  | Conexões/Request   | Latência Típica | Uso de Memória | Recomendado |
| --------- | ------------------ | --------------- | -------------- | ----------- |
| Singleton | 1 (reutilizada)    | 5-10ms          | Baixo          | ✅ **SIM**   |
| Scoped    | 1 nova por request | 500-1000ms      | Médio          | ❌ Não       |
| Transient | 1 nova por injeção | 500-1000ms      | Alto           | ❌ **NUNCA** |

## 📋 Interface Completa

A biblioteca oferece métodos para todas as operações principais:
**Você não precisa implementar isso, basta injetar essa interface em alguma classe no seu projeto "Bus" e consumir esses metodos**

```csharp
public interface IServiceBusMessageSender : IAsyncDisposable
{
    // Envio simples
    Task SendMessageToQueueAsync<T>(string queueName, T message, ServiceBusMessageOptions? options = null, CancellationToken cancellationToken = default);
    Task SendMessageToTopicAsync<T>(string topicName, T message, ServiceBusMessageOptions? options = null, CancellationToken cancellationToken = default);

    // Envio em lote
    Task SendBatchMessagesToQueueAsync<T>(string queueName, IEnumerable<T> messages, ServiceBusMessageOptions? options = null, CancellationToken cancellationToken = default);
    Task SendBatchMessagesToTopicAsync<T>(string topicName, IEnumerable<T> messages, ServiceBusMessageOptions? options = null, CancellationToken cancellationToken = default);

    // Mensagens agendadas
    Task<long> ScheduleMessageToQueueAsync<T>(string queueName, T message, DateTimeOffset scheduledEnqueueTime, ServiceBusMessageOptions? options = null, CancellationToken cancellationToken = default);
    Task<long> ScheduleMessageToTopicAsync<T>(string topicName, T message, DateTimeOffset scheduledEnqueueTime, ServiceBusMessageOptions? options = null, CancellationToken cancellationToken = default);

    // Cancelamento de agendamento
    Task CancelScheduledMessageInQueueAsync(string queueName, long sequenceNumber, CancellationToken cancellationToken = default);
    Task CancelScheduledMessageInTopicAsync(string topicName, long sequenceNumber, CancellationToken cancellationToken = default);
}
```

## 🔧 Troubleshooting

### Problemas Comuns

1. **Connection String Inválida**
   ```
   Erro: ArgumentException - Invalid connection string
   Solução: Verificar format no portal Azure
   ```

2. **Timeout em Operações**
   ```
   Erro: ServiceBusException - Operation timed out
   Solução: Aumentar OperationTimeoutSeconds na configuração
   ```

3. **Fila/Tópico Não Existe**
   ```
   Erro: ServiceBusException - Entity not found
   Solução: Criar a entidade no portal Azure ou verificar nome
   ```

### Logs Úteis

A biblioteca produz logs detalhados:

```csharp
// Ativar logs no appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Nuuvify.CommonPack.AzureServiceBus": "Information"
    }
  }
}
```

## 🧪 Testes

### Cobertura de Testes

A biblioteca possui **cobertura de testes abrangente** com mais de **300 testes unitários e de integração**:

- **49.6% de cobertura de linha** para ServiceBusMessageReceiver
- **70% de cobertura** para ServiceBusMessageSender
- **96.9% de cobertura** para ServiceBusConfigurationManager
- **44 testes específicos** para ServiceBusMessageReceiver e classes parciais

### Estrutura de Testes

Os testes estão organizados por funcionalidade:

```
Nuuvify.CommonPack.AzureServiceBus.xTest/
├── Configuration/           # Testes de configuração
├── Services/               # Testes de serviços
│   ├── ServiceBusMessageSender*Tests.cs
│   ├── ServiceBusMessageReceiver*Tests.cs
│   └── ServiceBusConfiguration*Tests.cs
└── Fixtures/               # Fixtures e helpers de teste
```

### Exemplo de Teste Unitário (Sender)

```csharp
[Fact]
public async Task SendMessage_ShouldSucceed_WhenValidConfiguration()
{
    // Arrange
    var config = new ServiceBusConfiguration
    {
        ConnectionString = TestConnectionString,
        MaxRetryAttempts = 1
    };

    var sender = new ServiceBusMessageSender(
        Options.Create(config),
        new NullLogger<ServiceBusMessageSender>()
    );

    // Act & Assert
    await sender.SendMessageToQueueAsync("test-queue", new { Message = "Test" });

    // Cleanup
    await sender.DisposeAsync();
}
```

### Exemplo de Teste Unitário (Receiver)

```csharp
[Fact]
public async Task Constructor_WithValidParameters_ShouldCreateInstance()
{
    // Arrange
    var loggerMock = new Mock<ILogger<TestServiceBusMessageReceiver>>();
    var configMock = new Mock<IConfigurationCustom>();
    var requestConfig = new RequestConfiguration { CorrelationId = Guid.NewGuid().ToString() };

    // Act
    await using var receiver = new TestServiceBusMessageReceiver(
        loggerMock.Object,
        configMock.Object,
        requestConfig);

    // Assert
    Assert.NotNull(receiver);
    Assert.False(receiver.IsProcessing);
}
```

### Testes de Thread Safety

```csharp
[Fact]
public async Task IsProcessing_ThreadSafety_ShouldHandleConcurrentAccess()
{
    // Arrange
    await using var receiver = new TestServiceBusMessageReceiver(logger, config, requestConfig);
    var tasks = new List<Task<bool>>();

    // Act - Access IsProcessing from multiple threads
    for (int i = 0; i < 10; i++)
    {
        tasks.Add(Task.Run(() => receiver.IsProcessing));
    }

    var results = await Task.WhenAll(tasks);

    // Assert - All should return same value
    Assert.All(results, result => Assert.False(result));
}
```

### Executando os Testes

```bash
# Todos os testes
dotnet test

# Apenas testes do AzureServiceBus
dotnet test --filter "FullyQualifiedName~AzureServiceBus"

# Com cobertura de código
dotnet test --collect:"XPlat Code Coverage"

# Gerar relatório de cobertura
reportgenerator -reports:"**/*.cobertura.xml" -targetdir:"CoverageReport"
```

### Ferramentas de Teste Utilizadas

- **xUnit** - Framework de testes principal
- **Moq** - Framework de mock para interfaces e classes
- **Shouldly** - Biblioteca de assertions mais expressivas
- **Bogus** - Geração de dados fake para testes
- **ReportGenerator** - Geração de relatórios de cobertura
- **Custom TestHelpers** - Classes de apoio específicas para ServiceBus

## ⚡ Otimização de Performance: ReuseConnections

### O que é ReuseConnections?

A propriedade `ReuseConnections` na `ServiceBusClientConfiguration` controla se os clientes criados temporariamente pelas `ServiceBusOperationOptions` devem ser reutilizados ou criados a cada operação.

### Como Funciona

```csharp
// Configuração com cache habilitado (padrão)
services.AddAzureServiceBusWithClientConfiguration(configuration, clientConfig =>
{
    clientConfig.ReuseConnections = true; // Padrão
});

// Exemplo de uso que beneficia do cache
var options = new ServiceBusOperationOptions
{
    CustomConnectionString = "Endpoint=sb://específico.servicebus.windows.net/;...",
    CustomClientOptions = new ServiceBusClientOptions
    {
        TransportType = ServiceBusTransportType.AmqpWebSockets
    }
};

// Múltiplas operações com as mesmas configurações
// reutilizarão o mesmo cliente (economiza recursos)
for (int i = 0; i < 100; i++)
{
    await sender.SendMessageToQueueAsync("queue", message, options);
}
```

### Benefícios

- **Performance**: Evita overhead de criação/destruição de conexões TCP
- **Recursos**: Reduz uso de memória e handles de rede
- **Latência**: Operações subsequentes são mais rápidas
- **Throughput**: Maior taxa de transferência para operações em lote

### Quando Usar ReuseConnections = false

```csharp
clientConfig.ReuseConnections = false; // Para debug ou casos específicos
```

- **Debugging**: Para isolar problemas de conexão
- **Testes**: Garantir estado limpo entre testes
- **Configurações únicas**: Cada operação precisa de cliente específico

### Chave do Cache

O cache usa uma chave baseada em:
- Connection String
- TransportType
- Configurações de Retry (MaxRetries, Mode)

Operações com configurações idênticas compartilham o mesmo cliente.

## 📊 Dependências

- **.NET 8.0** - Framework principal
- **Microsoft.Extensions.Logging** - Logging concreto (implementação)
- **Microsoft.Extensions.Options.ConfigurationExtensions** - Extensões de configuração
- **Nuuvify.CommonPack.AzureServiceBus.Abstraction** - Interfaces e abstrações

### Dependências Herdadas (via Abstraction)
- **Azure.Messaging.ServiceBus** - Cliente oficial do Azure Service Bus
- **Microsoft.Extensions.Logging.Abstractions** - Abstrações de logging
- **Microsoft.Extensions.Configuration.Abstractions** - Abstrações de configuração
- **Microsoft.Extensions.Options** - Sistema de opções do .NET
- **Microsoft.Extensions.DependencyInjection.Abstractions** - Abstrações de DI

### Organização do Código

O projeto utiliza uma arquitetura modular com **GlobalUsings.cs** centralizando todas as declarações using:

```csharp
// GlobalUsings.cs - Centraliza using statements
global using Azure.Messaging.ServiceBus;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
// ... outros usings globais
```

### Qualidade do Código

- **Nullable Reference Types**: Habilitado a nível de projeto (não por arquivo)
- **SonarQube Compliant**: Supressões apropriadas para falsos positivos
- **EditorConfig**: Padronização de estilo seguindo .editorconfig do projeto
- **Code Coverage**: Monitoramento contínuo com metas de cobertura
- **Thread-Safe**: Design thread-safe para uso em aplicações concorrentes

## 📄 Licença

Este projeto está licenciado sob a [Licença MIT](LICENSE).

## 🤝 Contribuição

Contribuições são bem-vindas! Por favor:

1. Fork o projeto
2. Crie uma feature branch (`git checkout -b feature/nova-funcionalidade`)
3. Commit suas mudanças (`git commit -am 'Adiciona nova funcionalidade'`)
4. Push para a branch (`git push origin feature/nova-funcionalidade`)
5. Abra um Pull Request

## 📞 Suporte

Para dúvidas e suporte técnico:

- 📧 Email: [suporte@nuuvify.com](mailto:suporte@nuuvify.com)
- 📋 Issues: [GitHub Issues](https://github.com/nuuvify/Nuuvify.CommonPack/issues)
- 📖 Documentação: [Wiki do Projeto](https://github.com/nuuvify/Nuuvify.CommonPack/wiki)
- 💬 Discussões: [GitHub Discussions](https://github.com/nuuvify/Nuuvify.CommonPack/discussions)

## 📈 Versionamento

Este projeto segue o [Semantic Versioning](https://semver.org/):
- **MAJOR**: Mudanças incompatíveis na API
- **MINOR**: Novas funcionalidades mantendo compatibilidade
- **PATCH**: Correções de bugs mantendo compatibilidade

Consulte o [CHANGELOG.md](./CHANGELOG.md) para ver todas as mudanças detalhadas.

## 🏢 Sobre a Nuuvify

A **Nuuvify** é uma empresa especializada em soluções tecnológicas para transformação digital, oferecendo bibliotecas e ferramentas robustas para acelerar o desenvolvimento de aplicações empresariais.

### Outros Pacotes da CommonPack
- [`Nuuvify.CommonPack.UnitOfWork`](../Nuuvify.CommonPack.UnitOfWork/) - Implementação do padrão Unit of Work
- [`Nuuvify.CommonPack.Email`](../Nuuvify.CommonPack.Email/) - Biblioteca para envio de emails
- [`Nuuvify.CommonPack.Security`](../Nuuvify.CommonPack.Security/) - Ferramentas de segurança
- [`Nuuvify.CommonPack.Middleware`](../Nuuvify.CommonPack.Middleware/) - Middlewares customizados
- [`Nuuvify.CommonPack.Extensions`](../Nuuvify.CommonPack.Extensions/) - Extensões úteis para .NET

---

Desenvolvido com ❤️ pela equipe **Nuuvify**.
