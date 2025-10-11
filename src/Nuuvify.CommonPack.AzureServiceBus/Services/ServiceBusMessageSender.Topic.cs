using Nuuvify.CommonPack.AzureServiceBus.Abstraction.Models;

namespace Nuuvify.CommonPack.AzureServiceBus.Services;

/// <summary>
/// Implementação do Azure Service Bus - Métodos para Topics
/// </summary>
public partial class ServiceBusMessageSender
{
    /// <summary>
    /// Envia uma mensagem única para um tópico específico do Azure Service Bus
    /// </summary>
    /// <typeparam name="T">Tipo da mensagem a ser enviada. Deve ser serializável para JSON</typeparam>
    /// <param name="topicName">Nome do tópico de destino no Azure Service Bus</param>
    /// <param name="message">Objeto da mensagem a ser enviada</param>
    /// <param name="messageOptions">Opções de configuração da mensagem (TTL, propriedades, etc). Opcional</param>
    /// <param name="operationOptions">Opções de configuração da operação (cliente customizado, connection string, etc). Opcional</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Task representando a operação assíncrona de envio</returns>
    /// <exception cref="ArgumentNullException">Quando topicName ou message são nulos</exception>
    /// <exception cref="ArgumentException">Quando topicName está vazio ou em branco</exception>
    /// <exception cref="ObjectDisposedException">Quando o ServiceBusMessageSender foi disposed</exception>
    /// <exception cref="InvalidOperationException">Quando ocorre erro ao enviar mensagem para o Service Bus</exception>
    /// <exception cref="OperationCanceledException">Quando a operação é cancelada via cancellationToken</exception>
    /// <remarks>
    /// <para>Tópicos implementam padrão publish/subscribe - uma mensagem pode ser recebida por múltiplas subscriptions.</para>
    /// <para>Diferenças entre Topic e Queue:</para>
    /// <list type="bullet">
    /// <item><description>Queue: 1 mensagem → 1 consumidor (ponto-a-ponto)</description></item>
    /// <item><description>Topic: 1 mensagem → N consumidores via subscriptions (pub/sub)</description></item>
    /// </list>
    /// <para>Ideal para notificações, eventos e cenários de broadcast.</para>
    /// <para>Cada subscription pode ter filtros independentes.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var evento = new PedidoCriado { PedidoId = 123, Cliente = "João" };
    /// await sender.SendMessageToTopicAsync("eventos-pedido", evento);
    /// // Múltiplas subscriptions podem processar este evento
    /// </code>
    /// </example>
    /// <summary>
    /// Envia uma mensagem para um tópico com suporte a opções de operação customizadas
    /// </summary>
    public async Task SendMessageToTopicAsync<T>(string topicName, T message, ServiceBusMessageOptions messageOptions = null, ServiceBusOperationOptions operationOptions = null, CancellationToken cancellationToken = default)
    {
        ValidateDisposed();
        ValidateParameter(topicName, nameof(topicName));
        ValidateParameter(message, nameof(message));

        var (client, shouldDisposeClient) = ResolveServiceBusClient(operationOptions);
        ServiceBusSender sender = null;

        try
        {
            sender = client.CreateSender(topicName);
            var serviceBusMessage = CreateServiceBusMessage(message, messageOptions);

            _logger.LogDebug("Enviando mensagem para tópico '{TopicName}' com ID '{MessageId}' usando cliente {ClientType}",
                topicName, serviceBusMessage.MessageId, shouldDisposeClient ? "temporário" : "padrão");

            await ExecuteWithRetryAsync(
                async () => await sender.SendMessageAsync(serviceBusMessage, cancellationToken),
                $"SendMessageToTopic({topicName})",
                cancellationToken);

            _logger.LogInformation("Mensagem enviada com sucesso para tópico '{TopicName}' com ID '{MessageId}'",
                topicName, serviceBusMessage.MessageId);
        }
        catch (ServiceBusException sbEx)
        {
            _logger.LogError(sbEx, "Erro do Service Bus ao enviar mensagem para tópico '{TopicName}': {Reason}",
                topicName, sbEx.Reason);
            throw new InvalidOperationException($"Erro do Service Bus ao enviar mensagem para tópico '{topicName}': {sbEx.Reason}", sbEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao enviar mensagem para tópico '{TopicName}'", topicName);
            throw new InvalidOperationException($"Falha ao enviar mensagem para tópico '{topicName}'", ex);
        }
        finally
        {
            if (sender != null)
            {
                await sender.DisposeAsync();
            }

            if (shouldDisposeClient && client != _serviceBusClient)
            {
                await client.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// Método de compatibilidade com versão anterior
    /// </summary>
    public async Task SendMessageToTopicAsync<T>(string topicName, T message, ServiceBusMessageOptions options = null, CancellationToken cancellationToken = default)
    {
        await SendMessageToTopicAsync(topicName, message, options, null, cancellationToken);
    }

    /// <summary>
    /// Envia múltiplas mensagens em lote para um tópico do Azure Service Bus de forma otimizada
    /// </summary>
    /// <typeparam name="T">Tipo das mensagens a serem enviadas. Deve ser serializável para JSON</typeparam>
    /// <param name="topicName">Nome do tópico de destino no Azure Service Bus</param>
    /// <param name="messages">Coleção de mensagens a serem enviadas em lote</param>
    /// <param name="messageOptions">Opções de configuração aplicadas a todas as mensagens. Opcional</param>
    /// <param name="operationOptions">Opções de configuração da operação (cliente customizado, connection string, etc). Opcional</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Task representando a operação assíncrona de envio em lote</returns>
    /// <exception cref="ArgumentNullException">Quando topicName ou messages são nulos</exception>
    /// <exception cref="ArgumentException">Quando topicName está vazio ou em branco</exception>
    /// <exception cref="ObjectDisposedException">Quando o ServiceBusMessageSender foi disposed</exception>
    /// <exception cref="InvalidOperationException">Quando ocorre erro ao enviar lote para o Service Bus</exception>
    /// <exception cref="OperationCanceledException">Quando a operação é cancelada via cancellationToken</exception>
    /// <remarks>
    /// <para>Envio em lote para tópicos é especialmente eficiente para cenários publish/subscribe:</para>
    /// <list type="bullet">
    /// <item><description>Eventos de domínio em lote (ex: múltiplas atualizações de status)</description></item>
    /// <item><description>Notificações massivas (ex: alertas para todos os usuários)</description></item>
    /// <item><description>Sincronização de dados entre sistemas</description></item>
    /// </list>
    /// <para>Cada mensagem do lote será entregue independentemente para todas as subscriptions ativas.</para>
    /// <para>Otimização automática de batches baseada no limite de tamanho do Service Bus.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var eventos = new List&lt;EventoPedido&gt;
    /// {
    ///     new EventoPedido { Tipo = "Criado", PedidoId = 1 },
    ///     new EventoPedido { Tipo = "Pago", PedidoId = 2 }
    /// };
    /// await sender.SendBatchMessagesToTopicAsync("eventos-pedido", eventos);
    /// </code>
    /// </example>
    /// <summary>
    /// Envia múltiplas mensagens em lote para um tópico com suporte a opções customizadas
    /// </summary>
    public async Task SendBatchMessagesToTopicAsync<T>(string topicName, IEnumerable<T> messages, ServiceBusMessageOptions messageOptions = null, ServiceBusOperationOptions operationOptions = null, CancellationToken cancellationToken = default)
    {
        await SendBatchMessagesToTopicAsync(topicName, messages, messageOptions, cancellationToken);
    }

    /// <summary>
    /// Método de compatibilidade com versão anterior
    /// </summary>
    public async Task SendBatchMessagesToTopicAsync<T>(string topicName, IEnumerable<T> messages, ServiceBusMessageOptions options = null, CancellationToken cancellationToken = default)
    {
        ValidateDisposed();
        ValidateParameter(topicName, nameof(topicName));
        ValidateParameter(messages, nameof(messages));

        var messageList = messages.ToList();
        if (messageList.Count == 0)
        {
            _logger.LogWarning("Lista de mensagens vazia para envio em lote para tópico '{TopicName}'", topicName);
            return;
        }

        ServiceBusSender sender = null;
        try
        {
            sender = _serviceBusClient.CreateSender(topicName);
            var batches = await CreateMessageBatchesAsync(messageList, options, sender, cancellationToken);

            _logger.LogDebug("Enviando {BatchCount} lotes com total de {MessageCount} mensagens para tópico '{TopicName}'",
                batches.Count, messageList.Count, topicName);

            foreach (var batch in batches)
            {
                await ExecuteWithRetryAsync(
                    async () => await sender.SendMessagesAsync(batch, cancellationToken),
                    $"SendBatchMessagesToTopic({topicName})",
                    cancellationToken);
            }

            _logger.LogInformation("Enviadas {MessageCount} mensagens em {BatchCount} lotes para tópico '{TopicName}'",
                messageList.Count, batches.Count, topicName);
        }
        catch (ServiceBusException sbEx)
        {
            _logger.LogError(sbEx, "Erro do Service Bus ao enviar lote para tópico '{TopicName}': {Reason}",
                topicName, sbEx.Reason);
            throw new InvalidOperationException($"Erro do Service Bus ao enviar lote para tópico '{topicName}': {sbEx.Reason}", sbEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao enviar lote para tópico '{TopicName}'", topicName);
            throw new InvalidOperationException($"Falha ao enviar lote para tópico '{topicName}'", ex);
        }
        finally
        {
            if (sender != null)
            {
                await sender.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// Agenda uma mensagem para um tópico com suporte a opções customizadas
    /// </summary>
    public async Task<long> ScheduleMessageToTopicAsync<T>(string topicName, T message, DateTimeOffset scheduledEnqueueTime, ServiceBusMessageOptions messageOptions = null, ServiceBusOperationOptions operationOptions = null, CancellationToken cancellationToken = default)
    {
        return await ScheduleMessageToTopicAsync(topicName, message, scheduledEnqueueTime, messageOptions, cancellationToken);
    }
    /// <summary>
    /// Agenda uma mensagem para ser entregue em um momento específico futuro no tópico
    /// </summary>
    /// <typeparam name="T">Tipo da mensagem a ser agendada. Deve ser serializável para JSON</typeparam>
    /// <param name="topicName">Nome do tópico de destino no Azure Service Bus</param>
    /// <param name="message">Objeto da mensagem a ser agendada</param>
    /// <param name="scheduledEnqueueTime">Data e hora futura para entrega da mensagem</param>
    /// <param name="options">Opções de configuração da mensagem. Opcional</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Task representando a operação assíncrona de agendamento</returns>
    /// <exception cref="ArgumentNullException">Quando topicName ou message são nulos</exception>
    /// <exception cref="ArgumentException">
    /// Quando topicName está vazio/branco ou scheduledEnqueueTime não é no futuro
    /// </exception>
    /// <exception cref="ObjectDisposedException">Quando o ServiceBusMessageSender foi disposed</exception>
    /// <exception cref="InvalidOperationException">Quando ocorre erro ao agendar mensagem no Service Bus</exception>
    /// <exception cref="OperationCanceledException">Quando a operação é cancelada via cancellationToken</exception>
    /// <remarks>
    /// <para>Agendamento em tópicos permite broadcast de eventos em momento específico:</para>
    /// <list type="bullet">
    /// <item><description>Notificações programadas para múltiplos sistemas</description></item>
    /// <item><description>Eventos de negócio com timing específico</description></item>
    /// <item><description>Sincronização agendada entre serviços</description></item>
    /// <item><description>Campanhas de marketing temporais</description></item>
    /// </list>
    /// <para>Todas as subscriptions do tópico receberão a mensagem no momento agendado.</para>
    /// <para>Ideal para eventos que precisam ser processados simultaneamente por múltiplos consumidores.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var evento = new CampanhaIniciada { CampanhaId = 123, Nome = "Black Friday" };
    /// var inicioBlackFriday = new DateTimeOffset(2024, 11, 29, 0, 0, 0, TimeSpan.Zero);
    /// await sender.ScheduleMessageToTopicAsync("eventos-campanha", evento, inicioBlackFriday);
    /// </code>
    /// </example>
    public async Task<long> ScheduleMessageToTopicAsync<T>(string topicName, T message, DateTimeOffset scheduledEnqueueTime, ServiceBusMessageOptions options = null, CancellationToken cancellationToken = default)
    {
        ValidateDisposed();
        ValidateParameter(topicName, nameof(topicName));
        ValidateParameter(message, nameof(message));

        if (scheduledEnqueueTime <= DateTimeOffset.UtcNow)
        {
            throw new ArgumentException("O horário agendado deve ser no futuro", nameof(scheduledEnqueueTime));
        }

        ServiceBusSender sender = null;
        try
        {
            sender = _serviceBusClient.CreateSender(topicName);
            var serviceBusMessage = CreateServiceBusMessage(message, options);

            _logger.LogDebug("Agendando mensagem para tópico '{TopicName}' com ID '{MessageId}' para '{ScheduledTime}'",
                topicName, serviceBusMessage.MessageId, scheduledEnqueueTime);

            var sequenceNumber = await ExecuteWithRetryAsync(
                async () => await sender.ScheduleMessageAsync(serviceBusMessage, scheduledEnqueueTime, cancellationToken),
                $"ScheduleMessageToTopic({topicName})",
                cancellationToken);

            _logger.LogInformation("Mensagem agendada com sucesso para tópico '{TopicName}' com ID '{MessageId}' e número de sequência '{SequenceNumber}'",
                topicName, serviceBusMessage.MessageId, sequenceNumber);

            return sequenceNumber;
        }
        catch (ServiceBusException sbEx)
        {
            _logger.LogError(sbEx, "Erro do Service Bus ao agendar mensagem para tópico '{TopicName}': {Reason}",
                topicName, sbEx.Reason);
            throw new InvalidOperationException($"Erro do Service Bus ao agendar mensagem para tópico '{topicName}': {sbEx.Reason}", sbEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao agendar mensagem para tópico '{TopicName}'", topicName);
            throw new InvalidOperationException($"Falha ao agendar mensagem para tópico '{topicName}'", ex);
        }
        finally
        {
            if (sender != null)
            {
                await sender.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// Cancela uma mensagem previamente agendada no tópico, evitando sua entrega futura para todas as subscriptions
    /// </summary>
    /// <param name="topicName">Nome do tópico onde a mensagem foi agendada</param>
    /// <param name="sequenceNumber">Número de sequência retornado pelo agendamento da mensagem</param>
    /// <param name="operationOptions">Opções de configuração da operação (cliente customizado, connection string, etc). Opcional</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Task representando a operação assíncrona de cancelamento</returns>
    /// <exception cref="ArgumentNullException">Quando topicName é nulo</exception>
    /// <exception cref="ArgumentException">Quando topicName está vazio ou em branco</exception>
    /// <exception cref="ObjectDisposedException">Quando o ServiceBusMessageSender foi disposed</exception>
    /// <exception cref="InvalidOperationException">Quando ocorre erro ao cancelar mensagem no Service Bus</exception>
    /// <exception cref="OperationCanceledException">Quando a operação é cancelada via cancellationToken</exception>
    /// <remarks>
    /// <para>Cancelamento de mensagens agendadas em tópicos afeta todas as subscriptions:</para>
    /// <list type="bullet">
    /// <item><description>Cancelamento de campanhas que não devem mais ser executadas</description></item>
    /// <item><description>Reversão de eventos de sistema programados</description></item>
    /// <item><description>Gestão dinâmica de workflows multi-serviço</description></item>
    /// <item><description>Cancelamento de notificações broadcast</description></item>
    /// </list>
    /// <para>Uma vez cancelada, nenhuma subscription do tópico receberá a mensagem.</para>
    /// <para>Se a mensagem já foi entregue, a operação não terá efeito.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Agendar evento para todas as subscriptions
    /// var seqNumber = await sender.ScheduleMessageToTopicAsync("eventos", evento, futureTime);
    ///
    /// // Cancelar para todas as subscriptions
    /// await sender.CancelScheduledMessageInTopicAsync("eventos", seqNumber);
    /// </code>
    /// </example>
    /// <summary>
    /// Cancela uma mensagem agendada em um tópico com suporte a opções customizadas
    /// </summary>
    public async Task CancelScheduledMessageInTopicAsync(string topicName, long sequenceNumber, ServiceBusOperationOptions operationOptions = null, CancellationToken cancellationToken = default)
    {
        await CancelScheduledMessageInTopicAsync(topicName, sequenceNumber, cancellationToken);
    }

    /// <summary>
    /// Método de compatibilidade com versão anterior
    /// </summary>
    public async Task CancelScheduledMessageInTopicAsync(string topicName, long sequenceNumber, CancellationToken cancellationToken = default)
    {
        ValidateDisposed();
        ValidateParameter(topicName, nameof(topicName));

        var (client, shouldDisposeClient) = ResolveServiceBusClient(null);
        ServiceBusSender sender = null;

        try
        {
            sender = client.CreateSender(topicName);

            _logger.LogDebug("Cancelando mensagem agendada no tópico '{TopicName}' com número de sequência '{SequenceNumber}'",
                topicName, sequenceNumber);

            await ExecuteWithRetryAsync(
                async () => await sender.CancelScheduledMessageAsync(sequenceNumber, cancellationToken),
                $"CancelScheduledMessageInTopic({topicName})",
                cancellationToken);

            _logger.LogInformation("Mensagem agendada cancelada com sucesso no tópico '{TopicName}' com número de sequência '{SequenceNumber}'",
                topicName, sequenceNumber);
        }
        catch (ServiceBusException sbEx)
        {
            _logger.LogError(sbEx, "Erro do Service Bus ao cancelar mensagem agendada no tópico '{TopicName}': {Reason}",
                topicName, sbEx.Reason);
            throw new InvalidOperationException($"Erro do Service Bus ao cancelar mensagem agendada no tópico '{topicName}': {sbEx.Reason}", sbEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao cancelar mensagem agendada no tópico '{TopicName}'", topicName);
            throw new InvalidOperationException($"Falha ao cancelar mensagem agendada no tópico '{topicName}'", ex);
        }
        finally
        {
            if (sender != null)
            {
                await sender.DisposeAsync();
            }

            if (shouldDisposeClient && client != _serviceBusClient)
            {
                await client.DisposeAsync();
            }
        }
    }

}
