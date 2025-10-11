using Nuuvify.CommonPack.AzureServiceBus.Abstraction.Models;

namespace Nuuvify.CommonPack.AzureServiceBus.Abstraction.Interfaces;

/// <summary>
/// Interface para envio de mensagens do Azure Service Bus
/// </summary>
/// <remarks>
/// Esta interface define os contratos para operações com Azure Service Bus,
/// incluindo envio de mensagens para queues e topics, operações em lote e agendamento.
/// </remarks>
public interface IServiceBusMessageSender : IAsyncDisposable
{
    /// <summary>
    /// Envia uma mensagem para uma fila
    /// </summary>
    /// <typeparam name="T">Tipo da mensagem a ser serializada</typeparam>
    /// <param name="queueName">Nome da fila de destino</param>
    /// <param name="message">Mensagem a ser enviada</param>
    /// <param name="messageOptions">Opções de configuração da mensagem (opcional)</param>
    /// <param name="operationOptions">Opções de cliente específicas para esta operação (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento para controle da operação</param>
    /// <returns>Task representando a operação assíncrona de envio</returns>
    /// <exception cref="ArgumentNullException">Quando queueName ou message são nulos</exception>
    /// <exception cref="InvalidOperationException">Quando a conexão não está configurada</exception>
    Task SendMessageToQueueAsync<T>(string queueName, T message, ServiceBusMessageOptions messageOptions = null, ServiceBusOperationOptions operationOptions = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia uma mensagem para um tópico
    /// </summary>
    /// <typeparam name="T">Tipo da mensagem a ser serializada</typeparam>
    /// <param name="topicName">Nome do tópico de destino</param>
    /// <param name="message">Mensagem a ser enviada</param>
    /// <param name="messageOptions">Opções de configuração da mensagem (opcional)</param>
    /// <param name="operationOptions">Opções de cliente específicas para esta operação (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento para controle da operação</param>
    /// <returns>Task representando a operação assíncrona de envio</returns>
    /// <exception cref="ArgumentNullException">Quando topicName ou message são nulos</exception>
    /// <exception cref="InvalidOperationException">Quando a conexão não está configurada</exception>
    Task SendMessageToTopicAsync<T>(string topicName, T message, ServiceBusMessageOptions messageOptions = null, ServiceBusOperationOptions operationOptions = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia múltiplas mensagens para uma fila em lote
    /// </summary>
    /// <typeparam name="T">Tipo das mensagens a serem serializadas</typeparam>
    /// <param name="queueName">Nome da fila de destino</param>
    /// <param name="messages">Coleção de mensagens a serem enviadas</param>
    /// <param name="messageOptions">Opções de configuração das mensagens (opcional)</param>
    /// <param name="operationOptions">Opções de cliente específicas para esta operação (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento para controle da operação</param>
    /// <returns>Task representando a operação assíncrona de envio em lote</returns>
    /// <exception cref="ArgumentNullException">Quando queueName ou messages são nulos</exception>
    /// <exception cref="InvalidOperationException">Quando a conexão não está configurada</exception>
    Task SendBatchMessagesToQueueAsync<T>(string queueName, IEnumerable<T> messages, ServiceBusMessageOptions messageOptions = null, ServiceBusOperationOptions operationOptions = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia múltiplas mensagens para um tópico em lote
    /// </summary>
    /// <typeparam name="T">Tipo das mensagens a serem serializadas</typeparam>
    /// <param name="topicName">Nome do tópico de destino</param>
    /// <param name="messages">Coleção de mensagens a serem enviadas</param>
    /// <param name="messageOptions">Opções de configuração das mensagens (opcional)</param>
    /// <param name="operationOptions">Opções de cliente específicas para esta operação (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento para controle da operação</param>
    /// <returns>Task representando a operação assíncrona de envio em lote</returns>
    /// <exception cref="ArgumentNullException">Quando topicName ou messages são nulos</exception>
    /// <exception cref="InvalidOperationException">Quando a conexão não está configurada</exception>
    Task SendBatchMessagesToTopicAsync<T>(string topicName, IEnumerable<T> messages, ServiceBusMessageOptions messageOptions = null, ServiceBusOperationOptions operationOptions = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia uma mensagem agendada para uma fila
    /// </summary>
    /// <typeparam name="T">Tipo da mensagem a ser serializada</typeparam>
    /// <param name="queueName">Nome da fila de destino</param>
    /// <param name="message">Mensagem a ser enviada</param>
    /// <param name="scheduledEnqueueTime">Horário agendado para entrega da mensagem</param>
    /// <param name="messageOptions">Opções de configuração da mensagem (opcional)</param>
    /// <param name="operationOptions">Opções de cliente específicas para esta operação (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento para controle da operação</param>
    /// <returns>Task com o número de sequência da mensagem agendada</returns>
    /// <exception cref="ArgumentNullException">Quando queueName ou message são nulos</exception>
    /// <exception cref="InvalidOperationException">Quando a conexão não está configurada</exception>
    Task<long> ScheduleMessageToQueueAsync<T>(string queueName, T message, DateTimeOffset scheduledEnqueueTime, ServiceBusMessageOptions messageOptions = null, ServiceBusOperationOptions operationOptions = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia uma mensagem agendada para um tópico
    /// </summary>
    /// <typeparam name="T">Tipo da mensagem a ser serializada</typeparam>
    /// <param name="topicName">Nome do tópico de destino</param>
    /// <param name="message">Mensagem a ser enviada</param>
    /// <param name="scheduledEnqueueTime">Horário agendado para entrega da mensagem</param>
    /// <param name="messageOptions">Opções de configuração da mensagem (opcional)</param>
    /// <param name="operationOptions">Opções de cliente específicas para esta operação (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento para controle da operação</param>
    /// <returns>Task com o número de sequência da mensagem agendada</returns>
    /// <exception cref="ArgumentNullException">Quando topicName ou message são nulos</exception>
    /// <exception cref="InvalidOperationException">Quando a conexão não está configurada</exception>
    Task<long> ScheduleMessageToTopicAsync<T>(string topicName, T message, DateTimeOffset scheduledEnqueueTime, ServiceBusMessageOptions messageOptions = null, ServiceBusOperationOptions operationOptions = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancela uma mensagem agendada em uma fila
    /// </summary>
    /// <param name="queueName">Nome da fila onde a mensagem está agendada</param>
    /// <param name="sequenceNumber">Número de sequência da mensagem agendada (retornado pelo método Schedule)</param>
    /// <param name="operationOptions">Opções de cliente específicas para esta operação (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento para controle da operação</param>
    /// <returns>Task representando a operação assíncrona de cancelamento</returns>
    /// <exception cref="ArgumentNullException">Quando queueName é nulo</exception>
    /// <exception cref="InvalidOperationException">Quando a conexão não está configurada</exception>
    Task CancelScheduledMessageInQueueAsync(string queueName, long sequenceNumber, ServiceBusOperationOptions operationOptions = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancela uma mensagem agendada em um tópico
    /// </summary>
    /// <param name="topicName">Nome do tópico onde a mensagem está agendada</param>
    /// <param name="sequenceNumber">Número de sequência da mensagem agendada (retornado pelo método Schedule)</param>
    /// <param name="operationOptions">Opções de cliente específicas para esta operação (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento para controle da operação</param>
    /// <returns>Task representando a operação assíncrona de cancelamento</returns>
    /// <exception cref="ArgumentNullException">Quando topicName é nulo</exception>
    /// <exception cref="InvalidOperationException">Quando a conexão não está configurada</exception>
    Task CancelScheduledMessageInTopicAsync(string topicName, long sequenceNumber, ServiceBusOperationOptions operationOptions = null, CancellationToken cancellationToken = default);
}
