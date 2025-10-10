using Nuuvify.CommonPack.AzureServiceBus.Abstraction.Interfaces;
using Nuuvify.CommonPack.AzureServiceBus.Abstraction.Models;

namespace Nuuvify.CommonPack.AzureServiceBus.Examples;

/// <summary>
/// Exemplos de uso do ServiceBusMessageSender
/// </summary>
public class ServiceBusUsageExamples
{
    private readonly IServiceBusMessageSender _serviceBusMessageSender;
    private readonly ILogger<ServiceBusUsageExamples> _logger;
    public ServiceBusUsageExamples(
        IServiceBusMessageSender serviceBusMessageSender, ILogger<ServiceBusUsageExamples> logger)
    {
        _serviceBusMessageSender = serviceBusMessageSender;
        _logger = logger;
    }

    /// <summary>
    /// Exemplo de envio simples para queue
    /// </summary>
    public async Task ExemploEnvioSimpleQueue()
    {
        var mensagem = new { Id = 1, Nome = "Teste", Data = DateTime.Now };

        await _serviceBusMessageSender.SendMessageToQueueAsync(
            queueName: "minha-queue",
            message: mensagem);
    }

    /// <summary>
    /// Exemplo de envio com opções customizadas
    /// </summary>
    public async Task ExemploEnvioComOpcoes()
    {
        var mensagem = new { Id = 1, Nome = "Teste", Data = DateTime.Now };

        var options = new ServiceBusMessageOptions
        {
            MessageId = Guid.NewGuid().ToString(),
            CorrelationId = "correlacao-123",
            Label = "ProcessamentoPagamento",
            TimeToLive = TimeSpan.FromMinutes(30),
            ApplicationProperties =
            {
                ["TipoProcessamento"] = "Pagamento",
                ["Prioridade"] = "Alta",
                ["Versao"] = "1.0"
            }
        };

        await _serviceBusMessageSender.SendMessageToQueueAsync(
            queueName: "pagamentos-queue",
            message: mensagem,
            messageOptions: options);
    }

    /// <summary>
    /// Exemplo de envio para tópico
    /// </summary>
    public async Task ExemploEnvioTopic()
    {
        var evento = new
        {
            EventoId = Guid.NewGuid(),
            Tipo = "PagamentoCriado",
            Dados = new { ValorTotal = 1000.50m, Cliente = "João Silva" }
        };

        var options = new ServiceBusMessageOptions
        {
            Label = "PagamentoCriado",
            PartitionKey = "pagamentos"
        };

        await _serviceBusMessageSender.SendMessageToTopicAsync(
            topicName: "eventos-pagamento",
            message: evento,
            messageOptions: options);
    }

    /// <summary>
    /// Exemplo de envio em lote
    /// </summary>
    public async Task ExemploEnvioLote()
    {
        var mensagens = new List<object>();

        for (int i = 1; i <= 10; i++)
        {
            mensagens.Add(new { Id = i, Processado = false, Timestamp = DateTime.Now });
        }

        await _serviceBusMessageSender.SendBatchMessagesToQueueAsync(
            queueName: "processamento-lote",
            messages: mensagens);
    }

    /// <summary>
    /// Exemplo de agendamento de mensagem
    /// </summary>
    public async Task ExemploAgendamento()
    {
        var lembreteCobranca = new
        {
            ClienteId = 123,
            ValorDevido = 500.00m,
            DataVencimento = DateTime.Today.AddDays(-5)
        };

        // Agenda para enviar em 1 hora
        var agendadoPara = DateTimeOffset.Now.AddHours(1);

        _ = await _serviceBusMessageSender.ScheduleMessageToQueueAsync(
            queueName: "lembretes-cobranca",
            message: lembreteCobranca,
            scheduledEnqueueTime: agendadoPara);
    }

