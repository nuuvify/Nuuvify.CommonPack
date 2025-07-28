using Azure.Messaging.ServiceBus;
using System.Diagnostics;

namespace Nuuvify.CommonPack.BackgroundService;

/// <summary>
/// Interface que define o contrato para serviços de background que processam mensagens do Azure Service Bus
/// </summary>
/// <typeparam name="T">Tipo específico do serviço de background</typeparam>
public interface IBackgroundServiceAbstract<T>
{
    /// <summary>
    /// Executa a lógica de negócio específica para processar uma mensagem do Service Bus
    /// </summary>
    /// <param name="message">Mensagem recebida do Service Bus</param>
    /// <param name="activitySource">Source para criação de atividades de telemetria</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>
    /// True se o processamento foi bem-sucedido (mensagem será marcada como completa),
    /// False se houve falha no processamento (mensagem será abandonada ou enviada para dead letter conforme configuração)
    /// </returns>
    Task<bool> ExecuteRule(
        ServiceBusReceivedMessage message,
        ActivitySource activitySource,
        CancellationToken cancellationToken);

    /// <summary>
    /// Define se a mensagem deve ser abandonada em caso de falha.
    /// True: Mensagem será abandonada (retorna para a fila para nova tentativa)
    /// False: Mensagem será enviada para dead letter queue (padrão)
    /// </summary>
    bool AbandonMessageIfFailed { get; set; }
}
