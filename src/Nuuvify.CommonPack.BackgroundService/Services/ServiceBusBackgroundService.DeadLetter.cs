using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.BackgroundService.Models;

namespace Nuuvify.CommonPack.BackgroundService.Services;

public abstract partial class ServiceBusBackgroundService<T>
{
    private const string DeadLetterRequeuedPropertyName = "RequeuedFromDeadLetter";
    private const string DeadLetterOriginalMessageIdPropertyName = "OriginalDeadLetterMessageId";

    /// <summary>
    /// Configura o processador da subfila de dead letter e o sender da entidade de origem.
    /// </summary>
    /// <param name="baseProcessorOptions">Opções do processador principal.</param>
    /// <exception cref="InvalidOperationException">Lançada quando o ServiceBusClient não foi configurado.</exception>
    protected virtual void ConfigureDeadLetterProcessing(ServiceBusProcessorOptions baseProcessorOptions)
    {
        if (_serviceBusClient == null)
        {
            throw new InvalidOperationException("ServiceBusClient não configurado para inicializar processamento de dead letter.");
        }

        var deadLetterProcessorOptions = new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = Math.Max(1, baseProcessorOptions?.MaxConcurrentCalls ?? 1),
            MaxAutoLockRenewalDuration = baseProcessorOptions?.MaxAutoLockRenewalDuration ?? TimeSpan.FromMinutes(5),
            ReceiveMode = ServiceBusReceiveMode.PeekLock,
            SubQueue = SubQueue.DeadLetter
        };

        if (_isTopicConfigured)
        {
            _deadLetterProcessor = _serviceBusClient.CreateProcessor(
                _configuredTopicName,
                _configuredSubscriptionName,
                deadLetterProcessorOptions);

            _originEntitySender = _serviceBusClient.CreateSender(_configuredTopicName);
            return;
        }

        _deadLetterProcessor = _serviceBusClient.CreateProcessor(
            _configuredQueueName,
            deadLetterProcessorOptions);

        _originEntitySender = _serviceBusClient.CreateSender(_configuredQueueName);
    }

    /// <summary>
    /// Processa mensagem recebida da Dead Letter Queue.
    /// </summary>
    /// <param name="args">Argumentos da mensagem recebida.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Tarefa assíncrona.</returns>
    protected virtual async Task HandleDeadLetterMessageAsync(
        ProcessMessageEventArgs args,
        CancellationToken cancellationToken)
    {
        var action = await DecideDeadLetterMessageActionAsync(args.Message, cancellationToken);

        switch (action)
        {
            case DeadLetterMessageAction.RequeueToOrigin:
                await RequeueDeadLetterMessageAsync(args, cancellationToken);
                return;
            case DeadLetterMessageAction.Discard:
            default:
                await DiscardDeadLetterMessageAsync(args, cancellationToken);
                return;
        }
    }

    /// <summary>
    /// Cria uma nova mensagem para reenvio mantendo o body e renovando identidade.
    /// </summary>
    /// <param name="deadLetterMessage">Mensagem original da dead letter.</param>
    /// <returns>Mensagem pronta para reenvio para a origem.</returns>
    protected virtual ServiceBusMessage CreateRequeueMessage(ServiceBusReceivedMessage deadLetterMessage)
    {
        var newMessage = new ServiceBusMessage(deadLetterMessage.Body)
        {
            MessageId = Guid.NewGuid().ToString("N"),
            CorrelationId = string.IsNullOrWhiteSpace(deadLetterMessage.CorrelationId)
                ? Guid.NewGuid().ToString()
                : deadLetterMessage.CorrelationId,
            ContentType = deadLetterMessage.ContentType,
            Subject = deadLetterMessage.Subject,
            ReplyTo = deadLetterMessage.ReplyTo,
            ReplyToSessionId = deadLetterMessage.ReplyToSessionId,
            SessionId = deadLetterMessage.SessionId,
            TimeToLive = deadLetterMessage.TimeToLive,
            To = deadLetterMessage.To
        };

        if (string.IsNullOrWhiteSpace(deadLetterMessage.SessionId) && !string.IsNullOrWhiteSpace(deadLetterMessage.PartitionKey))
        {
            newMessage.PartitionKey = deadLetterMessage.PartitionKey;
        }

        foreach (var property in deadLetterMessage.ApplicationProperties)
        {
            newMessage.ApplicationProperties[property.Key] = property.Value;
        }

        newMessage.ApplicationProperties[DeadLetterRequeuedPropertyName] = true;
        newMessage.ApplicationProperties[DeadLetterOriginalMessageIdPropertyName] = deadLetterMessage.MessageId;

        return newMessage;
    }

    private async Task RequeueDeadLetterMessageAsync(
        ProcessMessageEventArgs args,
        CancellationToken cancellationToken)
    {
        try
        {
            var requeueMessage = CreateRequeueMessage(args.Message);

            await _originEntitySender.SendMessageAsync(requeueMessage, cancellationToken);
            await args.CompleteMessageAsync(args.Message, cancellationToken);

            _logger.LogInformation(
                "Mensagem {MessageId} reenfileirada para origem com novo MessageId {NewMessageId}",
                args.Message.MessageId,
                requeueMessage.MessageId);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (ServiceBusException ex)
        {
            await HandleRequeueFailureAsync(args, ex, cancellationToken);
        }
        catch (ObjectDisposedException ex)
        {
            await HandleRequeueFailureAsync(args, ex, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            await HandleRequeueFailureAsync(args, ex, cancellationToken);
        }
    }

    private async Task HandleRequeueFailureAsync(
        ProcessMessageEventArgs args,
        Exception ex,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            ex,
            "Falha ao reenfileirar mensagem {MessageId} da DLQ. A mensagem sera descartada para evitar acumulacao.",
            args.Message.MessageId);

        await DiscardDeadLetterMessageAsync(args, cancellationToken);
    }

    private async Task DiscardDeadLetterMessageAsync(
        ProcessMessageEventArgs args,
        CancellationToken cancellationToken)
    {
        await args.CompleteMessageAsync(args.Message, cancellationToken);

        _logger.LogInformation(
            "Mensagem {MessageId} descartada da Dead Letter Queue.",
            args.Message.MessageId);
    }
}
