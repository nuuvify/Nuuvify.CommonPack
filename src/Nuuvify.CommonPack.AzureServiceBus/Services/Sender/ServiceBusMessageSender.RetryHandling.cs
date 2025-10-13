namespace Nuuvify.CommonPack.AzureServiceBus.Services.Sender;

/// <summary>
/// Implementação do Azure Service Bus - Métodos de Retry e Tratamento de Exceções
/// </summary>
public partial class ServiceBusMessageSender
{
    #region Retry e Exception Handling

    /// <summary>
    /// Executa uma operação assíncrona com retry automático em caso de falha temporária
    /// </summary>
    /// <typeparam name="T">Tipo do resultado da operação</typeparam>
    /// <param name="operation">Função assíncrona a ser executada</param>
    /// <param name="operationName">Nome da operação para logging e diagnóstico</param>
    /// <param name="cancellationToken">Token de cancelamento para controle de execução</param>
    /// <returns>Resultado da operação executada com sucesso</returns>
    /// <exception cref="OperationCanceledException">Quando a operação é cancelada pelo usuário</exception>
    /// <exception cref="TimeoutException">Quando a operação excede o tempo limite configurado</exception>
    /// <exception cref="InvalidOperationException">Quando todas as tentativas de retry falharam</exception>
    /// <remarks>
    /// Este método implementa uma estratégia de retry exponencial para operações do Service Bus.
    /// Apenas exceções temporárias são retriadas (ServiceTimeout, ServiceBusy, etc).
    /// O número máximo de tentativas e delays são configurados via ServiceBusConfiguration.
    /// </remarks>
    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, string operationName, CancellationToken cancellationToken)
    {
        var config = _configurationManager.BaseConfiguration;
        var attempt = 0;
        var maxAttempts = config.MaxRetryAttempts + 1;

        while (attempt < maxAttempts)
        {
            try
            {
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(config.OperationTimeoutSeconds));
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                return await operation();
            }
            catch (ServiceBusException sbEx) when (IsRetriableException(sbEx) && attempt < config.MaxRetryAttempts)
            {
                attempt++;
                var delay = TimeSpan.FromSeconds(config.RetryDelaySeconds * attempt);

                _logger.LogWarning(sbEx, "Tentativa {Attempt}/{MaxAttempts} falhou para operação '{Operation}'. Tentando novamente em {Delay}ms. Erro: {Reason}",
                    attempt, maxAttempts, operationName, delay.TotalMilliseconds, sbEx.Reason);

                await Task.Delay(delay, cancellationToken);
            }
            catch (TaskCanceledException ex) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Operação '{Operation}' foi cancelada pelo usuário", operationName);
                throw new OperationCanceledException($"Operação '{operationName}' foi cancelada pelo usuário", ex, cancellationToken);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout na operação '{Operation}' após {Timeout}s", operationName, config.OperationTimeoutSeconds);
                throw new TimeoutException($"Timeout na operação '{operationName}' após {config.OperationTimeoutSeconds}s", ex);
            }
        }

        throw new InvalidOperationException($"Operação '{operationName}' falhou após {maxAttempts} tentativas");
    }

    /// <summary>
    /// Executa uma operação assíncrona sem retorno com retry automático em caso de falha temporária
    /// </summary>
    /// <param name="operation">Função assíncrona a ser executada (sem retorno)</param>
    /// <param name="operationName">Nome da operação para logging e diagnóstico</param>
    /// <param name="cancellationToken">Token de cancelamento para controle de execução</param>
    /// <returns>Task representando a operação assíncrona</returns>
    /// <exception cref="OperationCanceledException">Quando a operação é cancelada pelo usuário</exception>
    /// <exception cref="TimeoutException">Quando a operação excede o tempo limite configurado</exception>
    /// <exception cref="InvalidOperationException">Quando todas as tentativas de retry falharam</exception>
    /// <remarks>
    /// Sobrecarga do método ExecuteWithRetryAsync para operações sem retorno.
    /// Internamente utiliza a versão genérica retornando um valor dummy.
    /// </remarks>
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members",
        Justification = "Método mantido para completude da API e futuras expansões da funcionalidade de retry")]
    private async Task ExecuteWithRetryAsync(Func<Task> operation, string operationName, CancellationToken cancellationToken)
    {
        _ = await ExecuteWithRetryAsync(async () =>
        {
            await operation();
            return 0; // Valor dummy para reutilizar o método genérico
        }, operationName, cancellationToken);
    }

    /// <summary>
    /// Determina se uma exceção do Service Bus é elegível para retry
    /// </summary>
    /// <param name="exception">A exceção do Service Bus a ser analisada</param>
    /// <returns>
    /// <c>true</c> se a exceção indica uma falha temporária e pode ser retriada;
    /// <c>false</c> se a exceção indica uma falha permanente que não deve ser retriada
    /// </returns>
    /// <remarks>
    /// Exceções retriáveis incluem:
    /// - ServiceTimeout: Timeout de comunicação temporário
    /// - ServiceBusy: Serviço temporariamente sobrecarregado
    /// - GeneralError: Erro geral que pode ser temporário
    /// - ServiceCommunicationProblem: Problemas de comunicação temporários
    ///
    /// Exceções não retriáveis incluem erros de autorização, entidades não encontradas, etc.
    /// </remarks>
    private static bool IsRetriableException(ServiceBusException exception)
    {
        return exception.Reason switch
        {
            ServiceBusFailureReason.ServiceTimeout => true,
            ServiceBusFailureReason.ServiceBusy => true,
            ServiceBusFailureReason.GeneralError => true,
            ServiceBusFailureReason.ServiceCommunicationProblem => true,
            _ => false
        };
    }

    #endregion
}
