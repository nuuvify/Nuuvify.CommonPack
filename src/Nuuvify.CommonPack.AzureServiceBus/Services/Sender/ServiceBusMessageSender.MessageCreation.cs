namespace Nuuvify.CommonPack.AzureServiceBus.Services.Sender;

/// <summary>
/// Implementação do Azure Service Bus - Métodos de Criação e Configuração de Mensagens
/// </summary>
public partial class ServiceBusMessageSender
{
    #region Criação e Configuração de Mensagens

    /// <summary>
    /// Cria uma mensagem do Service Bus a partir de um objeto, com serialização JSON e configuração de propriedades
    /// </summary>
    /// <typeparam name="T">Tipo do objeto a ser serializado</typeparam>
    /// <param name="message">Objeto a ser convertido em mensagem do Service Bus</param>
    /// <param name="options">Opções de configuração da mensagem (encoding, TTL, propriedades, etc.)</param>
    /// <returns>ServiceBusMessage configurada e pronta para envio</returns>
    /// <exception cref="InvalidOperationException">Quando falha na serialização JSON ou configuração</exception>
    /// <remarks>
    /// <para>Processo de criação da mensagem:</para>
    /// <list type="number">
    /// <item><description>Serializa o objeto para JSON usando System.Text.Json</description></item>
    /// <item><description>Converte JSON para bytes usando encoding especificado (padrão UTF-8)</description></item>
    /// <item><description>Cria ServiceBusMessage com o payload</description></item>
    /// <item><description>Aplica configurações das opções (TTL, correlationId, etc.)</description></item>
    /// </list>
    /// <para>Logs de erro detalhados para troubleshooting de serialização.</para>
    /// </remarks>
    private ServiceBusMessage CreateServiceBusMessage<T>(T message, ServiceBusMessageOptions options)
    {
        try
        {
            var jsonMessage = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(options?.Encoding?.GetBytes(jsonMessage) ?? Encoding.UTF8.GetBytes(jsonMessage));

            ConfigureMessageProperties(serviceBusMessage, options);

            return serviceBusMessage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar mensagem do Service Bus");
            throw new InvalidOperationException("Falha ao serializar mensagem", ex);
        }
    }

    /// <summary>
    /// Configura todas as propriedades da mensagem do Service Bus baseada nas opções fornecidas
    /// </summary>
    /// <param name="serviceBusMessage">Mensagem do Service Bus a ser configurada</param>
    /// <param name="options">Opções contendo as propriedades a serem aplicadas</param>
    /// <remarks>
    /// <para>Se options for null, aplica propriedades padrão.</para>
    /// <para>Se options for fornecida, aplica em sequência:</para>
    /// <list type="number">
    /// <item><description>Propriedades básicas (MessageId, CorrelationId, etc.)</description></item>
    /// <item><description>Propriedades avançadas (TTL, scheduling, routing)</description></item>
    /// <item><description>Propriedades de aplicação customizadas</description></item>
    /// </list>
    /// </remarks>
    private void ConfigureMessageProperties(ServiceBusMessage serviceBusMessage, ServiceBusMessageOptions options)
    {
        if (options != null)
        {
            ConfigureBasicProperties(serviceBusMessage, options);
            ConfigureAdvancedProperties(serviceBusMessage, options);
            ConfigureApplicationProperties(serviceBusMessage, options);
        }
        else
        {
            SetDefaultProperties(serviceBusMessage);
        }
    }

