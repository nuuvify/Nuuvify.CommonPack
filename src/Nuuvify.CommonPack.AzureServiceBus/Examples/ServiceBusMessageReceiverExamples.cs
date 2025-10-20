// =====================================================================================
// 🔥 EXEMPLOS DE USO - ServiceBusMessageReceiver
// =====================================================================================
//
// Este arquivo contém exemplos de como usar a nova classe ServiceBusMessageReceiver.
// Esta classe pode ser usada em qualquer tipo de aplicação: Worker Service, Console App,
// API, etc.
//
// 🚀 COMO USAR:
// 1. Copie este arquivo para seu projeto
// 2. Ajuste os namespaces conforme necessário
// 3. Configure as dependências no Program.cs ou Startup.cs
// 4. Implemente sua lógica de processamento
//
// =====================================================================================

using Nuuvify.CommonPack.AzureServiceBus.Services.Receiver;

namespace Nuuvify.CommonPack.AzureServiceBus.Examples;

/// <summary>
/// Exemplo básico de implementação para processamento de pedidos
/// </summary>
public class PedidosMessageProcessor : ServiceBusMessageReceiver<PedidoContext>
{
    private readonly ILogger<PedidosMessageProcessor> _logger;

    public PedidosMessageProcessor(
        ILogger<PedidosMessageProcessor> logger,
        IConfigurationCustom configurationCustom,
        RequestConfiguration requestConfiguration)
        : base(logger, configurationCustom, requestConfiguration)
    {
        _logger = logger;

        // Configurar ActivitySource para telemetria
        ActivitySourceCustom = new ActivitySource("PedidosService");

        // Configurar comportamento em caso de falha
        AbandonMessageIfFailed = false; // Enviar para Dead Letter Queue em caso de falha
    }

    /// <summary>
    /// Configura e inicia o processamento de mensagens
    /// </summary>
    public async Task IniciarProcessamentoAsync(CancellationToken cancellationToken = default)
    {
        // Configurar Service Bus com Connection String para Topic
        ConfigureServiceBus(
            cnnName: "ServiceBus:Pedidos:ConnectionString",
            topicName: ConfigurationCustom.GetSectionValue("ServiceBus:Pedidos:TopicName"),
            subscription: ConfigurationCustom.GetSectionValue("ServiceBus:Pedidos:TopicSubscription"),
            serviceBusClientOptions: new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpTcp,
                RetryOptions = new ServiceBusRetryOptions
                {
                    MaxRetries = 5,
                    Delay = TimeSpan.FromSeconds(2)
                }
            },
            serviceBusProcessorOptions: new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 10,
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            });

        // Iniciar processamento
        await StartProcessingAsync(cancellationToken);
    }

    /// <summary>
    /// Implementa a lógica de processamento da mensagem
    /// </summary>
    public override async Task<bool> ExecuteReceivedMessageAsync(
        ServiceBusReceivedMessage message,
        ActivitySource activitySource,
        CancellationToken cancellationToken)
    {
        using var activity = activitySource?.StartActivity("ProcessarPedido");

        try
        {
            // Deserializar mensagem
            var pedidoData = JsonSerializer.Deserialize<PedidoMessage>(message.Body.ToString());

            if (pedidoData == null)
            {
                _logger.LogWarning("Mensagem {MessageId} não pôde ser deserializada", message.MessageId);
                return false;
            }

            _ = activity?.SetTag("pedido.id", pedidoData.PedidoId.ToString(System.Globalization.CultureInfo.InvariantCulture));
            _ = activity?.SetTag("pedido.cliente", pedidoData.ClienteId.ToString(System.Globalization.CultureInfo.InvariantCulture));

            _logger.LogInformation("Processando pedido {PedidoId} do cliente {ClienteId}",
                pedidoData.PedidoId, pedidoData.ClienteId);

            // Processar lógica de negócio
            await ProcessarPedido(pedidoData, cancellationToken);

            _logger.LogInformation("Pedido {PedidoId} processado com sucesso", pedidoData.PedidoId);

            return true; // Sucesso - mensagem será marcada como completa
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Erro ao deserializar mensagem {MessageId}: {Error}",
                message.MessageId, ex.Message);
            return false; // Falha de deserialização - enviar para DLQ
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Erro de validação ao processar mensagem {MessageId}: {Error}",
                message.MessageId, ex.Message);
            return false; // Falha de validação - enviar para DLQ
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Erro de operação ao processar mensagem {MessageId}: {Error}",
                message.MessageId, ex.Message);
            return false; // Falha operacional - comportamento depende de AbandonMessageIfFailed
        }
    }

    private async Task ProcessarPedido(PedidoMessage pedido, CancellationToken cancellationToken)
    {
        // Simular validação
        await Task.Delay(100, cancellationToken);

        if (pedido.Valor <= 0)
        {
            throw new ArgumentException("Valor do pedido deve ser maior que zero");
        }

        // Simular processamento
        await Task.Delay(500, cancellationToken);

        // Aqui você implementaria:
        // - Validar dados do pedido
        // - Salvar no banco de dados
        // - Enviar notificações
        // - Integrar com sistemas de pagamento
        // - Atualizar estoque
    }
}

