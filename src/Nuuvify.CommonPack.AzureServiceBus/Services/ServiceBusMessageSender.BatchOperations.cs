using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.AzureServiceBus.Abstraction.Models;

namespace Nuuvify.CommonPack.AzureServiceBus.Services;

/// <summary>
/// Implementação do Azure Service Bus - Métodos para Operações de Lote
/// </summary>
public partial class ServiceBusMessageSender
{
    #region Operações de Lote

    /// <summary>
    /// Cria batches de mensagens de forma assíncrona para otimizar operações em DLL
    /// </summary>
    /// <typeparam name="T">Tipo das mensagens</typeparam>
    /// <param name="messages">Lista de mensagens para processar</param>
    /// <param name="options">Opções de configuração das mensagens</param>
    /// <param name="sender">Sender do Service Bus</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de batches criados</returns>
    private async Task<List<ServiceBusMessageBatch>> CreateMessageBatchesAsync<T>(
        List<T> messages,
        ServiceBusMessageOptions options,
        ServiceBusSender sender,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var batches = new List<ServiceBusMessageBatch>();
            var currentBatch = await sender.CreateMessageBatchAsync(cancellationToken);

            _logger.LogDebug("Iniciando criação de batches para {MessageCount} mensagens", messages.Count);

            foreach (var message in messages)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var serviceBusMessage = CreateServiceBusMessage(message, options);

                if (!currentBatch.TryAddMessage(serviceBusMessage))
                {
                    if (currentBatch.Count == 0)
                    {
                        var errorMsg = "Mensagem muito grande para caber em um batch. Considere reduzir o tamanho da mensagem.";
                        _logger.LogError(errorMsg);
                        throw new InvalidOperationException(errorMsg);
                    }

                    batches.Add(currentBatch);
                    _logger.LogDebug("Batch fechado com {MessageCount} mensagens. Criando novo batch.", currentBatch.Count);

                    currentBatch = await sender.CreateMessageBatchAsync(cancellationToken);

                    if (!currentBatch.TryAddMessage(serviceBusMessage))
                    {
                        var errorMsg = "Mensagem muito grande para caber em um novo batch. Verifique o tamanho da mensagem.";
                        _logger.LogError(errorMsg);
                        throw new InvalidOperationException(errorMsg);
                    }
                }
            }

            if (currentBatch.Count > 0)
            {
                batches.Add(currentBatch);
            }

            _logger.LogDebug("Criação de batches concluída. Total de {BatchCount} batches criados", batches.Count);
            return batches;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Criação de batches foi cancelada");
            throw new OperationCanceledException("Operação de criação de batches foi cancelada", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar batches de mensagens");
            throw new InvalidOperationException("Falha ao criar batches de mensagens", ex);
        }
    }

    #endregion
}