    /// <summary>
    /// Exemplo com tratamento de erro
    /// </summary>
    public async Task ExemploComTratamentoErro()
    {
        try
        {
            var mensagem = new { Dados = "importante" };

            await _serviceBusMessageSender.SendMessageToQueueAsync(
                queueName: "queue-critica",
                message: mensagem);

            _logger.LogInformation("Mensagem enviada com sucesso!");
        }
        catch (InvalidOperationException ex)
        {
            // Erro específico do Service Bus
            _logger.LogError(ex, "Erro do Service Bus: {Message}", ex.Message);
            // Implementar lógica de fallback ou notificação
        }
        catch (ArgumentException ex)
        {
            // Erro de parâmetros
            _logger.LogError(ex, "Erro de parâmetros: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            // Outros erros
            _logger.LogError(ex, "Erro inesperado: {Message}", ex.Message);
        }
    }

    /// <summary>
    /// Exemplo com cancellation token
    /// </summary>
    public async Task ExemploComCancellation(CancellationToken cancellationToken)
    {
        var mensagem = new { Status = "Processando" };

        try
        {
            await _serviceBusMessageSender.SendMessageToQueueAsync(
                queueName: "processamento",
                message: mensagem,
                cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Operação foi cancelada");
        }
    }

    /// <summary>
    /// Exemplo de envio com ServiceBusOperationOptions (cliente customizado)
    /// </summary>
    public async Task ExemploComClienteCustomizado()
    {
        var mensagem = new { Id = 1, Prioridade = "Alta" };

        // Opções para usar cliente customizado
        var operationOptions = new ServiceBusOperationOptions
        {
            CustomConnectionString = "Endpoint=sb://priority-namespace.servicebus.windows.net/;...",
            CustomClientOptions = new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpTcp,
                RetryOptions = new ServiceBusRetryOptions
                {
                    MaxRetries = 10,
                    Delay = TimeSpan.FromMilliseconds(500)
                }
            },
            UseTemporaryClient = true
        };

        var messageOptions = new ServiceBusMessageOptions
        {
            Subject = "HighPriorityMessage",
            ApplicationProperties = { ["Priority"] = "High" }
        };

        await _serviceBusMessageSender.SendMessageToQueueAsync(
            "high-priority-queue",
            mensagem,
            messageOptions,
            operationOptions);
    }

    /// <summary>
    /// Exemplo de uso com cliente pré-configurado via factory
    /// </summary>
    public async Task ExemploComClienteFactory()
    {
        var mensagem = new { Data = "Dados importantes" };

        // Factory para criar cliente customizado
        var operationOptions = new ServiceBusOperationOptions
        {
            ClientFactory = () => new ServiceBusClient(
                "Endpoint=sb://custom-namespace.servicebus.windows.net/;...",
                new ServiceBusClientOptions
                {
                    TransportType = ServiceBusTransportType.AmqpWebSockets,
                    RetryOptions = new ServiceBusRetryOptions
                    {
                        MaxRetries = 3,
                        Delay = TimeSpan.FromSeconds(1),
                        Mode = ServiceBusRetryMode.Fixed
                    }
                }),
            UseTemporaryClient = true
        };

        await _serviceBusMessageSender.SendMessageToQueueAsync(
            "custom-queue",
            mensagem,
            operationOptions: operationOptions);
    }

    /// <summary>
    /// Exemplo multi-tenant - diferentes configurações por tenant
    /// </summary>
    public async Task ExemploMultiTenant(string tenantId, object mensagem)
    {
        // Connection string específica por tenant
        string connectionString = GetConnectionStringForTenant(tenantId);

        var operationOptions = new ServiceBusOperationOptions
        {
            CustomConnectionString = connectionString,
            CustomClientOptions = new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpTcp,
                RetryOptions = new ServiceBusRetryOptions
                {
                    MaxRetries = 10,
                    Delay = TimeSpan.FromMilliseconds(500)
                }
            },
            UseTemporaryClient = true
        };

        var messageOptions = new ServiceBusMessageOptions
        {
            Subject = $"TenantMessage-{tenantId}",
            ApplicationProperties = {
                ["TenantId"] = tenantId,
                ["Priority"] = "High",
                ["Environment"] = "Production"
            }
        };

        await _serviceBusMessageSender.SendMessageToQueueAsync(
            $"tenant-{tenantId}-queue",
            mensagem,
            messageOptions,
            operationOptions);
    }

