using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.BackgroundService.Services;
using Nuuvify.CommonPack.Middleware.Abstraction;
using System.Diagnostics;

namespace Nuuvify.CommonPack.BackgroundService.xTest;

/// <summary>
/// Implementação simplificada de teste do ServiceBusBackgroundService
/// </summary>
public class TestServiceBusBackgroundServiceSimplified : ServiceBusBackgroundService<TestServiceBusBackgroundServiceSimplified>
{
    private bool _executeRuleResult = true;
    private bool _throwOperationCanceledException;
    private bool _throwGenericException;
    private bool _throwOnDispose;

    public TestServiceBusBackgroundServiceSimplified(
        ILogger<TestServiceBusBackgroundServiceSimplified> logger,
        IConfigurationCustom configurationCustom,
        RequestConfiguration requestConfiguration)
        : base(logger, configurationCustom, requestConfiguration)
    {
    }

    protected override Task<bool> ExecuteReceivedMessageAsync(ServiceBusReceivedMessage message, ActivitySource activitySource, CancellationToken cancellationToken)
    {
        if (_throwOperationCanceledException)
        {
            throw new OperationCanceledException("Test operation was cancelled");
        }

        if (_throwGenericException)
        {
            throw new InvalidOperationException("Test generic exception");
        }

        return Task.FromResult(_executeRuleResult);
    }

    protected override void DisposeCustom(bool disposing)
    {
        if (_throwOnDispose)
        {
            try
            {
                throw new InvalidOperationException("Test dispose exception");
            }
            catch (InvalidOperationException ex)
            {
                Logger.LogWarning(ex, "Recurso do Service Bus já estava em processo de liberação");
            }
        }

        base.DisposeCustom(disposing);
    }

    // Propriedades públicas para teste
    public bool TestExecuteRuleResult => _executeRuleResult;
    public bool TestThrowOperationCanceledException => _throwOperationCanceledException;
    public bool TestThrowGenericException => _throwGenericException;
    public bool TestThrowOnDispose => _throwOnDispose;

    // Métodos públicos para testes
    public RequestConfiguration GetRequestConfiguration() => RequestConfiguration;
    public ILogger<ServiceBusBackgroundService<TestServiceBusBackgroundServiceSimplified>> GetLogger() => Logger;
    public IConfigurationCustom GetConfigurationCustom() => ConfigurationCustom;

    public void SetActivitySource(ActivitySource activitySource)
    {
        ActivitySourceCustom = activitySource;
    }

    public ActivitySource GetActivitySource() => ActivitySourceCustom;

    public void SetExecuteRuleResult(bool result)
    {
        _executeRuleResult = result;
    }

    public void SetThrowOperationCanceledException(bool throwException)
    {
        _throwOperationCanceledException = throwException;
    }

    public void SetThrowGenericException(bool throwException)
    {
        _throwGenericException = throwException;
    }

    public void SetThrowInvalidOperationExceptionOnDispose(bool throwException)
    {
        _throwOnDispose = throwException;
    }

    public void SetAbandonMessageIfFailed(bool abandon)
    {
        AbandonMessageIfFailed = abandon;
    }

    public bool GetAbandonMessageIfFailed() => AbandonMessageIfFailed;

    public ServiceBusReceiveMode GetReceiveMode() => ReceiveMode;
    public bool GetIsReceiveAndDeleteMode() => IsReceiveAndDeleteMode;

    public void TestConfigureServiceBus(string cnnName, string queueName, Azure.Messaging.ServiceBus.ServiceBusClientOptions serviceBusClientOptions, Azure.Messaging.ServiceBus.ServiceBusProcessorOptions serviceBusProcessorOptions)
    {
        ConfigureServiceBus(cnnName, queueName, serviceBusClientOptions, serviceBusProcessorOptions);
    }
}
