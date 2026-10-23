using Azure.Core;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Nuuvify.CommonPack.BackgroundService.Services;

public abstract partial class ServiceBusBackgroundService<T>
{
    /// <summary>
    /// Configura o processamento de uma fila com sessões habilitadas usando string de conexão.
    /// </summary>
    /// <param name="cnnName">ServiceBus:SuaAplicacao:ConnectionString (Nome da ConnectionString no Vault)</param>
    /// <param name="queueName">ServiceBus:SuaAplicacao:QueueName</param>
    /// <param name="serviceBusClientOptions">Opções do cliente Service Bus</param>
    /// <param name="serviceBusSessionProcessorOptions">Opções do processador de sessões</param>
    /// <remarks>
    /// Obrigatório para filas criadas com <c>requires_session = true</c>, que não aceitam o processador comum.
    /// Mensagens de uma mesma sessão são entregues em ordem e nunca processadas em paralelo entre si.
    /// O settlement segue as mesmas regras do fluxo sem sessão, e a Dead Letter Queue continua sendo
    /// tratada por <see cref="DecideDeadLetterMessageActionAsync"/>.
    /// </remarks>
    protected virtual void ConfigureServiceBusSession(
        string cnnName,
        string queueName,
        ServiceBusClientOptions serviceBusClientOptions,
        ServiceBusSessionProcessorOptions serviceBusSessionProcessorOptions)
    {
        if (string.IsNullOrEmpty(cnnName))
        {
            throw new ArgumentException("A conexão com o Service Bus não foi configurada corretamente. Verifique a configuração 'ServiceBus:SuaAplicacao:ConnectionString'");
        }
        if (string.IsNullOrEmpty(queueName))
        {
            throw new ArgumentException("A fila do Service Bus não foi configurada corretamente. Verifique a configuração 'ServiceBus:SuaAplicacao:QueueName'");
        }

        var connectionString = _configurationCustom.GetSectionValue(cnnName);
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("A conexão com o Service Bus não foi configurada corretamente. Não foi possível obter a ConnectionString do Vault. Verifique a configuração 'ServiceBus:SuaAplicacao:ConnectionString' se existe no Vault dessa aplicação.");
        }

        _serviceBusClient = new ServiceBusClient(
            connectionString: connectionString,
            options: serviceBusClientOptions);