    /// <summary>
    /// Exemplo com diferentes prioridades de mensagem
    /// </summary>
    public async Task ExemploComPrioridades(object mensagem, MessagePriority prioridade)
    {
        ServiceBusOperationOptions operationOptions;
        ServiceBusMessageOptions messageOptions;

        switch (prioridade)
        {
            case MessagePriority.High:
                // Alta prioridade: cliente otimizado com retry rápido
                operationOptions = new ServiceBusOperationOptions
                {
                    CustomClientOptions = new ServiceBusClientOptions
                    {
                        RetryOptions = new ServiceBusRetryOptions
                        {
                            MaxRetries = 10,
                            Delay = TimeSpan.FromMilliseconds(100),
                            Mode = ServiceBusRetryMode.Exponential
                        }
                    },
                    CustomConnectionString = HighPriorityConnectionString,
                    UseTemporaryClient = true
                };

                messageOptions = new ServiceBusMessageOptions
                {
                    Subject = "HighPriority",
                    ApplicationProperties = { ["Priority"] = "High" },
                    TimeToLive = TimeSpan.FromMinutes(30)
                };

                await _serviceBusMessageSender.SendMessageToQueueAsync(
                    "high-priority-queue",
                    mensagem,
                    messageOptions,
                    operationOptions);
                break;

            case MessagePriority.Normal:
                messageOptions = new ServiceBusMessageOptions
                {
                    Subject = "NormalPriority",
                    ApplicationProperties = { ["Priority"] = "Normal" },
                    TimeToLive = TimeSpan.FromHours(2)
                };

                await _serviceBusMessageSender.SendMessageToQueueAsync(
                    "normal-queue",
                    mensagem,
                    messageOptions);
                break;

            case MessagePriority.Low:
                messageOptions = new ServiceBusMessageOptions
                {
                    Subject = "LowPriority",
                    ApplicationProperties = { ["Priority"] = "Low" },
                    TimeToLive = TimeSpan.FromDays(1)
                };

                await _serviceBusMessageSender.SendMessageToQueueAsync(
                    "low-priority-queue",
                    mensagem,
                    messageOptions);
                break;
        }
    }

    /// <summary>
    /// Exemplo com configuração para ambiente corporativo (proxy, certificados)
    /// </summary>
    public async Task ExemploAmbienteCorporativo()
    {
        var mensagem = new { Sistema = "ERP", Modulo = "Financeiro" };

        var operationOptions = new ServiceBusOperationOptions
        {
            CustomClientOptions = new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets, // WebSockets para atravessar firewall
                RetryOptions = new ServiceBusRetryOptions
                {
                    MaxRetries = 5,
                    Delay = TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(30),
                    Mode = ServiceBusRetryMode.Exponential
                },
                // Para ambientes com proxy corporativo
                WebProxy = new System.Net.WebProxy("http://proxy.empresa.com:8080")
            },
            UseTemporaryClient = true
        };

        var messageOptions = new ServiceBusMessageOptions
        {
            Subject = "CorporateMessage",
            ApplicationProperties = {
                ["Department"] = "Finance",
                ["Classification"] = "Internal",
                ["ComplianceRequired"] = "true"
            }
        };

