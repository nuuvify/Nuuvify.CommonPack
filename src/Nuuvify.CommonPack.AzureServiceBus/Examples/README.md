# 📚 Exemplos de Uso - Nuuvify.CommonPack.AzureServiceBus

**Obrigado por instalar o Nuuvify.CommonPack.AzureServiceBus!** 🎉

Este arquivo contém exemplos práticos de como usar o **Nuuvify.CommonPack.AzureServiceBus** em diferentes cenários.

## 📁 **Arquivos Incluídos**

Após a instalação do pacote NuGet, você encontrará:

- **`Examples/ServiceBusUsageExamples.cs`** - Classe completa com 20+ exemplos para envio de mensagens
- **`Examples/ServiceBusMessageReceiverExamples.cs`** - Exemplos completos para recebimento de mensagens
- **`Examples/README.md`** - Esta documentação detalhada dos exemplos

## 🚀 Como Usar os Exemplos

### 1. **Configuração Inicial**

Primeiro, configure o Azure Service Bus no seu projeto:

```csharp
// Program.cs (.NET 8)
using Nuuvify.CommonPack.AzureServiceBus.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Registra o Azure Service Bus
builder.Services.AddAzureServiceBus(builder.Configuration);

// Registra a classe de exemplos (opcional)
builder.Services.AddTransient<ServiceBusUsageExamples>();

var app = builder.Build();
```

### 3. **Uso Básico - Primeiro Exemplo**

```csharp
public class MeuService
{
    private readonly IServiceBusMessageSender _serviceBus;

    public MeuService(IServiceBusMessageSender serviceBus)
    {
        _serviceBus = serviceBus;
    }

    public async Task EnviarMensagem()
    {
        await _serviceBus.SendMessageToQueueAsync("minha-queue", new { Dados = "teste" });
    }
}
```

### 2. **Configuração no appsettings.json**

```json
{
  "ServiceBusConfiguration": {
    "ConnectionString": "Endpoint=sb://seu-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=sua-chave",
    "OperationTimeoutSeconds": 30,
    "MaxRetryAttempts": 3,
    "RetryDelaySeconds": 5,
    "MaxBatchSize": 100
  },
  "ServiceBus": {
    "Pedidos": {
      "ConnectionString": "Endpoint=sb://pedidos-namespace.servicebus.windows.net/;...",
      "TopicName": "pedidos-topic",
      "TopicSubscription": "processador-pedidos"
    },
    "Notificacoes": {
      "ConnectionString": "Endpoint=sb://notificacoes-namespace.servicebus.windows.net/;...",
      "QueueName": "notificacoes-queue"
    }
  }
}
```

### 3. **Novo! ServiceBusMessageReceiver - Recebimento de Mensagens**

A nova classe `ServiceBusMessageReceiver<T>` permite receber e processar mensagens de forma agnóstica ao tipo de aplicação:

```csharp
// Exemplo básico de implementação
public class MeuProcessador : ServiceBusMessageReceiver<MeuContexto>
{
    public MeuProcessador(
        ILogger<MeuProcessador> logger,
        IConfigurationCustom config,
        RequestConfiguration requestConfig)
        : base(logger, config, requestConfig)
    {
        ActivitySourceCustom = new ActivitySource("MeuService");
    }

    public override async Task<bool> ExecuteReceivedMessageAsync(
        ServiceBusReceivedMessage message,
        ActivitySource? activitySource,
        CancellationToken cancellationToken)
    {
        // Sua lógica de processamento aqui
        var dados = JsonSerializer.Deserialize<MeuModelo>(message.Body.ToString());
        await ProcessarDados(dados);
        return true; // Sucesso
    }
}

// Uso em Console Application
public async Task Main(string[] args)
{
    // Configurar DI
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddScoped<MeuProcessador>();

    var provider = services.BuildServiceProvider();
    var processador = provider.GetService<MeuProcessador>();

    // Configurar e iniciar
    await processador.IniciarProcessamentoAsync();

    // Aguardar Ctrl+C
    Console.ReadKey();

    // Parar e limpar
    await processador.StopProcessingAsync();
    await processador.DisposeAsync();
}
```

## 📋 Exemplos Disponíveis