    /// <summary>
    /// Configura propriedades básicas de identificação e roteamento da mensagem
    /// </summary>
    /// <param name="serviceBusMessage">Mensagem do Service Bus a ser configurada</param>
    /// <param name="options">Opções contendo as propriedades básicas</param>
    /// <remarks>
    /// <para>Propriedades básicas configuradas (se fornecidas):</para>
    /// <list type="bullet">
    /// <item><description>MessageId - Identificador único da mensagem</description></item>
    /// <item><description>CorrelationId - ID para correlação entre mensagens</description></item>
    /// <item><description>PartitionKey - Chave para particionamento (garante ordem)</description></item>
    /// <item><description>SessionId - ID da sessão (para mensagens ordenadas)</description></item>
    /// <item><description>ContentType - Tipo do conteúdo (padrão: application/json)</description></item>
    /// </list>
    /// <para>Valores vazios ou nulos são ignorados, mantendo valores padrão do Service Bus.</para>
    /// </remarks>
    private static void ConfigureBasicProperties(ServiceBusMessage serviceBusMessage, ServiceBusMessageOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.MessageId))
            serviceBusMessage.MessageId = options.MessageId;

        if (!string.IsNullOrWhiteSpace(options.CorrelationId))
            serviceBusMessage.CorrelationId = options.CorrelationId;

        if (!string.IsNullOrWhiteSpace(options.PartitionKey))
            serviceBusMessage.PartitionKey = options.PartitionKey;

        if (!string.IsNullOrWhiteSpace(options.SessionId))
            serviceBusMessage.SessionId = options.SessionId;

        if (!string.IsNullOrWhiteSpace(options.ContentType))
            serviceBusMessage.ContentType = options.ContentType;
    }

    /// <summary>
    /// Configura propriedades avançadas de roteamento, tempo de vida e agendamento da mensagem
    /// </summary>
    /// <param name="serviceBusMessage">Mensagem do Service Bus a ser configurada</param>
    /// <param name="options">Opções contendo as propriedades avançadas</param>
    /// <remarks>
    /// <para>Propriedades avançadas configuradas (se fornecidas):</para>
    /// <list type="bullet">
    /// <item><description>Subject/Label - Rótulo da mensagem para filtros de subscriptions</description></item>
    /// <item><description>ReplyTo - Queue/Topic para respostas</description></item>
    /// <item><description>ReplyToSessionId - Session ID para respostas</description></item>
    /// <item><description>To - Destinatário da mensagem</description></item>
    /// <item><description>TimeToLive - Tempo de vida da mensagem no broker</description></item>
    /// <item><description>ScheduledEnqueueTimeDelay - Atraso para entrega agendada</description></item>
    /// </list>
    /// <para>Subject tem precedência sobre Label (ambos mapeiam para ServiceBusMessage.Subject).</para>
    /// </remarks>
    private static void ConfigureAdvancedProperties(ServiceBusMessage serviceBusMessage, ServiceBusMessageOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.Label))
            serviceBusMessage.Subject = options.Label;

        if (!string.IsNullOrWhiteSpace(options.Subject))
            serviceBusMessage.Subject = options.Subject;

        if (!string.IsNullOrWhiteSpace(options.ReplyTo))
            serviceBusMessage.ReplyTo = options.ReplyTo;

        if (!string.IsNullOrWhiteSpace(options.ReplyToSessionId))
            serviceBusMessage.ReplyToSessionId = options.ReplyToSessionId;

        if (!string.IsNullOrWhiteSpace(options.To))
            serviceBusMessage.To = options.To;

        if (options.TimeToLive != TimeSpan.Zero)
            serviceBusMessage.TimeToLive = options.TimeToLive;

        if (options.ScheduledEnqueueTimeDelay.HasValue)
            serviceBusMessage.ScheduledEnqueueTime = DateTimeOffset.UtcNow.Add(options.ScheduledEnqueueTimeDelay.Value);
    }

    /// <summary>
    /// Adiciona propriedades customizadas da aplicação à mensagem do Service Bus
    /// </summary>
    /// <param name="serviceBusMessage">Mensagem do Service Bus a ser configurada</param>
    /// <param name="options">Opções contendo as propriedades da aplicação</param>
    /// <remarks>
    /// <para>Propriedades da aplicação são metadados customizados que:</para>
    /// <list type="bullet">
    /// <item><description>Podem ser usados para filtros de subscriptions</description></item>
    /// <item><description>São acessíveis sem desserializar o corpo da mensagem</description></item>
    /// <item><description>Suportam tipos: string, int, double, bool, DateTime, etc.</description></item>
    /// <item><description>Têm limite de ~32KB total por mensagem</description></item>
    /// </list>
    /// <para>Útil para implementar padrões como routing slip, content-based routing, etc.</para>
    /// </remarks>
    private static void ConfigureApplicationProperties(ServiceBusMessage serviceBusMessage, ServiceBusMessageOptions options)
    {
        foreach (var prop in options.ApplicationProperties)
        {
            serviceBusMessage.ApplicationProperties.Add(prop.Key, prop.Value);
        }
    }

    /// <summary>
    /// Define propriedades padrão para mensagens quando nenhuma opção é fornecida
    /// </summary>
    /// <param name="serviceBusMessage">Mensagem do Service Bus a receber propriedades padrão</param>
    /// <remarks>
    /// <para>Propriedades padrão aplicadas:</para>
    /// <list type="bullet">
    /// <item><description>ContentType: "application/json" - indica serialização JSON</description></item>
    /// <item><description>TimeToLive: valor configurado em DefaultMessageTtlMinutes</description></item>
    /// </list>
    /// <para>Essas propriedades garantem comportamento consistente quando mensagens
    /// são enviadas sem configuração específica.</para>
    /// </remarks>
    private void SetDefaultProperties(ServiceBusMessage serviceBusMessage)
    {
        serviceBusMessage.ContentType = "application/json";
        serviceBusMessage.TimeToLive = TimeSpan.FromMinutes(_configurationManager.BaseConfiguration.DefaultMessageTtlMinutes);
    }

    #endregion
}