        await _serviceBusMessageSender.SendMessageToQueueAsync(
            "corporate-systems-queue",
            mensagem,
            messageOptions,
            operationOptions);
    }

    /// <summary>
    /// Exemplo de envio em lote com configurações avançadas
    /// </summary>
    public async Task ExemploLoteAvancado()
    {
        var mensagens = new List<object>();

        // Criar lote de mensagens
        for (int i = 1; i <= 100; i++)
        {
            mensagens.Add(new
            {
                Id = i,
                ProcessedAt = DateTime.UtcNow,
                BatchId = Guid.NewGuid(),
                Data = $"Dados do item {i}"
            });
        }

        var operationOptions = new ServiceBusOperationOptions
        {
            CustomClientOptions = new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpTcp, // TCP para melhor throughput
                RetryOptions = new ServiceBusRetryOptions
                {
                    MaxRetries = 3,
                    Delay = TimeSpan.FromSeconds(1)
                }
            }
        };

        var messageOptions = new ServiceBusMessageOptions
        {
            Subject = "BatchProcessing",
            ApplicationProperties = {
                ["BatchSize"] = mensagens.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["ProcessingType"] = "Bulk",
                ["CreatedAt"] = DateTime.UtcNow.ToString("O")
            }
        };

        await _serviceBusMessageSender.SendBatchMessagesToQueueAsync(
            "bulk-processing-queue",
            mensagens,
            messageOptions,
            operationOptions);
    }

    /// <summary>
    /// Exemplo de agendamento com cancelamento
    /// </summary>
    public async Task ExemploAgendamentoComCancelamento()
    {
        var lembrete = new
        {
            ClienteId = 456,
            TipoLembrete = "Vencimento",
            Detalhes = "Fatura vence em 3 dias"
        };

        // Agendar para 3 dias
        var dataAgendamento = DateTimeOffset.Now.AddDays(3);

        var messageOptions = new ServiceBusMessageOptions
        {
            Subject = "ReminderScheduled",
            ApplicationProperties = {
                ["ReminderType"] = "PaymentDue",
                ["ScheduledFor"] = dataAgendamento.ToString("O")
            }
        };

        // Agendar mensagem
        var sequenceNumber = await _serviceBusMessageSender.ScheduleMessageToQueueAsync(
            "lembretes-agendados",
            lembrete,
            dataAgendamento,
            messageOptions);

        _logger.LogInformation("Lembrete agendado com sequence number: {SequenceNumber}", sequenceNumber);

        // Simular cancelamento após algumas condições
        if (ClientePagouAntecipado(456))
        {
            await _serviceBusMessageSender.CancelScheduledMessageInQueueAsync(
                "lembretes-agendados",
                sequenceNumber);

            _logger.LogInformation("Lembrete cancelado - cliente pagou antecipadamente");
        }
    }

    /// <summary>
    /// Exemplo de publicação em tópico com múltiplas configurações
    /// </summary>
    public async Task ExemploTopicAvancado()
    {
        var evento = new
        {
            EventoId = Guid.NewGuid(),
            TipoEvento = "PedidoCriado",
            Timestamp = DateTime.UtcNow,
            Dados = new
            {
                PedidoId = 12345,
                ClienteId = 789,
                Valor = 1500.00m,
                Status = "Pendente"
            }
        };

        var operationOptions = new ServiceBusOperationOptions
        {
            CustomClientOptions = new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpTcp,
                RetryOptions = new ServiceBusRetryOptions
                {
                    MaxRetries = 5,
                    Delay = TimeSpan.FromSeconds(1)
                }
            }
        };

        var messageOptions = new ServiceBusMessageOptions
        {
            Subject = "PedidoCriado",
            PartitionKey = "pedidos", // Para ordenação dentro da partição
            CorrelationId = $"pedido-{evento.Dados.PedidoId}",
            ApplicationProperties = {
                ["EventType"] = "OrderCreated",
                ["Source"] = "OrderService",
                ["Version"] = "2.0",
                ["CustomerId"] = evento.Dados.ClienteId.ToString(System.Globalization.CultureInfo.InvariantCulture)
            }
        };

        await _serviceBusMessageSender.SendMessageToTopicAsync(
            "eventos-pedidos",
            evento,
            messageOptions,
            operationOptions);
    }

    #region Métodos Helper

    /// <summary>
    /// Simula obtenção de connection string por tenant
    /// </summary>
    private static string GetConnectionStringForTenant(string tenantId)
    {
        // Em implementação real, viria de configuração ou Key Vault
        return tenantId switch
        {
            "tenant-premium" => "Endpoint=sb://premium-namespace.servicebus.windows.net/;...",
            "tenant-standard" => "Endpoint=sb://standard-namespace.servicebus.windows.net/;...",
            _ => "Endpoint=sb://default-namespace.servicebus.windows.net/;..."
        };
    }

    /// <summary>
    /// Connection string para alta prioridade
    /// </summary>
    private const string HighPriorityConnectionString = "Endpoint=sb://high-priority-namespace.servicebus.windows.net/;...";

    /// <summary>
    /// Simula verificação se cliente pagou antecipadamente
    /// </summary>
    private static bool ClientePagouAntecipado(int clienteId)
    {
        // Simulação - em implementação real consultaria base de dados
        return clienteId % 2 == 0; // Simula que clientes com ID par pagaram antecipado
    }

    /// <summary>
    /// Exemplo demonstrando o impacto do ReuseConnections na performance
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representando a operação assíncrona</returns>
    /// <remarks>
    /// <para>Este exemplo demonstra como o ReuseConnections otimiza performance:</para>
    /// <list type="bullet">
    /// <item><description>ReuseConnections = true (padrão): Clientes são armazenados em cache</description></item>
    /// <item><description>Múltiplas operações com configurações idênticas reutilizam o mesmo cliente</description></item>
    /// <item><description>Cache é baseado em: ConnectionString + TransportType + RetryOptions</description></item>
    /// <item><description>Reduz overhead de criação/destruição de conexões TCP</description></item>
    /// </list>
    ///
    /// <para>Para configurar ReuseConnections na DI:</para>
    /// <code>
    /// services.AddAzureServiceBusWithClientConfiguration(config, clientConfig =>
    /// {
    ///     clientConfig.ReuseConnections = true; // Padrão
    /// });
    /// </code>
    /// </remarks>
    public async Task ExemploReuseConnections(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("=== Exemplo ReuseConnections - Otimização de Performance ===");

        // Configuração com ReuseConnections habilitado (padrão)
        var optionsComCache = new ServiceBusOperationOptions
        {
            CustomConnectionString = "Endpoint=sb://exemplo-com-cache.servicebus.windows.net/;...",
            CustomClientOptions = new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets,
                RetryOptions = new ServiceBusRetryOptions
                {
                    MaxRetries = 3,
                    Mode = ServiceBusRetryMode.Exponential
                }
            }
        };

        // Simula múltiplas operações que se beneficiam do cache de clientes
        for (int i = 1; i <= 5; i++)
        {
            var mensagem = new
            {
                Id = i,
                ProcessoId = $"PROC-{i:000}",
                Timestamp = DateTimeOffset.UtcNow,
                Dados = $"Processamento batch {i}"
            };

            try
            {
                // Com ReuseConnections = true, o mesmo cliente será reutilizado
                // para todas essas operações com configurações idênticas
                await _serviceBusMessageSender.SendMessageToQueueAsync(
                    queueName: "queue-otimizada",
                    message: mensagem,
                    operationOptions: optionsComCache,
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Mensagem {MessageId} enviada (cliente reutilizado)", i);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar mensagem {MessageId}", i);
            }

            // Pequeno delay para simular processamento
            await Task.Delay(100, cancellationToken);
        }

        _logger.LogInformation("=== Comparação: Sem cache de cliente ===");

        // Para demonstrar diferença, simula cenário onde cada operação cria novo cliente
        // (comportamento quando ReuseConnections = false na ServiceBusClientConfiguration)
        for (int i = 1; i <= 3; i++)
        {
            // Cria opções ligeiramente diferentes para evitar cache
            var optionsSemCache = new ServiceBusOperationOptions
            {
                CustomConnectionString = $"Endpoint=sb://exemplo-sem-cache-{i}.servicebus.windows.net/;...",
                CustomClientOptions = new ServiceBusClientOptions
                {
                    TransportType = ServiceBusTransportType.AmqpWebSockets
                }
            };

            var mensagem = new
            {
                Id = i,
                ProcessoId = $"TEMP-{i:000}",
                Timestamp = DateTimeOffset.UtcNow,
                Dados = "Operação sem cache"
            };

            try
            {
                await _serviceBusMessageSender.SendMessageToQueueAsync(
                    queueName: "queue-temporaria",
                    message: mensagem,
                    operationOptions: optionsSemCache,
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Mensagem {MessageId} enviada (novo cliente criado)", i);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar mensagem {MessageId} sem cache", i);
            }
        }

        _logger.LogInformation("Exemplo ReuseConnections concluído - Verifique os logs para comparar performance");
    }

    #endregion
}

/// <summary>
/// Enum para prioridades de mensagem
/// </summary>
public enum MessagePriority
{
    Low,
    Normal,
    High
}
