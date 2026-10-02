using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Nuuvify.CommonPack.BackgroundService.Services;

public abstract partial class ServiceBusBackgroundService<T>
{
    /// <summary>
    /// Trata exceções específicas do Service Bus com razões conhecidas
    /// (MessageLockLost, SessionLockLost, QuotaExceeded)
    /// </summary>
    /// <param name="args">Argumentos da mensagem</param>
    /// <param name="ex">Exceção do Service Bus</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representando a operação assíncrona</returns>
    /// <remarks>
    /// Este método envia a mensagem para Dead Letter Queue com propriedades de diagnóstico
    /// contendo informações detalhadas sobre o erro específico do Service Bus.
    /// </remarks>
    protected virtual Task HandleServiceBusSpecificExceptionAsync(
        ProcessMessageEventArgs args,
        ServiceBusException ex,
        CancellationToken cancellationToken)
        => HandleServiceBusSpecificExceptionCoreAsync(MessageSettlement.From(args), ex, cancellationToken);

    private async Task HandleServiceBusSpecificExceptionCoreAsync(
        MessageSettlement settlement,
        ServiceBusException ex,
        CancellationToken cancellationToken)
    {
        _logger.LogError(ex, "Erro específico do Service Bus durante a execução do Worker: {Reason}", ex.Reason);

        if (IsReceiveAndDeleteMode)
        {
            _logger.LogWarning("Modo ReceiveAndDelete: mensagem {MessageId} já foi removida da fila. Não é possível enviar para Dead Letter.",
                settlement.Message.MessageId);
            return;
        }

        var deadLetterProperties = CreateDeadLetterProperties(settlement.Message, $"ServiceBus specific error: {ex.Reason} - {ex.Message}", ex.GetType().Name);
        await settlement.DeadLetterAsync(deadLetterProperties, cancellationToken);
    }

    /// <summary>
    /// Trata problemas de comunicação do Service Bus (ServiceCommunicationProblem)
    /// </summary>
    /// <param name="args">Argumentos da mensagem</param>
    /// <param name="ex">Exceção de comunicação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representando a operação assíncrona</returns>
    /// <remarks>
    /// Este método envia a mensagem para Dead Letter Queue com propriedades de diagnóstico
    /// e então relança a exceção encapsulada em InvalidOperationException para interromper
    /// o processamento devido à falha crítica de comunicação.
    /// </remarks>
    protected virtual Task HandleServiceBusCommunicationExceptionAsync(
        ProcessMessageEventArgs args,
        ServiceBusException ex,
        CancellationToken cancellationToken)
        => HandleServiceBusCommunicationExceptionCoreAsync(MessageSettlement.From(args), ex, cancellationToken);

    private async Task HandleServiceBusCommunicationExceptionCoreAsync(
        MessageSettlement settlement,
        ServiceBusException ex,
        CancellationToken cancellationToken)
    {
        _logger.LogError(ex, "Erro de comunicação no Service Bus. Verifique as configurações de rede. Reason: {Reason}, Resource: {EntityPath}",
            ex.Reason, settlement.Message?.Subject ?? UnknownValue);

        if (!IsReceiveAndDeleteMode && settlement.Message != null)
        {
            var deadLetterProperties = CreateDeadLetterProperties(settlement.Message, $"Service communication problem: {ex.Message}", ex.GetType().Name);
            await settlement.DeadLetterAsync(deadLetterProperties, cancellationToken);
        }
        else if (IsReceiveAndDeleteMode)
        {
            _logger.LogWarning("Modo ReceiveAndDelete: mensagem {MessageId} já foi removida da fila. Não é possível enviar para Dead Letter.",
                settlement.Message?.MessageId ?? UnknownValue);

            return;
        }

        throw new InvalidOperationException($"Erro de comunicação no Service Bus para mensagem {settlement.Message?.MessageId ?? UnknownValue}: {ex.Reason}", ex);
    }

    /// <summary>
    /// Trata operações canceladas (OperationCanceledException)
    /// </summary>
    /// <param name="args">Argumentos da mensagem</param>
    /// <param name="ex">Exceção de operação cancelada</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representando a operação assíncrona</returns>
    /// <remarks>
    /// O comportamento depende da configuração AbandonMessageIfFailed:
    /// <list type="bullet">
    /// <item><description>Se true: abandona a mensagem com propriedades de diagnóstico para reprocessamento</description></item>
    /// <item><description>Se false: envia para Dead Letter Queue com propriedades de diagnóstico</description></item>
    /// </list>
    /// </remarks>
    protected virtual Task HandleOperationCanceledExceptionAsync(
        ProcessMessageEventArgs args,
        OperationCanceledException ex,
        CancellationToken cancellationToken)
        => HandleOperationCanceledExceptionCoreAsync(MessageSettlement.From(args), ex, cancellationToken);

