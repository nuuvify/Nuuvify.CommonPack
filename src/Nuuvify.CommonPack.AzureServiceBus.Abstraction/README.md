# Nuuvify.CommonPack.AzureServiceBus.Abstraction

[![.NET Standard 2.1](https://img.shields.io/badge/.NET%20Standard-2.1-blue.svg)](https://dotnet.microsoft.com/)
[![Abstractions](https://img.shields.io/badge/Package-Abstractions-yellow.svg)]()
[![NuGet Version](https://img.shields.io/nuget/v/Nuuvify.CommonPack.AzureServiceBus.Abstraction.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.AzureServiceBus.Abstraction/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Pacote de abstrações para integração com Azure Service Bus. Este pacote contém todas as interfaces, modelos e configurações necessárias para trabalhar com Azure Service Bus de forma desacoplada.

## 📋 Conteúdo do Pacote

Este pacote de abstrações inclui:

- **Interfaces**: Contratos para operações com Service Bus
- **Modelos**: Classes de configuração e opções de mensagens
- **Configurações**: Classes de configuração tipadas
- **Sem dependências externas**: Apenas referências mínimas do .NET

## 🎯 Objetivo

O pacote de abstrações permite:

1. **Desacoplamento**: Implementações podem referenciar apenas as abstrações
2. **Testabilidade**: Facilita criação de mocks e testes unitários
3. **Flexibilidade**: Permite múltiplas implementações da mesma interface
4. **Dependency Inversion**: Segue o princípio SOLID de inversão de dependência

## 📦 Estrutura

```
Nuuvify.CommonPack.AzureServiceBus.Abstraction/
├── Configuration/
│   └── ServiceBusConfiguration.cs        # Configurações do Service Bus
├── Interfaces/
│   └── IServiceBusMessageSender.cs       # Interface principal para envio
├── Models/
│   └── ServiceBusMessageOptions.cs       # Opções para mensagens
└── README.md                             # Este arquivo
```

## 🚀 Instalação

```bash
dotnet add package Nuuvify.CommonPack.AzureServiceBus.Abstraction
```

## 🛠️ Uso em Projetos

### Para Implementações

Se você está **implementando** uma funcionalidade de Service Bus:

```bash
# Instale ambos os pacotes
dotnet add package Nuuvify.CommonPack.AzureServiceBus.Abstraction
dotnet add package Nuuvify.CommonPack.AzureServiceBus
```

### Para Consumidores

Se você está **consumindo** funcionalidades de Service Bus:

```bash
# Instale apenas as abstrações
dotnet add package Nuuvify.CommonPack.AzureServiceBus.Abstraction
```

```csharp
// Seu serviço depende apenas da abstração
public class OrderService
{
    private readonly IServiceBusMessageSender _sender;

    public OrderService(IServiceBusMessageSender sender)
    {
        _sender = sender; // ✅ Depende apenas da interface
    }

    public async Task ProcessOrder(Order order)
    {
        await _sender.SendMessageToQueueAsync("orders", order);
    }
}
```

## 🔧 Configuração

### ServiceBusConfiguration

Configuração completa para conexão com Azure Service Bus:

```csharp
public class ServiceBusConfiguration
{
    public string ConnectionString { get; set; } = string.Empty;
    public string? DefaultQueueName { get; set; }
    public string? DefaultTopicName { get; set; }
    public int OperationTimeoutSeconds { get; set; } = 30;
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 5;
    public int MaxBatchSize { get; set; } = 100;
    public bool EnableSessions { get; set; } = false;
    public bool EnablePartitioning { get; set; } = false;
    public int DefaultMessageTtlMinutes { get; set; } = 60;

    // Métodos de validação
    public bool IsValid() { /* ... */ }
    public ReadOnlyCollection<string> ValidateConfiguration() { /* ... */ }
}
```

### ServiceBusMessageOptions

Opções avançadas para personalização de mensagens:

```csharp
public class ServiceBusMessageOptions
{
    public string? MessageId { get; set; }
    public string? CorrelationId { get; set; }
    public string? SessionId { get; set; }
    public TimeSpan? TimeToLive { get; set; }
    public string? Subject { get; set; }
    public string? ContentType { get; set; }
    public string? ReplyTo { get; set; }
    public string? ReplyToSessionId { get; set; }
    public string? PartitionKey { get; set; }
    public string? ViaPartitionKey { get; set; }
    public DateTimeOffset? ScheduledEnqueueTime { get; set; }

    public Dictionary<string, object> ApplicationProperties { get; set; }
}
```

## 📋 Interface Principal

### IServiceBusMessageSender

Interface completa para operações com Azure Service Bus:
**Você não precisa implementar isso basta injetar esse interface em uma classe do seu projeto "Bus" e consumir os metodos abaixo**

```csharp
public interface IServiceBusMessageSender : IAsyncDisposable
{
    // Envio para filas
    Task SendMessageToQueueAsync<T>(string queueName, T message,
        ServiceBusMessageOptions? options = null, CancellationToken cancellationToken = default);

    // Envio para tópicos
    Task SendMessageToTopicAsync<T>(string topicName, T message,
        ServiceBusMessageOptions? options = null, CancellationToken cancellationToken = default);

    // Operações em lote
    Task SendBatchMessagesToQueueAsync<T>(string queueName, IEnumerable<T> messages,
        ServiceBusMessageOptions? options = null, CancellationToken cancellationToken = default);

    Task SendBatchMessagesToTopicAsync<T>(string topicName, IEnumerable<T> messages,
        ServiceBusMessageOptions? options = null, CancellationToken cancellationToken = default);

    // Mensagens agendadas
    Task<long> ScheduleMessageToQueueAsync<T>(string queueName, T message, DateTimeOffset scheduledEnqueueTime,
        ServiceBusMessageOptions? options = null, CancellationToken cancellationToken = default);

    Task<long> ScheduleMessageToTopicAsync<T>(string topicName, T message, DateTimeOffset scheduledEnqueueTime,
        ServiceBusMessageOptions? options = null, CancellationToken cancellationToken = default);

    // Cancelamento de agendamento
    Task CancelScheduledMessageInQueueAsync(string queueName, long sequenceNumber,
        CancellationToken cancellationToken = default);

    Task CancelScheduledMessageInTopicAsync(string topicName, long sequenceNumber,
        CancellationToken cancellationToken = default);
}
```

## 🧪 Testabilidade

### Mock para Testes Unitários

```csharp
public class MockServiceBusMessageSender : IServiceBusMessageSender
{
    public List<(string Queue, object Message)> SentMessages { get; } = new();

    public Task SendMessageToQueueAsync<T>(string queueName, T message,
        ServiceBusMessageOptions? options = null, CancellationToken cancellationToken = default)
    {
        SentMessages.Add((queueName, message!));
        return Task.CompletedTask;
    }

    // Implementar outros métodos conforme necessário...

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

// Uso em testes
[Test]
public async Task ProcessOrder_ShouldSendMessage()
{
    // Arrange
    var mockSender = new MockServiceBusMessageSender();
    var service = new OrderService(mockSender);
    var order = new Order { Id = 1, Total = 100m };

    // Act
    await service.ProcessOrder(order);

    // Assert
    Assert.That(mockSender.SentMessages, Has.Count.EqualTo(1));
    Assert.That(mockSender.SentMessages[0].Queue, Is.EqualTo("orders"));
}
```

### Usando Moq

```csharp
[Test]
public async Task ProcessOrder_ShouldSendMessage_UsingMoq()
{
    // Arrange
    var mockSender = new Mock<IServiceBusMessageSender>();
    var service = new OrderService(mockSender.Object);
    var order = new Order { Id = 1, Total = 100m };

    // Act
    await service.ProcessOrder(order);

    // Assert
    mockSender.Verify(x => x.SendMessageToQueueAsync(
        "orders",
        order,
        It.IsAny<ServiceBusMessageOptions>(),
        It.IsAny<CancellationToken>()),
        Times.Once);
}
```

## 🏗️ Arquitetura de Dependências

### Camadas da Aplicação

```
┌─────────────────────────────────────────┐
│           Application Layer             │
│  (Controllers, Services, Handlers)      │
│                   │                     │
│         depends on │                    │
│                   ▼                     │
├─────────────────────────────────────────┤
│         Abstraction Layer               │
│  (Interfaces, Models, Configuration)    │  ◀── Este pacote
│                   ▲                     │
│         implements │                    │
│                   │                     │
├─────────────────────────────────────────┤
│        Implementation Layer             │
│    (ServiceBusMessageSender, etc)       │
│                   │                     │
│         depends on │                    │
│                   ▼                     │
├─────────────────────────────────────────┤
│        Infrastructure Layer             │
│  (Azure.Messaging.ServiceBus, etc)      │
└─────────────────────────────────────────┘
```

### Benefícios desta Arquitetura

1. **Dependency Inversion**: Aplicação depende de abstrações, não de implementações
2. **Flexibilidade**: Pode trocar implementações sem alterar código da aplicação
3. **Testabilidade**: Facilita criação de mocks e testes isolados
4. **Deploy Independente**: Abstrações podem ser versionadas independentemente

## 📊 Dependências

Este pacote mantém as dependências essenciais para abstrações:

- **.NET Standard 2.1** - Máxima compatibilidade com diferentes frameworks
- **Azure.Messaging.ServiceBus** - SDK oficial do Azure para definições de tipos
- **Microsoft.Extensions.Logging.Abstractions** - Abstrações de logging
- **Microsoft.Extensions.Configuration.Abstractions** - Abstrações de configuração
- **Microsoft.Extensions.Options** - Sistema de opções do .NET
- **Microsoft.Extensions.DependencyInjection.Abstractions** - Abstrações de DI

### Por que essas dependências?

- **Azure.Messaging.ServiceBus**: Fornece tipos nativos (ServiceBusMessage, etc.) usados nas interfaces
- **Microsoft.Extensions.***: Padrões estabelecidos do ecossistema .NET para configuração e DI
- **.NET Standard 2.1**: Garante compatibilidade com .NET Framework, .NET Core e .NET 5+

## 🔄 Compatibilidade

### Versionamento

- **Major Version**: Mudanças incompatíveis nas interfaces públicas
- **Minor Version**: Adição de novos membros (sempre backward compatible)
- **Patch Version**: Correções de bugs e melhorias na documentação

### Breaking Changes

Mudanças que **quebram** compatibilidade:
- Remoção de métodos da interface
- Mudança de assinatura de métodos existentes
- Remoção de propriedades de configuração

Mudanças que **mantêm** compatibilidade:
- Adição de novos métodos com implementação padrão
- Adição de novas propriedades com valores padrão
- Adição de novos parâmetros opcionais

## 📄 Licença

Este projeto está licenciado sob a [Licença MIT](LICENSE).

## � Suporte

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

## �🔗 Pacotes Relacionados

- **[Nuuvify.CommonPack.AzureServiceBus](../Nuuvify.CommonPack.AzureServiceBus/)** - Implementação completa
- **[Nuuvify.CommonPack.UnitOfWork.Abstraction](../Nuuvify.CommonPack.UnitOfWork.Abstraction/)** - Padrão Unit of Work
- **[Nuuvify.CommonPack.Domain](../Nuuvify.CommonPack.Domain/)** - Abstrações de domínio
- **[Nuuvify.CommonPack.Security.Abstraction](../Nuuvify.CommonPack.Security.Abstraction/)** - Abstrações de segurança

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