        ConfigureSessionProcessor(queueName, serviceBusSessionProcessorOptions);
    }

    /// <summary>
    /// Configura o processamento de uma fila com sessões habilitadas usando credencial Azure.
    /// </summary>
    /// <param name="queueName">ServiceBus:SuaAplicacao:QueueName</param>
    /// <param name="fullyQualifiedNamespace">ServiceBus:SuaAplicacao:FullyQualifiedNamespace</param>
    /// <param name="credential">Credencial Azure para autenticação</param>
    /// <param name="serviceBusClientOptions">Opções do cliente Service Bus</param>
    /// <param name="serviceBusSessionProcessorOptions">Opções do processador de sessões</param>
    protected virtual void ConfigureServiceBusSession(
        string queueName,
        string fullyQualifiedNamespace,
        TokenCredential credential,
        ServiceBusClientOptions serviceBusClientOptions,
        ServiceBusSessionProcessorOptions serviceBusSessionProcessorOptions)
    {
        if (string.IsNullOrEmpty(queueName))
        {
            throw new ArgumentException("A fila do Service Bus não foi configurada corretamente. Verifique a configuração 'ServiceBus:SuaAplicacao:QueueName'");
        }
        if (string.IsNullOrEmpty(fullyQualifiedNamespace))
        {
            throw new ArgumentException("O namespace totalmente qualificado do Service Bus não foi configurado corretamente. Verifique a configuração 'ServiceBus:SuaAplicacao:FullyQualifiedNamespace'");
        }
        if (credential == null)
        {
            throw new ArgumentException("O TokenCredential do Azure não foi configurado corretamente. Verifique a configuração 'AzureCredentialBuilderExtensions'");
        }

        _serviceBusClient = new ServiceBusClient(
            fullyQualifiedNamespace: fullyQualifiedNamespace,
            credential: credential,
            options: serviceBusClientOptions);

        ConfigureSessionProcessor(queueName, serviceBusSessionProcessorOptions);
    }

    /// <summary>
    /// Processa uma mensagem recebida por uma sessão do Service Bus
    /// </summary>
    /// <param name="args">Argumentos da mensagem da sessão</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representando a operação assíncrona</returns>
    protected virtual async Task HandleSessionMessageAsync(ProcessSessionMessageEventArgs args, CancellationToken cancellationToken)
    {
        if (ActivitySourceCustom == null)
        {
            throw new ArgumentException($"{nameof(ActivitySourceCustom)} não está configurado. Certifique-se de que o ActivitySource foi inicializado corretamente.");
        }

        var settlement = MessageSettlement.From(args);
        var activitySource = ActivitySourceCustom;
        using var activity = activitySource.StartActivity(nameof(HandleSessionMessageAsync));
        try
        {
            _ = activity?.SetTag("Worker.CorrelationId", ResolveCorrelationId(args.Message));
            _ = activity?.SetTag("Worker.SessionId", args.SessionId);

            _logger.LogInformation("Iniciando {ClassName} Worker: {Data}, SessionId: {SessionId}",
                nameof(HandleSessionMessageAsync), DateTimeOffset.Now, args.SessionId);

            var result = await ExecuteReceivedMessageAsync(args.Message, activitySource, cancellationToken);

            _logger.LogInformation("Finalizando {ClassName} Worker: {Data}, SessionId: {SessionId}",
                nameof(HandleSessionMessageAsync), DateTimeOffset.Now, args.SessionId);

            if (result)
            {
                if (!IsReceiveAndDeleteMode)
                {
                    await settlement.CompleteAsync(cancellationToken);
                }

                _logger.LogDebug("Mensagem {MessageId} da sessão {SessionId} processada com sucesso", args.Message.MessageId, args.SessionId);
            }
            else
            {
                await HandleBusinessLogicFailureCoreAsync(settlement, cancellationToken);
            }
        }
        catch (ServiceBusException ex) when (
            ex.Reason == ServiceBusFailureReason.MessageLockLost ||
            ex.Reason == ServiceBusFailureReason.SessionLockLost ||
            ex.Reason == ServiceBusFailureReason.QuotaExceeded)
        {
            await HandleServiceBusSpecificExceptionCoreAsync(settlement, ex, cancellationToken);
        }
        catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.ServiceCommunicationProblem)
        {
            await HandleServiceBusCommunicationExceptionCoreAsync(settlement, ex, cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            await HandleOperationCanceledExceptionCoreAsync(settlement, ex, cancellationToken);
        }
    }

    private void ConfigureSessionProcessor(string queueName, ServiceBusSessionProcessorOptions serviceBusSessionProcessorOptions)
    {
        _receiveMode = serviceBusSessionProcessorOptions?.ReceiveMode ?? ServiceBusReceiveMode.PeekLock;

        _serviceBusSessionProcessor = _serviceBusClient.CreateSessionProcessor(
            queueName: queueName,
            options: serviceBusSessionProcessorOptions);

        _configuredQueueName = queueName;
        _configuredTopicName = string.Empty;
        _configuredSubscriptionName = string.Empty;
        _isTopicConfigured = false;

        // A sub-fila de dead letter não exige sessão, então usa o processador comum.
        ConfigureDeadLetterProcessing(new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = Math.Max(1, serviceBusSessionProcessorOptions?.MaxConcurrentSessions ?? 1),
            MaxAutoLockRenewalDuration = serviceBusSessionProcessorOptions?.MaxAutoLockRenewalDuration ?? TimeSpan.FromMinutes(5)
        });
    }

    /// <summary>
    /// Unifica o settlement de mensagens com e sem sessão, que não compartilham tipo base no SDK.
    /// </summary>
    private readonly struct MessageSettlement
    {
        private readonly Func<CancellationToken, Task> _complete;
        private readonly Func<Dictionary<string, object>, CancellationToken, Task> _abandon;
        private readonly Func<Dictionary<string, object>, CancellationToken, Task> _deadLetter;

        private MessageSettlement(
            ServiceBusReceivedMessage message,
            Func<CancellationToken, Task> complete,
            Func<Dictionary<string, object>, CancellationToken, Task> abandon,
            Func<Dictionary<string, object>, CancellationToken, Task> deadLetter)
        {
            Message = message;
            _complete = complete;
            _abandon = abandon;
            _deadLetter = deadLetter;
        }

        public ServiceBusReceivedMessage Message { get; }

        public static MessageSettlement From(ProcessMessageEventArgs args) => new(
            args.Message,
            cancellationToken => args.CompleteMessageAsync(args.Message, cancellationToken),
            (properties, cancellationToken) => args.AbandonMessageAsync(args.Message, propertiesToModify: properties, cancellationToken: cancellationToken),
            (properties, cancellationToken) => args.DeadLetterMessageAsync(args.Message, propertiesToModify: properties, cancellationToken: cancellationToken));

        public static MessageSettlement From(ProcessSessionMessageEventArgs args) => new(
            args.Message,
            cancellationToken => args.CompleteMessageAsync(args.Message, cancellationToken),
            (properties, cancellationToken) => args.AbandonMessageAsync(args.Message, propertiesToModify: properties, cancellationToken: cancellationToken),
            (properties, cancellationToken) => args.DeadLetterMessageAsync(args.Message, propertiesToModify: properties, cancellationToken: cancellationToken));

        public Task CompleteAsync(CancellationToken cancellationToken) => _complete(cancellationToken);

        public Task AbandonAsync(Dictionary<string, object> properties, CancellationToken cancellationToken) => _abandon(properties, cancellationToken);

        public Task DeadLetterAsync(Dictionary<string, object> properties, CancellationToken cancellationToken) => _deadLetter(properties, cancellationToken);
    }
}