    private async Task HandleOperationCanceledExceptionCoreAsync(
        MessageSettlement settlement,
        OperationCanceledException ex,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning(ex, "Operação cancelada durante a execução do Worker");

        if (IsReceiveAndDeleteMode)
        {
            _logger.LogWarning("Modo ReceiveAndDelete: mensagem {MessageId} já foi removida da fila. Não é possível abandonar ou enviar para Dead Letter.",
                settlement.Message.MessageId);
            return;
        }

        if (AbandonMessageIfFailed)
        {
            var abandonProperties = CreateAbandonProperties(settlement.Message, "Operation was cancelled");
            await settlement.AbandonAsync(abandonProperties, cancellationToken);
        }
        else
        {
            var deadLetterProperties = CreateDeadLetterProperties(settlement.Message, $"Operation cancelled: {ex.Message}", ex.GetType().Name);
            await settlement.DeadLetterAsync(deadLetterProperties, cancellationToken);
        }
    }

    /// <summary>
    /// Trata exceções genéricas não capturadas (Exception)
    /// </summary>
    /// <param name="args">Argumentos da mensagem</param>
    /// <param name="ex">Exceção genérica</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representando a operação assíncrona</returns>
    /// <remarks>
    /// Este método sempre envia a mensagem para Dead Letter Queue com propriedades de diagnóstico
    /// detalhadas e então relança a exceção encapsulada em InvalidOperationException para
    /// garantir que erros não tratados sejam propagados corretamente.
    /// </remarks>
    protected virtual async Task HandleGenericExceptionAsync(
        ProcessMessageEventArgs args,
        Exception ex,
        CancellationToken cancellationToken)
    {
        _logger.LogError(ex, "Houve um erro durante a execução do Worker.ExecuteAsync");

        if (!IsReceiveAndDeleteMode && args.Message != null)
        {
            var deadLetterProperties = CreateDeadLetterProperties(args.Message, $"Unhandled exception: {ex.Message}", ex.GetType().Name);
            await args.DeadLetterMessageAsync(args.Message, propertiesToModify: deadLetterProperties, cancellationToken: cancellationToken);
        }
        else if (IsReceiveAndDeleteMode)
        {
            _logger.LogWarning("Modo ReceiveAndDelete: mensagem {MessageId} já foi removida da fila. Não é possível enviar para Dead Letter.",
                args.Message?.MessageId ?? UnknownValue);

            return;
        }

        throw new InvalidOperationException($"Erro não tratado durante processamento da mensagem {args.Message?.MessageId ?? UnknownValue}: {ex.Message}", ex);
    }

    /// <summary>
    /// Processa o resultado de falha na lógica de negócio (ExecuteReceivedMessageAsync retorna false)
    /// </summary>
    /// <param name="args">Argumentos da mensagem</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representando a operação assíncrona</returns>
    /// <remarks>
    /// O comportamento depende da configuração AbandonMessageIfFailed:
    /// <list type="bullet">
    /// <item><description>Se true: abandona a mensagem com propriedades de diagnóstico indicando falha na lógica de negócio</description></item>
    /// <item><description>Se false: envia para Dead Letter Queue com ExceptionType "BusinessLogicFailure"</description></item>
    /// </list>
    /// </remarks>
    protected virtual Task HandleBusinessLogicFailureAsync(
        ProcessMessageEventArgs args,
        CancellationToken cancellationToken)
        => HandleBusinessLogicFailureCoreAsync(MessageSettlement.From(args), cancellationToken);

    private async Task HandleBusinessLogicFailureCoreAsync(
        MessageSettlement settlement,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning("{MethodName} retornou {TrueOrFalse} para a mensagem {MessageId}. Verificando comportamento de falha.",
            nameof(ExecuteReceivedMessageAsync), false, settlement.Message.MessageId);

        if (IsReceiveAndDeleteMode)
        {
            _logger.LogWarning("Modo ReceiveAndDelete: mensagem {MessageId} já foi removida da fila. Não é possível abandonar ou enviar para Dead Letter.",
                settlement.Message.MessageId);
            return;
        }

        if (AbandonMessageIfFailed)
        {
            var abandonProperties = CreateAbandonProperties(settlement.Message, "Business logic returned false");
            await settlement.AbandonAsync(abandonProperties, cancellationToken);
        }
        else
        {
            var deadLetterProperties = CreateDeadLetterProperties(settlement.Message, "Business logic returned false", "BusinessLogicFailure");
            await settlement.DeadLetterAsync(deadLetterProperties, cancellationToken);
        }
    }
}