/// <summary>
/// Exemplo para processamento de notificações usando Queue
/// </summary>
public class NotificacoesQueueProcessor : ServiceBusMessageReceiver<NotificacaoContext>
{
    private readonly ILogger<NotificacoesQueueProcessor> _logger;

    public NotificacoesQueueProcessor(
        ILogger<NotificacoesQueueProcessor> logger,
        IConfigurationCustom configurationCustom,
        RequestConfiguration requestConfiguration)
        : base(logger, configurationCustom, requestConfiguration)
    {
        _logger = logger;
        ActivitySourceCustom = new ActivitySource("NotificacoesService");
        AbandonMessageIfFailed = true; // Para notificações, tentar reprocessar
    }

    /// <summary>
    /// Configura para Queue em vez de Topic
    /// </summary>
    public async Task ConfigurarEIniciarAsync()
    {
        ConfigureServiceBus(
            cnnName: "ServiceBus:Notificacoes:ConnectionString",
            queueName: ConfigurationCustom.GetSectionValue("ServiceBus:Notificacoes:QueueName"),
            serviceBusClientOptions: new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets,
                RetryOptions = new ServiceBusRetryOptions
                {
                    MaxRetries = 3,
                    Delay = TimeSpan.FromSeconds(1)
                }
            },
            serviceBusProcessorOptions: new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 5,
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            });

        await StartProcessingAsync();
    }

    public override async Task<bool> ExecuteReceivedMessageAsync(
        ServiceBusReceivedMessage message,
        ActivitySource activitySource,
        CancellationToken cancellationToken)
    {
        using var activity = activitySource?.StartActivity("EnviarNotificacao");

        try
        {
            var notificacao = JsonSerializer.Deserialize<NotificacaoMessage>(message.Body.ToString());

            if (notificacao == null)
            {
                _logger.LogWarning("Mensagem de notificação {MessageId} inválida", message.MessageId);
                return false;
            }

            _ = activity?.SetTag("notificacao.tipo", notificacao.TipoNotificacao);
            _ = activity?.SetTag("notificacao.destinatario", notificacao.Destinatario);

            await EnviarNotificacao(notificacao, cancellationToken);

            _logger.LogInformation("Notificação {TipoNotificacao} enviada para {Destinatario}",
                notificacao.TipoNotificacao, notificacao.Destinatario);

            return true;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Erro ao deserializar notificação {MessageId}", message.MessageId);
            return false;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação {MessageId}", message.MessageId);
            return false; // Com AbandonMessageIfFailed = true, será abandonada para retry
        }
    }

    private async Task EnviarNotificacao(NotificacaoMessage notificacao, CancellationToken cancellationToken)
    {
        // Simular diferentes tipos de notificação
        switch (notificacao.TipoNotificacao?.ToUpperInvariant())
        {
            case "EMAIL":
                await EnviarEmail(notificacao, cancellationToken);
                break;
            case "SMS":
                await EnviarSms(notificacao, cancellationToken);
                break;
            case "PUSH":
                await EnviarPushNotification(notificacao, cancellationToken);
                break;
            default:
                throw new InvalidOperationException($"Tipo de notificação não suportado: {notificacao.TipoNotificacao}");
        }
    }

    private async Task EnviarEmail(NotificacaoMessage notificacao, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Enviando email para {Destinatario}", notificacao.Destinatario);
        await Task.Delay(200, cancellationToken); // Simular envio
    }

    private async Task EnviarSms(NotificacaoMessage notificacao, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Enviando SMS para {Destinatario}", notificacao.Destinatario);
        await Task.Delay(150, cancellationToken); // Simular envio
    }

    private async Task EnviarPushNotification(NotificacaoMessage notificacao, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Enviando push notification para {Destinatario}", notificacao.Destinatario);
        await Task.Delay(100, cancellationToken); // Simular envio
    }
}

