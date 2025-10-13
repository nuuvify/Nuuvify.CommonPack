namespace Nuuvify.CommonPack.AzureServiceBus.Services.Receiver;

/// <summary>
/// Implementação do Azure Service Bus - Métodos de Diagnóstico e Propriedades
/// </summary>
public abstract partial class ServiceBusMessageReceiver<T>
{
    #region Diagnostic Properties Creation

    /// <summary>
    /// Cria propriedades de diagnóstico para mensagens que vão para Dead Letter Queue
    /// Utiliza o RequestConfiguration.CorrelationId como uma das propriedades
    /// </summary>
    /// <param name="message">Mensagem original</param>
    /// <param name="errorDetails">Detalhes do erro</param>
    /// <param name="exceptionType">Tipo da exceção</param>
    /// <returns>Dicionário com propriedades de diagnóstico</returns>
    /// <remarks>
    /// Propriedades criadas:
    /// <list type="bullet">
    /// <item><description><c>ErrorDetails</c>: Detalhes específicos do erro ocorrido</description></item>
    /// <item><description><c>FailureTime</c>: Timestamp ISO 8601 do momento da falha</description></item>
    /// <item><description><c>ProcessorVersion</c>: Versão do assembly do processador</description></item>
    /// <item><description><c>CorrelationId</c>: ID de correlação da requisição</description></item>
    /// <item><description><c>DeliveryAttempt</c>: Número de tentativas de entrega da mensagem</description></item>
    /// <item><description><c>MessageId</c>: ID único da mensagem do Service Bus</description></item>
    /// <item><description><c>ExceptionType</c>: Tipo da exceção ou "ProcessingFailure" se não especificado</description></item>
    /// <item><description><c>ProcessorInstance</c>: Nome da máquina onde o processador está executando</description></item>
    /// <item><description><c>ProcessedBy</c>: Nome da classe que processou a mensagem</description></item>
    /// </list>
    /// </remarks>
    protected virtual Dictionary<string, object> CreateDeadLetterProperties(
        ServiceBusReceivedMessage message,
        string errorDetails,
        string exceptionType = null)
    {
        return new Dictionary<string, object>
        {
            ["ErrorDetails"] = errorDetails,
            ["FailureTime"] = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture),
            ["ProcessorVersion"] = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? UnknownValue,
            ["CorrelationId"] = RequestConfiguration.CorrelationId,
            ["DeliveryAttempt"] = message.DeliveryCount,
            ["MessageId"] = message.MessageId,
            ["ExceptionType"] = exceptionType ?? "ProcessingFailure",
            ["ProcessorInstance"] = Environment.MachineName,
            ["ProcessedBy"] = GetType().Name
        };
    }

    /// <summary>
    /// Cria propriedades de diagnóstico para mensagens abandonadas
    /// Utiliza o RequestConfiguration.CorrelationId como uma das propriedades
    /// </summary>
    /// <param name="message">Mensagem original</param>
    /// <param name="abandonReason">Motivo do abandono</param>
    /// <returns>Dicionário com propriedades de diagnóstico</returns>
    /// <remarks>
    /// Propriedades criadas:
    /// <list type="bullet">
    /// <item><description><c>AbandonReason</c>: Motivo específico pelo qual a mensagem foi abandonada</description></item>
    /// <item><description><c>AbandonTime</c>: Timestamp ISO 8601 do momento do abandono</description></item>
    /// <item><description><c>RetryCount</c>: Número de tentativas já realizadas (DeliveryCount)</description></item>
    /// <item><description><c>CorrelationId</c>: ID de correlação da requisição</description></item>
    /// <item><description><c>MessageId</c>: ID único da mensagem do Service Bus</description></item>
    /// <item><description><c>ProcessorInstance</c>: Nome da máquina onde o processador está executando</description></item>
    /// <item><description><c>NextRetryHint</c>: Sugestão de timestamp para próxima tentativa (1 minuto após o abandono)</description></item>
    /// <item><description><c>ProcessedBy</c>: Nome da classe que processou a mensagem</description></item>
    /// </list>
    /// </remarks>
    protected virtual Dictionary<string, object> CreateAbandonProperties(
        ServiceBusReceivedMessage message,
        string abandonReason)
    {
        return new Dictionary<string, object>
        {
            ["AbandonReason"] = abandonReason,
            ["AbandonTime"] = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture),
            ["RetryCount"] = message.DeliveryCount,
            ["CorrelationId"] = RequestConfiguration.CorrelationId,
            ["MessageId"] = message.MessageId,
            ["ProcessorInstance"] = Environment.MachineName,
            ["NextRetryHint"] = DateTimeOffset.UtcNow.AddMinutes(1).ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture),
            ["ProcessedBy"] = GetType().Name
        };
    }

    #endregion
}
