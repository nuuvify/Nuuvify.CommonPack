using Nuuvify.CommonPack.AzureServiceBus.Abstraction.Models;

namespace Nuuvify.CommonPack.AzureServiceBus.Services;

/// <summary>
/// Implementação do Azure Service Bus - Métodos para Queues
/// </summary>
public partial class ServiceBusMessageSender
{
    #region Métodos Auxiliares para Operações Customizadas

    /// <summary>
    /// Resolve o cliente Service Bus a ser usado para a operação
    /// </summary>
    /// <param name="operationOptions">Opções de operação que podem conter cliente customizado</param>
    /// <returns>Tupla contendo o cliente a usar e se deve ser disposed após uso</returns>
    private (ServiceBusClient client, bool shouldDispose) ResolveServiceBusClient(ServiceBusOperationOptions operationOptions)
    {
        if (operationOptions == null)
        {
            return (_serviceBusClient, false);
        }

        var customClient = operationOptions.CreateClient(_configurationManager.BaseConfiguration.ConnectionString, _configurationManager.ClientConfiguration?.CreateClientOptions());

        if (customClient != null)
        {
            return (customClient, operationOptions.UseTemporaryClient);
        }

        return (_serviceBusClient, false);
    }

    #endregion
    /// <summary>
    /// Envia uma mensagem única para uma queue específica do Azure Service Bus
    /// </summary>
    /// <typeparam name="T">Tipo da mensagem a ser enviada. Deve ser serializável para JSON</typeparam>
    /// <param name="queueName">Nome da queue de destino no Azure Service Bus</param>
    /// <param name="message">Objeto da mensagem a ser enviada</param>
    /// <param name="messageOptions">Opções de configuração da mensagem (TTL, propriedades, etc). Opcional</param>
    /// <param name="operationOptions">Opções de configuração da operação (cliente customizado, connection string, etc). Opcional</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Task representando a operação assíncrona de envio</returns>
    /// <exception cref="ArgumentNullException">Quando queueName ou message são nulos</exception>
    /// <exception cref="ArgumentException">Quando queueName está vazio ou em branco</exception>
    /// <exception cref="ObjectDisposedException">Quando o ServiceBusMessageSender foi disposed</exception>
    /// <exception cref="InvalidOperationException">Quando ocorre erro ao enviar mensagem para o Service Bus</exception>
    /// <exception cref="OperationCanceledException">Quando a operação é cancelada via cancellationToken</exception>
    /// <remarks>
    /// Este método implementa retry automático para falhas temporárias do Service Bus.
    /// A mensagem é serializada para JSON antes do envio.
    /// Logs detalhados são gerados para auditoria e troubleshooting.
    /// </remarks>
    /// <example>
    /// <code>
    /// var pedido = new Pedido { Id = 123, Cliente = "João" };
    /// await sender.SendMessageToQueueAsync("pedidos-queue", pedido);
    /// </code>
    /// </example>
    /// <summary>
    /// Envia uma mensagem para uma queue com suporte a opções de operação customizadas
    /// </summary>
    public async Task SendMessageToQueueAsync<T>(string queueName, T message, ServiceBusMessageOptions messageOptions = null, ServiceBusOperationOptions operationOptions = null, CancellationToken cancellationToken = default)
    {
        ValidateDisposed();
        ValidateParameter(queueName, nameof(queueName));
        ValidateParameter(message, nameof(message));

        var (client, shouldDisposeClient) = ResolveServiceBusClient(operationOptions);
        ServiceBusSender sender = null;

        try
        {
            sender = client.CreateSender(queueName);
            var serviceBusMessage = CreateServiceBusMessage(message, messageOptions);

            _logger.LogDebug("Enviando mensagem para queue '{QueueName}' com ID '{MessageId}' usando cliente {ClientType}",
                queueName, serviceBusMessage.MessageId, shouldDisposeClient ? "temporário" : "padrão");

            await ExecuteWithRetryAsync(
                async () => await sender.SendMessageAsync(serviceBusMessage, cancellationToken),
                $"SendMessageToQueue({queueName})",
                cancellationToken);

            _logger.LogInformation("Mensagem enviada com sucesso para queue '{QueueName}' com ID '{MessageId}'",
                queueName, serviceBusMessage.MessageId);
        }
        catch (ServiceBusException sbEx)
        {
            _logger.LogError(sbEx, "Erro do Service Bus ao enviar mensagem para queue '{QueueName}': {Reason}",
                queueName, sbEx.Reason);
            throw new InvalidOperationException($"Erro do Service Bus ao enviar mensagem para queue '{queueName}': {sbEx.Reason}", sbEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao enviar mensagem para queue '{QueueName}'", queueName);
            throw new InvalidOperationException($"Falha ao enviar mensagem para queue '{queueName}'", ex);
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
    public async Task SendMessageToQueueAsync<T>(string queueName, T message, ServiceBusMessageOptions options = null, CancellationToken cancellationToken = default)
    {
        await SendMessageToQueueAsync(queueName, message, options, null, cancellationToken);
    }

    /// <summary>
    /// Envia múltiplas mensagens em lote para uma queue do Azure Service Bus de forma otimizada
    /// </summary>
    /// <typeparam name="T">Tipo das mensagens a serem enviadas. Deve ser serializável para JSON</typeparam>
    /// <param name="queueName">Nome da queue de destino no Azure Service Bus</param>
    /// <param name="messages">Coleção de mensagens a serem enviadas em lote</param>
    /// <param name="messageOptions">Opções de configuração aplicadas a todas as mensagens. Opcional</param>
    /// <param name="operationOptions">Opções de configuração da operação (cliente customizado, connection string, etc). Opcional</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Task representando a operação assíncrona de envio em lote</returns>
    /// <exception cref="ArgumentNullException">Quando queueName ou messages são nulos</exception>
    /// <exception cref="ArgumentException">Quando queueName está vazio ou em branco</exception>
    /// <exception cref="ObjectDisposedException">Quando o ServiceBusMessageSender foi disposed</exception>
    /// <exception cref="InvalidOperationException">Quando ocorre erro ao enviar lote para o Service Bus</exception>
    /// <exception cref="OperationCanceledException">Quando a operação é cancelada via cancellationToken</exception>
    /// <remarks>
    /// <para>Este método otimiza o envio de múltiplas mensagens criando batches automaticamente.</para>
    /// <para>Benefícios do envio em lote:</para>
    /// <list type="bullet">
    /// <item><description>Melhor throughput - múltiplas mensagens em uma única operação de rede</description></item>
    /// <item><description>Redução de latência - menos round trips para o Service Bus</description></item>
    /// <item><description>Eficiência de recursos - melhor utilização da largura de banda</description></item>
    /// </list>
    /// <para>O método divide automaticamente mensagens muito grandes em múltiplos batches.</para>
    /// <para>Lista vazia não gera erro, apenas log de warning e retorna sem executar.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var pedidos = new List&lt;Pedido&gt;
    /// {
    ///     new Pedido { Id = 1, Cliente = "João" },
    ///     new Pedido { Id = 2, Cliente = "Maria" }
    /// };
    /// await sender.SendBatchMessagesToQueueAsync("pedidos-queue", pedidos);
    /// </code>
    /// </example>
    /// <summary>
    /// Envia múltiplas mensagens em lote para uma queue com suporte a opções customizadas
    /// </summary>
    public async Task SendBatchMessagesToQueueAsync<T>(string queueName, IEnumerable<T> messages, ServiceBusMessageOptions messageOptions = null, ServiceBusOperationOptions operationOptions = null, CancellationToken cancellationToken = default)
    {
        ValidateDisposed();
        ValidateParameter(queueName, nameof(queueName));
        ValidateParameter(messages, nameof(messages));

        var messageList = messages.ToList();
        if (messageList.Count == 0)
        {
            _logger.LogWarning("Lista de mensagens vazia para envio em lote para queue '{QueueName}'", queueName);
            return;
        }

        var (client, shouldDisposeClient) = ResolveServiceBusClient(operationOptions);
        ServiceBusSender sender = null;

        try
        {
            sender = client.CreateSender(queueName);
            var batches = await CreateMessageBatchesAsync(messageList, messageOptions, sender, cancellationToken);

            _logger.LogDebug("Enviando {BatchCount} lotes com total de {MessageCount} mensagens para queue '{QueueName}' usando cliente {ClientType}",
                batches.Count, messageList.Count, queueName, shouldDisposeClient ? "temporário" : "padrão");

            foreach (var batch in batches)
            {
                await ExecuteWithRetryAsync(
                    async () => await sender.SendMessagesAsync(batch, cancellationToken),
                    $"SendBatchMessagesToQueue({queueName})",
                    cancellationToken);
            }

            _logger.LogInformation("Enviadas {MessageCount} mensagens em {BatchCount} lotes para queue '{QueueName}'",
                messageList.Count, batches.Count, queueName);
        }
        catch (ServiceBusException sbEx)
        {
            _logger.LogError(sbEx, "Erro do Service Bus ao enviar lote para queue '{QueueName}': {Reason}",
                queueName, sbEx.Reason);
            throw new InvalidOperationException($"Erro do Service Bus ao enviar lote para queue '{queueName}': {sbEx.Reason}", sbEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao enviar lote para queue '{QueueName}'", queueName);
            throw new InvalidOperationException($"Falha ao enviar lote para queue '{queueName}'", ex);
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
    public async Task SendBatchMessagesToQueueAsync<T>(string queueName, IEnumerable<T> messages, ServiceBusMessageOptions options = null, CancellationToken cancellationToken = default)
    {
        await SendBatchMessagesToQueueAsync(queueName, messages, options, null, cancellationToken);
    }

    /// <summary>
    /// Agenda uma mensagem para ser entregue em um momento específico futuro na queue
    /// </summary>
    /// <typeparam name="T">Tipo da mensagem a ser agendada. Deve ser serializável para JSON</typeparam>
    /// <param name="queueName">Nome da queue de destino no Azure Service Bus</param>
    /// <param name="message">Objeto da mensagem a ser agendada</param>
    /// <param name="scheduledEnqueueTime">Data e hora futura para entrega da mensagem</param>
    /// <param name="messageOptions">Opções de configuração da mensagem. Opcional</param>
    /// <param name="operationOptions">Opções de configuração da operação (cliente customizado, connection string, etc). Opcional</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Task representando a operação assíncrona de agendamento</returns>
    /// <exception cref="ArgumentNullException">Quando queueName ou message são nulos</exception>
    /// <exception cref="ArgumentException">
    /// Quando queueName está vazio/branco ou scheduledEnqueueTime não é no futuro
    /// </exception>
    /// <exception cref="ObjectDisposedException">Quando o ServiceBusMessageSender foi disposed</exception>
    /// <exception cref="InvalidOperationException">Quando ocorre erro ao agendar mensagem no Service Bus</exception>
    /// <exception cref="OperationCanceledException">Quando a operação é cancelada via cancellationToken</exception>
    /// <remarks>
    /// <para>Funcionalidade útil para implementar:</para>
    /// <list type="bullet">
    /// <item><description>Lembretes e notificações programadas</description></item>
    /// <item><description>Processamento diferido de tarefas</description></item>
    /// <item><description>Retry com delay personalizado</description></item>
    /// <item><description>Workflows com timing específico</description></item>
    /// </list>
    /// <para>A mensagem fica armazenada no Service Bus e só é entregue no momento agendado.</para>
    /// <para>O horário deve ser especificado em UTC ou com offset adequado.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var lembrete = new Lembrete { Texto = "Reunião às 14h" };
    /// var agendamento = DateTimeOffset.Now.AddHours(2);
    /// await sender.ScheduleMessageToQueueAsync("lembretes-queue", lembrete, agendamento);
    /// </code>
    /// </example>
    /// <summary>
    /// Agenda uma mensagem para uma queue com suporte a opções customizadas
    /// </summary>
    public async Task<long> ScheduleMessageToQueueAsync<T>(string queueName, T message, DateTimeOffset scheduledEnqueueTime, ServiceBusMessageOptions messageOptions = null, ServiceBusOperationOptions operationOptions = null, CancellationToken cancellationToken = default)
    {
        ValidateDisposed();
        ValidateParameter(queueName, nameof(queueName));
        ValidateParameter(message, nameof(message));

        if (scheduledEnqueueTime <= DateTimeOffset.UtcNow)
        {
            throw new ArgumentException("O horário agendado deve ser no futuro", nameof(scheduledEnqueueTime));
        }

        var (client, shouldDisposeClient) = ResolveServiceBusClient(operationOptions);
        ServiceBusSender sender = null;

        try
        {
            sender = client.CreateSender(queueName);
            var serviceBusMessage = CreateServiceBusMessage(message, messageOptions);

            _logger.LogDebug("Agendando mensagem para queue '{QueueName}' com ID '{MessageId}' para '{ScheduledTime}' usando cliente {ClientType}",
                queueName, serviceBusMessage.MessageId, scheduledEnqueueTime, shouldDisposeClient ? "temporário" : "padrão");

            var sequenceNumber = await ExecuteWithRetryAsync(
                async () => await sender.ScheduleMessageAsync(serviceBusMessage, scheduledEnqueueTime, cancellationToken),
                $"ScheduleMessageToQueue({queueName})",
                cancellationToken);

            _logger.LogInformation("Mensagem agendada com sucesso para queue '{QueueName}' com ID '{MessageId}' e número de sequência '{SequenceNumber}'",
                queueName, serviceBusMessage.MessageId, sequenceNumber);

            return sequenceNumber;
        }
        catch (ServiceBusException sbEx)
        {
            _logger.LogError(sbEx, "Erro do Service Bus ao agendar mensagem para queue '{QueueName}': {Reason}",
                queueName, sbEx.Reason);
            throw new InvalidOperationException($"Erro do Service Bus ao agendar mensagem para queue '{queueName}': {sbEx.Reason}", sbEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao agendar mensagem para queue '{QueueName}'", queueName);
            throw new InvalidOperationException($"Falha ao agendar mensagem para queue '{queueName}'", ex);
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
    public async Task<long> ScheduleMessageToQueueAsync<T>(string queueName, T message, DateTimeOffset scheduledEnqueueTime, ServiceBusMessageOptions options = null, CancellationToken cancellationToken = default)
    {
        return await ScheduleMessageToQueueAsync(queueName, message, scheduledEnqueueTime, options, null, cancellationToken);
    }

    /// <summary>
    /// Cancela uma mensagem previamente agendada na queue, evitando sua entrega futura
    /// </summary>
    /// <param name="queueName">Nome da queue onde a mensagem foi agendada</param>
    /// <param name="sequenceNumber">Número de sequência retornado pelo agendamento da mensagem</param>
    /// <param name="operationOptions">Opções de configuração da operação (cliente customizado, connection string, etc). Opcional</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Task representando a operação assíncrona de cancelamento</returns>
    /// <exception cref="ArgumentNullException">Quando queueName é nulo</exception>
    /// <exception cref="ArgumentException">Quando queueName está vazio ou em branco</exception>
    /// <exception cref="ObjectDisposedException">Quando o ServiceBusMessageSender foi disposed</exception>
    /// <exception cref="InvalidOperationException">Quando ocorre erro ao cancelar mensagem no Service Bus</exception>
    /// <exception cref="OperationCanceledException">Quando a operação é cancelada via cancellationToken</exception>
    /// <remarks>
    /// <para>Este método permite cancelar mensagens agendadas antes da sua entrega.</para>
    /// <para>Cenários úteis:</para>
    /// <list type="bullet">
    /// <item><description>Cancelamento de lembretes que não são mais necessários</description></item>
    /// <item><description>Reversão de operações agendadas</description></item>
    /// <item><description>Gestão dinâmica de workflows temporais</description></item>
    /// </list>
    /// <para>O sequenceNumber é obtido como retorno do método ScheduleMessageToQueueAsync.</para>
    /// <para>Se a mensagem já foi entregue, a operação não terá efeito.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Agendamento retorna sequenceNumber
    /// var seqNumber = await sender.ScheduleMessageToQueueAsync("queue", msg, futureTime);
    ///
    /// // Cancelamento usando o sequenceNumber
    /// await sender.CancelScheduledMessageInQueueAsync("queue", seqNumber);
    /// </code>
    /// </example>
    /// <summary>
    /// Cancela uma mensagem agendada em uma queue com suporte a opções customizadas
    /// </summary>
    public async Task CancelScheduledMessageInQueueAsync(string queueName, long sequenceNumber, ServiceBusOperationOptions operationOptions = null, CancellationToken cancellationToken = default)
    {
        ValidateDisposed();
        ValidateParameter(queueName, nameof(queueName));

        var (client, shouldDisposeClient) = ResolveServiceBusClient(operationOptions);
        ServiceBusSender sender = null;

        try
        {
            sender = client.CreateSender(queueName);

            _logger.LogDebug("Cancelando mensagem agendada na queue '{QueueName}' com número de sequência '{SequenceNumber}' usando cliente {ClientType}",
                queueName, sequenceNumber, shouldDisposeClient ? "temporário" : "padrão");

            await ExecuteWithRetryAsync(
                async () => await sender.CancelScheduledMessageAsync(sequenceNumber, cancellationToken),
                $"CancelScheduledMessageInQueue({queueName})",
                cancellationToken);

            _logger.LogInformation("Mensagem agendada cancelada com sucesso na queue '{QueueName}' com número de sequência '{SequenceNumber}'",
                queueName, sequenceNumber);
        }
        catch (ServiceBusException sbEx)
        {
            _logger.LogError(sbEx, "Erro do Service Bus ao cancelar mensagem agendada na queue '{QueueName}': {Reason}",
                queueName, sbEx.Reason);
            throw new InvalidOperationException($"Erro do Service Bus ao cancelar mensagem agendada na queue '{queueName}': {sbEx.Reason}", sbEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao cancelar mensagem agendada na queue '{QueueName}'", queueName);
            throw new InvalidOperationException($"Falha ao cancelar mensagem agendada na queue '{queueName}'", ex);
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
    public async Task CancelScheduledMessageInQueueAsync(string queueName, long sequenceNumber, CancellationToken cancellationToken = default)
    {
        await CancelScheduledMessageInQueueAsync(queueName, sequenceNumber, null, cancellationToken);
    }
}