/// <summary>
/// Exemplo usando Azure Credentials em vez de connection string
/// </summary>
public class EventosAzureCredentialsProcessor : ServiceBusMessageReceiver<EventoContext>
{
    private readonly ILogger<EventosAzureCredentialsProcessor> _logger;

    public EventosAzureCredentialsProcessor(
        ILogger<EventosAzureCredentialsProcessor> logger,
        IConfigurationCustom configurationCustom,
        RequestConfiguration requestConfiguration)
        : base(logger, configurationCustom, requestConfiguration)
    {
        _logger = logger;
        ActivitySourceCustom = new ActivitySource("EventosService");
    }

    /// <summary>
    /// Configura usando DefaultAzureCredential
    /// </summary>
    public async Task ConfigurarComAzureCredentialsAsync()
    {
        // Usar DefaultAzureCredential para autenticação automática
        // Nota: Adicione a referência "Azure.Identity" ao seu projeto para usar este exemplo
        // var credential = new Azure.Identity.DefaultAzureCredential();

        // Por enquanto, use connection string como alternativa
        var connectionString = ConfigurationCustom.GetSectionValue("ServiceBus:Eventos:ConnectionString");

        if (!string.IsNullOrEmpty(connectionString))
        {
            ConfigureServiceBus(
                cnnName: "ServiceBus:Eventos:ConnectionString",
                topicName: ConfigurationCustom.GetSectionValue("ServiceBus:Eventos:TopicName"),
                subscription: ConfigurationCustom.GetSectionValue("ServiceBus:Eventos:Subscription"));
        }
        else
        {
            throw new InvalidOperationException("Para usar Azure Credentials, adicione a referência 'Azure.Identity' e descomente o código apropriado.");
        }


        await StartProcessingAsync();
    }

    public override async Task<bool> ExecuteReceivedMessageAsync(
        ServiceBusReceivedMessage message,
        ActivitySource activitySource,
        CancellationToken cancellationToken)
    {
        using var activity = activitySource?.StartActivity("ProcessarEvento");

        try
        {
            var evento = JsonSerializer.Deserialize<EventoSistema>(message.Body.ToString());

            if (evento == null)
            {
                _logger.LogWarning("Evento {MessageId} inválido", message.MessageId);
                return false;
            }

            _ = activity?.SetTag("evento.tipo", evento.TipoEvento);
            _ = activity?.SetTag("evento.origem", evento.Origem);

            _logger.LogInformation("Processando evento {TipoEvento} de {Origem}",
                evento.TipoEvento, evento.Origem);

            await ProcessarEvento(evento, cancellationToken);

            return true;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Erro ao deserializar evento {MessageId}", message.MessageId);
            return false;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Erro ao processar evento {MessageId}", message.MessageId);
            return false;
        }
    }

    private async Task ProcessarEvento(EventoSistema evento, CancellationToken cancellationToken)
    {
        // Simular processamento baseado no tipo de evento
        switch (evento.TipoEvento)
        {
            case "UsuarioCriado":
                await ProcessarUsuarioCriado(evento, cancellationToken);
                break;
            case "PedidoFinalizado":
                await ProcessarPedidoFinalizado(evento, cancellationToken);
                break;
            case "PagamentoProcessado":
                await ProcessarPagamentoProcessado(evento, cancellationToken);
                break;
            default:
                _logger.LogWarning("Tipo de evento não reconhecido: {TipoEvento}", evento.TipoEvento);
                break;
        }
    }

    private async Task ProcessarUsuarioCriado(EventoSistema evento, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processando criação de usuário: {EventoId}", evento.TipoEvento);
        await Task.Delay(100, cancellationToken);
        // Implementar: criar perfil, enviar email de boas-vindas, etc.
    }

    private async Task ProcessarPedidoFinalizado(EventoSistema evento, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processando finalização de pedido: {EventoId}", evento.TipoEvento);
        await Task.Delay(150, cancellationToken);
        // Implementar: atualizar estoque, gerar nota fiscal, etc.
    }

    private async Task ProcessarPagamentoProcessado(EventoSistema evento, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processando confirmação de pagamento: {EventoId}", evento.TipoEvento);
        await Task.Delay(200, cancellationToken);
        // Implementar: liberar produtos, enviar confirmação, etc.
    }
}

