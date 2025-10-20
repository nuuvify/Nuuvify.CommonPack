using Azure.Messaging.ServiceBus;
using Nuuvify.CommonPack.BackgroundService.Services;
using Nuuvify.CommonPack.Middleware.Abstraction;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace OrderProcessingWorker;

/// <summary>
/// Exemplo de implementação concreta da classe BackgroundServiceAbstract
/// Este exemplo processa mensagens de pedidos de e-commerce
/// </summary>
public class OrderProcessingBackgroundService : ServiceBusBackgroundService<OrderProcessingBackgroundService>
{
    public OrderProcessingBackgroundService(
        ILogger<OrderProcessingBackgroundService> logger,
        IConfigurationCustom configurationCustom,
        RequestConfiguration requestConfiguration)
        : base(
            logger,
            configurationCustom,
            requestConfiguration)
    {
        AbandonMessageIfFailed = true;
        ActivitySourceCustom = new ActivitySource("BackgroundWorkerBus");

        var serviceBusClientOptions = new ServiceBusClientOptions
        {
            WebProxy = WebRequest.DefaultWebProxy,
            TransportType = ServiceBusTransportType.AmqpWebSockets
        };
        var serviceBusProcessorOptions = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 1,
            AutoCompleteMessages = false,
            MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5)
        };

        var cnnName = configurationCustom.GetSectionValue("ServiceBus:CnnName");
        var topicName = configurationCustom.GetSectionValue("ServiceBus:Topic:Name");
        var subscription = configurationCustom.GetSectionValue("ServiceBus:Topic:Subscription");

        logger.LogInformation("Configurando Service Bus - Topic: {Topic}, Subscription: {Subscription}",
            topicName, subscription);

        base.ConfigureServiceBus(
            cnnName: cnnName,
            topicName: topicName,
            subscription: subscription,
            serviceBusClientOptions: serviceBusClientOptions,
            serviceBusProcessorOptions: serviceBusProcessorOptions
        );

    }

    /// <summary>
    /// Implementa a lógica específica para processar mensagens de pedidos
    /// </summary>
    /// <param name="message">Mensagem do Service Bus contendo dados do pedido</param>
    /// <param name="activitySource">Source para telemetria</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se o processamento foi bem-sucedido</returns>
    protected override async Task<bool> ExecuteReceivedMessageAsync(
        ServiceBusReceivedMessage message,
        ActivitySource activitySource,
        CancellationToken cancellationToken)
    {
        using var activity = activitySource.StartActivity("ProcessOrder");

        try
        {
            // Deserializar a mensagem
            var messageBody = message.Body.ToString();
            var orderData = JsonSerializer.Deserialize<OrderMessage>(messageBody);

            if (orderData == null)
            {
                Logger.LogError("Mensagem de pedido inválida ou vazia. MessageId: {MessageId}", message.MessageId);
                return false;
            }

            // Adicionar tags de telemetria
            _ = activity?.SetTag("order.id", orderData.OrderId);
            _ = activity?.SetTag("order.customer_id", orderData.CustomerId);
            _ = activity?.SetTag("order.total_amount", orderData.TotalAmount);

            Logger.LogInformation(
                "Processando pedido {OrderId} do cliente {CustomerId} com valor total {TotalAmount}",
                orderData.OrderId,
                orderData.CustomerId,
                orderData.TotalAmount);

            // Simular processamento do pedido
            await ProcessOrderAsync(orderData, cancellationToken);

            Logger.LogInformation(
                "Pedido {OrderId} processado com sucesso",
                orderData.OrderId);

            return true;
        }
        catch (JsonException ex)
        {
            Logger.LogError(ex, "Erro ao deserializar mensagem do pedido. MessageId: {MessageId}", message.MessageId);
            return false;
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning("Processamento do pedido foi cancelado. MessageId: {MessageId}", message.MessageId);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            Logger.LogError(ex, "Erro de validação ao processar pedido. MessageId: {MessageId}", message.MessageId);
            _ = activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Processa os dados do pedido
    /// </summary>
    /// <param name="orderData">Dados do pedido</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    private async Task ProcessOrderAsync(OrderMessage orderData, CancellationToken cancellationToken)
    {
        // Simular validação do pedido
        await ValidateOrderAsync(orderData, cancellationToken);

        // Simular reserva de estoque
        await ReserveInventoryAsync(orderData, cancellationToken);

        // Simular processamento de pagamento
        await ProcessPaymentAsync(orderData, cancellationToken);

        // Simular criação do pedido no sistema
        await CreateOrderAsync(orderData, cancellationToken);
    }

    private async Task ValidateOrderAsync(OrderMessage orderData, CancellationToken cancellationToken)
    {
        Logger.LogDebug("Validando pedido {OrderId}", orderData.OrderId);

        // Simular validação
        await Task.Delay(100, cancellationToken);

        if (orderData.TotalAmount <= 0)
        {
            throw new InvalidOperationException($"Valor total do pedido {orderData.OrderId} é inválido: {orderData.TotalAmount}");
        }
    }

    private async Task ReserveInventoryAsync(OrderMessage orderData, CancellationToken cancellationToken)
    {
        Logger.LogDebug("Reservando estoque para pedido {OrderId}", orderData.OrderId);

        // Simular reserva de estoque
        await Task.Delay(200, cancellationToken);
    }

    private async Task ProcessPaymentAsync(OrderMessage orderData, CancellationToken cancellationToken)
    {
        Logger.LogDebug("Processando pagamento para pedido {OrderId}", orderData.OrderId);

        // Simular processamento de pagamento
        await Task.Delay(300, cancellationToken);
    }

    private async Task CreateOrderAsync(OrderMessage orderData, CancellationToken cancellationToken)
    {
        Logger.LogDebug("Criando pedido {OrderId} no sistema", orderData.OrderId);

        // Simular criação do pedido
        await Task.Delay(150, cancellationToken);
    }
}

/// <summary>
/// Modelo de dados para mensagens de pedido
/// </summary>
public class OrderMessage
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public IList<OrderItem> Items { get; set; } = new List<OrderItem>();
}

/// <summary>
/// Item do pedido
/// </summary>
public class OrderItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
