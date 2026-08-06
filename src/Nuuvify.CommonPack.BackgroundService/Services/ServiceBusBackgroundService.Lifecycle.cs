using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Nuuvify.CommonPack.BackgroundService.Services;

public abstract partial class ServiceBusBackgroundService<T>
{
    /// <summary>
    /// Para o processamento das mensagens e libera os recursos.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Parando o processamento das mensagens do bus");

        try
        {
            await DisposeServiceBusResourcesAsync(stopProcessors: true, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao parar o processamento das mensagens do bus");
            throw new InvalidOperationException($"Erro ao parar o processamento do Service Bus: {ex.Message}", ex);
        }
        finally
        {
            await base.StopAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Libera os recursos utilizados pela classe.
    /// </summary>
    public sealed override void Dispose()
    {
        lock (_resourceDisposalSync)
        {
            if (_resourcesDisposed)
            {
                base.Dispose();
                GC.SuppressFinalize(this);
                return;
            }

            _resourcesDisposed = true;

            if (_serviceBusProcessor is not null)
            {
                try
                {
                    _serviceBusProcessor.DisposeAsync().AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch (ObjectDisposedException ex)
                {
                    _logger.LogWarning(ex, "Falha ao liberar recurso síncrono do Service Bus.");
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(ex, "Falha ao liberar recurso síncrono do Service Bus.");
                }
            }

            if (_deadLetterProcessor is not null)
            {
                try
                {
                    _deadLetterProcessor.DisposeAsync().AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch (ObjectDisposedException ex)
                {
                    _logger.LogWarning(ex, "Falha ao liberar recurso síncrono do Service Bus.");
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(ex, "Falha ao liberar recurso síncrono do Service Bus.");
                }
            }

            if (_originEntitySender is not null)
            {
                try
                {
                    _originEntitySender.DisposeAsync().AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch (ObjectDisposedException ex)
                {
                    _logger.LogWarning(ex, "Falha ao liberar recurso síncrono do Service Bus.");
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(ex, "Falha ao liberar recurso síncrono do Service Bus.");
                }
            }

            if (_serviceBusClient is not null)
            {
                try
                {
                    _serviceBusClient.DisposeAsync().AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch (ObjectDisposedException ex)
                {
                    _logger.LogWarning(ex, "Falha ao liberar recurso síncrono do Service Bus.");
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(ex, "Falha ao liberar recurso síncrono do Service Bus.");
                }
            }

            TryDisposeActivitySource(ActivitySourceCustom);

            _serviceBusProcessor = null!;
            _deadLetterProcessor = null!;
            _originEntitySender = null!;
            _serviceBusClient = null!;
            ActivitySourceCustom = null!;
        }

        base.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Libera os recursos assíncronos utilizados pela classe.
    /// </summary>
    /// <returns>
    /// Uma <see cref="ValueTask"/> que representa a conclusão da liberação assíncrona dos recursos do Service Bus.
    /// </returns>
    /// <remarks>
    /// Este método é idempotente e pode ser chamado múltiplas vezes com segurança.
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        await DisposeServiceBusResourcesAsync(stopProcessors: false, CancellationToken.None);
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task DisposeServiceBusResourcesAsync(bool stopProcessors, CancellationToken cancellationToken)
    {
        var resources = CaptureResourcesForDisposal();
        if (resources.IsEmpty)
        {
            return;
        }

        if (stopProcessors)
        {
            await TryStopProcessorAsync(resources.ServiceBusProcessor, cancellationToken);
            await TryStopProcessorAsync(resources.DeadLetterProcessor, cancellationToken);
        }

        await TryDisposeAsync(resources.ServiceBusProcessor);
        await TryDisposeAsync(resources.DeadLetterProcessor);
        await TryDisposeAsync(resources.OriginEntitySender);
        await TryDisposeAsync(resources.ServiceBusClient);

        TryDisposeActivitySource(resources.ActivitySource);
    }

    private ServiceBusResourcesSnapshot CaptureResourcesForDisposal()
    {
        lock (_resourceDisposalSync)
        {
            if (_resourcesDisposed)
            {
                return default;
            }

            _resourcesDisposed = true;

            var snapshot = new ServiceBusResourcesSnapshot(
                _serviceBusProcessor,
                _deadLetterProcessor,
                _originEntitySender,
                _serviceBusClient,
                ActivitySourceCustom);

            _serviceBusProcessor = null!;
            _deadLetterProcessor = null!;
            _originEntitySender = null!;
            _serviceBusClient = null!;
            ActivitySourceCustom = null!;

            return snapshot;
        }
    }

    private async Task TryStopProcessorAsync(ServiceBusProcessor processor, CancellationToken cancellationToken)
    {
        if (processor == null)
        {
            return;
        }

        try
        {
            await processor.StopProcessingAsync(cancellationToken);
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogWarning(ex, "Falha ao parar processador do Service Bus durante descarte.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Falha ao parar processador do Service Bus durante descarte.");
        }
        catch (ServiceBusException ex)
        {
            _logger.LogWarning(ex, "Falha ao parar processador do Service Bus durante descarte.");
        }
    }

    private async Task TryDisposeAsync(IAsyncDisposable disposable)
    {
        if (disposable == null)
        {
            return;
        }

        try
        {
            await disposable.DisposeAsync();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogWarning(ex, "Falha ao liberar recurso assíncrono do Service Bus.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Falha ao liberar recurso assíncrono do Service Bus.");
        }
        catch (ServiceBusException ex)
        {
            _logger.LogWarning(ex, "Falha ao liberar recurso assíncrono do Service Bus.");
        }
    }

    private void TryDisposeActivitySource(ActivitySource activitySource)
    {
        if (activitySource == null)
        {
            return;
        }

        try
        {
            activitySource.Dispose();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogWarning(ex, "Falha ao liberar ActivitySource do worker.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Falha ao liberar ActivitySource do worker.");
        }
    }

    private readonly record struct ServiceBusResourcesSnapshot(
        ServiceBusProcessor ServiceBusProcessor,
        ServiceBusProcessor DeadLetterProcessor,
        ServiceBusSender OriginEntitySender,
        ServiceBusClient ServiceBusClient,
        ActivitySource ActivitySource)
    {
        public bool IsEmpty =>
            ServiceBusProcessor == null &&
            DeadLetterProcessor == null &&
            OriginEntitySender == null &&
            ServiceBusClient == null &&
            ActivitySource == null;
    }
}
