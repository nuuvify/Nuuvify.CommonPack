using Azure.Messaging.ServiceBus;
using System.Diagnostics;

namespace Nuuvify.CommonPack.AzureServiceBus.Abstraction.Interfaces;

/// <summary>
/// Interface para recebimento de mensagens do Azure Service Bus
/// </summary>
/// <remarks>
/// Esta interface define os contratos para operações de recebimento de mensagens do Azure Service Bus,
/// incluindo configuração, processamento e controle do ciclo de vida do receptor.
/// </remarks>
public interface IServiceBusMessageReceiver : IAsyncDisposable
{
    /// <summary>
    /// Indica se o processamento está ativo
    /// </summary>
    bool IsProcessing { get; }

    /// <summary>
    /// Inicia o processamento das mensagens do Service Bus
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento para parar o processamento</param>
    /// <returns>Task representando a operação assíncrona</returns>
    /// <exception cref="InvalidOperationException">Quando ServiceBus não foi configurado ou já está processando</exception>
    Task StartProcessingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Para o processamento das mensagens e libera os recursos
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representando a operação assíncrona</returns>
    Task StopProcessingAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface genérica para recebimento de mensagens do Azure Service Bus
/// </summary>
/// <typeparam name="T">Tipo que representa o contexto da mensagem processada</typeparam>
public interface IServiceBusMessageReceiver<T> : IServiceBusMessageReceiver
{
    /// <summary>
    /// Método que deve ser implementado pelas classes derivadas para executar a lógica de negócio
    /// </summary>
    /// <param name="message">Mensagem recebida do Service Bus</param>
    /// <param name="activitySource">Source para criação de atividades de telemetria</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se o processamento foi bem-sucedido, false caso contrário</returns>
    Task<bool> ExecuteReceivedMessageAsync(
        ServiceBusReceivedMessage message,
        ActivitySource activitySource,
        CancellationToken cancellationToken);
}