/// <summary>
/// Exemplo para uso em Console Application
/// </summary>
public class ConsoleServiceBusReceiver
{
    private readonly PedidosMessageProcessor _pedidosProcessor;
    private readonly NotificacoesQueueProcessor _notificacoesProcessor;
    private readonly ILogger<ConsoleServiceBusReceiver> _logger;

    public ConsoleServiceBusReceiver(
        PedidosMessageProcessor pedidosProcessor,
        NotificacoesQueueProcessor notificacoesProcessor,
        ILogger<ConsoleServiceBusReceiver> logger)
    {
        _pedidosProcessor = pedidosProcessor;
        _notificacoesProcessor = notificacoesProcessor;
        _logger = logger;
    }

    /// <summary>
    /// Executa o processamento das mensagens
    /// </summary>
    public async Task ExecuteAsync()
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        // Configurar cancelamento por Ctrl+C
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        try
        {
            _logger.LogInformation("Iniciando processadores de mensagens...");

            // Iniciar múltiplos processadores
            var tasks = new[]
            {
                _pedidosProcessor.IniciarProcessamentoAsync(cancellationTokenSource.Token),
                _notificacoesProcessor.ConfigurarEIniciarAsync()
            };

            await Task.WhenAll(tasks);

            _logger.LogInformation("Todos os processadores iniciados. Pressione Ctrl+C para parar.");

            // Aguardar até cancelamento
            await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Processamento cancelado pelo usuário.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Erro de configuração durante o processamento: {Error}", ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Erro de parâmetros durante o processamento: {Error}", ex.Message);
        }
        finally
        {
            await FinalizarProcessadores();
        }
    }

    private async Task FinalizarProcessadores()
    {
        try
        {
            var stopTasks = new[]
            {
                _pedidosProcessor.StopProcessingAsync(),
                _notificacoesProcessor.StopProcessingAsync()
            };

            await Task.WhenAll(stopTasks);

            var disposeTasks = new[]
            {
                _pedidosProcessor.DisposeAsync().AsTask(),
                _notificacoesProcessor.DisposeAsync().AsTask()
            };

            await Task.WhenAll(disposeTasks);

            _logger.LogInformation("Todos os processadores finalizados.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Erro ao finalizar processadores: {Error}", ex.Message);
        }
    }
}

#region Models de Exemplo

/// <summary>
/// Modelo para mensagens de pedido
/// </summary>
public class PedidoMessage
{
    public int PedidoId { get; set; }
    public int ClienteId { get; set; }
    public decimal Valor { get; set; }
    public DateTime DataPedido { get; set; }
    public IReadOnlyList<ItemPedido> Itens { get; set; } = Array.Empty<ItemPedido>();
}

/// <summary>
/// Modelo para item de pedido
/// </summary>
public class ItemPedido
{
    public int ProdutoId { get; set; }
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
}

/// <summary>
/// Modelo para mensagens de notificação
/// </summary>
public class NotificacaoMessage
{
    public string Destinatario { get; set; } = string.Empty;
    public string Assunto { get; set; } = string.Empty;
    public string Conteudo { get; set; } = string.Empty;
    public string TipoNotificacao { get; set; } = string.Empty; // Email, SMS, Push
}

/// <summary>
/// Modelo para eventos do sistema
/// </summary>
public class EventoSistema
{
    public string TipoEvento { get; set; } = string.Empty;
    public string Origem { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Dados { get; set; } = new();
}

// Contextos para diferentes tipos de processamento
public class PedidoContext { }
public class NotificacaoContext { }
public class EventoContext { }

#endregion

/// <summary>
/// Exemplo de configuração no Program.cs para Console Application
/// </summary>
public static class ProgramExample
{
    public static void ConfigureServices()
    {
        /*
        // Exemplo de configuração no Program.cs
        var builder = WebApplication.CreateBuilder(args);

        // Configurar logging
        builder.Logging.AddConsole();

        // Registrar dependências
        builder.Services.AddScoped<RequestConfiguration>();
        builder.Services.AddScoped<IConfigurationCustom, ConfigurationCustom>();

        // Registrar processadores
        builder.Services.AddScoped<PedidosMessageProcessor>();
        builder.Services.AddScoped<NotificacoesQueueProcessor>();
        builder.Services.AddScoped<EventosAzureCredentialsProcessor>();
        builder.Services.AddScoped<ConsoleServiceBusReceiver>();

        var app = builder.Build();

        // Para Console Application
        var receiver = app.Services.GetRequiredService<ConsoleServiceBusReceiver>();
        await receiver.ExecuteAsync();
        */
    }
}