## 🔥 **NOVIDADE: Recebimento de Mensagens**

### 📥 **ServiceBusMessageReceiver**
- `PedidosMessageProcessor` - Processamento de pedidos com Topic
- `NotificacoesQueueProcessor` - Processamento de notificações com Queue
- `EventosAzureCredentialsProcessor` - Usando Azure Credentials
- `ConsoleServiceBusReceiver` - Uso em Console Application

### � **Envio de Mensagens (ServiceBusMessageSender)**
- `ExemploEnvioSimpleQueue()` - Envio simples para fila
- `ExemploEnvioComOpcoes()` - Envio com opções customizadas
- `ExemploEnvioTopic()` - Envio para tópico
- `ExemploEnvioLote()` - Operações em lote

### ⏰ **Agendamento**
- `ExemploAgendamento()` - Agendamento básico de mensagens
- `ExemploAgendamentoComCancelamento()` - Agendamento com cancelamento

### 🏢 **Empresariais**
- `ExemploMultiTenant()` - Cenários multi-tenant
- `ExemploComPrioridades()` - Diferentes prioridades de mensagem
- `ExemploAmbienteCorporativo()` - Configurações corporativas

### ⚡ **Avançados**
- `ExemploComClienteCustomizado()` - Cliente Service Bus customizado
- `ExemploComClienteFactory()` - Factory personalizada
- `ExemploReuseConnections()` - Otimização de performance

### 🛡️ **Robustez**
- `ExemploComTratamentoErro()` - Tratamento de exceções
- `ExemploComCancellation()` - Uso de CancellationToken

## 🎯 **Casos de Uso por Exemplo**

### 📤 **Envio de Mensagens**
| Exemplo                   | Cenário               | Quando Usar                        |
| ------------------------- | --------------------- | ---------------------------------- |
| `ExemploEnvioSimpleQueue` | Envio básico          | Primeiros passos, testes           |
| `ExemploEnvioLote`        | Alta throughput       | Processamento em massa             |
| `ExemploMultiTenant`      | SaaS multi-tenant     | Aplicações multi-cliente           |
| `ExemploComPrioridades`   | Processamento crítico | Mensagens com diferentes urgências |
| `ExemploReuseConnections` | Otimização            | Alta performance, muitas operações |

### 📥 **Recebimento de Mensagens**
| Exemplo                            | Cenário                  | Quando Usar                              |
| ---------------------------------- | ------------------------ | ---------------------------------------- |
| `PedidosMessageProcessor`          | Processamento de negócio | Worker Services, processamento batch     |
| `NotificacoesQueueProcessor`       | Notificações assíncronas | Envio de emails, SMS, push notifications |
| `EventosAzureCredentialsProcessor` | Eventos do sistema       | Arquiteturas orientadas a eventos        |
| `ConsoleServiceBusReceiver`        | Aplicações console       | Scripts, ferramentas administrativas     |

## 🔧 **Personalização**

Você pode copiar estes exemplos para seu projeto e adaptá-los conforme necessário:

1. **Copie** o arquivo `ServiceBusUsageExamples.cs` para seu projeto
2. **Ajuste** os namespaces conforme sua estrutura
3. **Modifique** os exemplos para suas necessidades específicas
4. **Adicione** seus próprios exemplos baseados nos padrões mostrados

## 📖 **Documentação Adicional**

- [README Principal](../README.md) - Documentação completa da biblioteca
- [CHANGELOG](../CHANGELOG.md) - Histórico de versões
- [Azure Service Bus Docs](https://docs.microsoft.com/azure/service-bus-messaging/) - Documentação oficial Microsoft

## ❓ **Precisa de Ajuda?**

- 📧 **Email**: [suporte@nuuvify.com](mailto:suporte@nuuvify.com)
- 📋 **Issues**: [GitHub Issues](https://github.com/nuuvify/Nuuvify.CommonPack/issues)
- 💬 **Discussões**: [GitHub Discussions](https://github.com/nuuvify/Nuuvify.CommonPack/discussions)

---

> **💡 Dica**: Execute os exemplos em ambiente de desenvolvimento primeiro para entender o comportamento antes de usar em produção.

---

Desenvolvido com ❤️ pela equipe **Nuuvify**.
