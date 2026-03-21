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
    protected virtual async Task HandleServiceBusSpecificExceptionAsync(
        ProcessMessageEventArgs args,
        ServiceBusException ex,
        CancellationToken cancellationToken)
    {
        _logger.LogError(ex, "Erro específico do Service Bus durante a execução do Worker: {Reason}", ex.Reason);

        if (_receiveMode != ServiceBusReceiveMode.ReceiveAndDelete)
        {
            var deadLetterProperties = CreateDeadLetterProperties(args.Message, $"ServiceBus specific error: {ex.Reason} - {ex.Message}", ex.GetType().Name);
            await args.DeadLetterMessageAsync(args.Message, propertiesToModify: deadLetterProperties, cancellationToken: cancellationToken);
        }
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
    protected virtual async Task HandleServiceBusCommunicationExceptionAsync(
        ProcessMessageEventArgs args,
        ServiceBusException ex,
        CancellationToken cancellationToken)
    {
        _logger.LogError(ex, "Erro de comunicação no Service Bus. Verifique as configurações de rede. Reason: {Reason}, Resource: {EntityPath}",
            ex.Reason, args.Message?.Subject ?? UnknownValue);

        if (_receiveMode != ServiceBusReceiveMode.ReceiveAndDelete && args.Message != null)
        {
            var deadLetterProperties = CreateDeadLetterProperties(args.Message, $"Service communication problem: {ex.Message}", ex.GetType().Name);
            await args.DeadLetterMessageAsync(args.Message, propertiesToModify: deadLetterProperties, cancellationToken: cancellationToken);
        }

        throw new InvalidOperationException($"Erro de comunicação no Service Bus para mensagem {args.Message?.MessageId ?? UnknownValue}: {ex.Reason}", ex);
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
    protected virtual async Task HandleOperationCanceledExceptionAsync(
        ProcessMessageEventArgs args,
        OperationCanceledException ex,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning(ex, "Operação cancelada durante a execução do Worker");

        if (_receiveMode != ServiceBusReceiveMode.ReceiveAndDelete)
        {
            if (AbandonMessageIfFailed)
            {
                var abandonProperties = CreateAbandonProperties(args.Message, "Operation was cancelled");
                await args.AbandonMessageAsync(args.Message, propertiesToModify: abandonProperties, cancellationToken: cancellationToken);
            }
            else
            {
                var deadLetterProperties = CreateDeadLetterProperties(args.Message, $"Operation cancelled: {ex.Message}", ex.GetType().Name);
                await args.DeadLetterMessageAsync(args.Message, propertiesToModify: deadLetterProperties, cancellationToken: cancellationToken);
            }
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

        if (_receiveMode != ServiceBusReceiveMode.ReceiveAndDelete)
        {
            var deadLetterProperties = CreateDeadLetterProperties(args.Message, $"Unhandled exception: {ex.Message}", ex.GetType().Name);
            await args.DeadLetterMessageAsync(args.Message, propertiesToModify: deadLetterProperties, cancellationToken: cancellationToken);
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
    protected virtual async Task HandleBusinessLogicFailureAsync(
        ProcessMessageEventArgs args,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning("{MethodName} retornou {TrueOrFalse} para a mensagem {MessageId}. Verificando comportamento de falha.",
            nameof(ExecuteReceivedMessageAsync), false, args.Message.MessageId);

        if (_receiveMode != ServiceBusReceiveMode.ReceiveAndDelete)
        {
            if (AbandonMessageIfFailed)
            {
                var abandonProperties = CreateAbandonProperties(args.Message, "Business logic returned false");
                await args.AbandonMessageAsync(message: args.Message, propertiesToModify: abandonProperties, cancellationToken: cancellationToken);
            }
            else
            {
                var deadLetterProperties = CreateDeadLetterProperties(args.Message, "Business logic returned false", "BusinessLogicFailure");
                await args.DeadLetterMessageAsync(message: args.Message, propertiesToModify: deadLetterProperties, cancellationToken: cancellationToken);
            }
        }
    }
}
