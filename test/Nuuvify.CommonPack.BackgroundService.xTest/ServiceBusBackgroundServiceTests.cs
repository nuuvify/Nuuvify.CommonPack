using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.BackgroundService.Services;
using Nuuvify.CommonPack.Middleware.Abstraction;
using System.Diagnostics;

namespace Nuuvify.CommonPack.BackgroundService.xTest;

/// <summary>
/// Implementação de teste do ServiceBusBackgroundService para permitir testes unitários
/// </summary>
public class TestServiceBusBackgroundService : ServiceBusBackgroundService<TestServiceBusBackgroundService>
{
    private bool _executeRuleResult = true;
    private ServiceBusFailureReason? _throwServiceBusException;
    private bool _throwOperationCanceledException;
    private bool _throwGenericException;
    private bool _throwOnDispose;

    public TestServiceBusBackgroundService(
        ILogger<TestServiceBusBackgroundService> logger,
        IConfigurationCustom configurationCustom,
        RequestConfiguration requestConfiguration)
        : base(logger, configurationCustom, requestConfiguration)
    {
    }

    protected override Task<bool> ExecuteReceivedMessageAsync(ServiceBusReceivedMessage message, ActivitySource activitySource, CancellationToken cancellationToken)
    {
        if (_throwServiceBusException.HasValue)
        {
            throw new ServiceBusException("Test ServiceBus exception", _throwServiceBusException.Value);
        }

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

    // Métodos públicos para testes
    public RequestConfiguration GetRequestConfiguration() => RequestConfiguration;
    public ILogger<ServiceBusBackgroundService<TestServiceBusBackgroundService>> GetLogger() => Logger;
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

    public void SetThrowServiceBusException(ServiceBusFailureReason reason)
    {
        _throwServiceBusException = reason;
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

    public void TestConfigureServiceBus(string cnnName, string queueName, ServiceBusClientOptions serviceBusClientOptions, ServiceBusProcessorOptions serviceBusProcessorOptions)
    {
        ConfigureServiceBus(cnnName, queueName, serviceBusClientOptions, serviceBusProcessorOptions);
    }

    public void TestConfigureServiceBusWithTopicConnectionString(string connectionString, string topicName, string subscription, ServiceBusClientOptions serviceBusClientOptions, ServiceBusProcessorOptions serviceBusProcessorOptions)
    {
        // Para testar com topic e subscription usando connection string
        ConfigureServiceBus(connectionString, topicName, subscription, serviceBusClientOptions, serviceBusProcessorOptions);
    }

    public void TestConfigureServiceBusWithTopicCredentials(string topicName, string subscription, string fullyQualifiedNamespace, Azure.Core.TokenCredential credential, ServiceBusClientOptions serviceBusClientOptions, ServiceBusProcessorOptions serviceBusProcessorOptions)
    {
        ConfigureServiceBus(topicName, subscription, fullyQualifiedNamespace, credential, serviceBusClientOptions, serviceBusProcessorOptions);
    }

    public void TestConfigureServiceBusWithQueueCredentials(string queueName, string fullyQualifiedNamespace, Azure.Core.TokenCredential credential, ServiceBusClientOptions serviceBusClientOptions, ServiceBusProcessorOptions serviceBusProcessorOptions)
    {
        ConfigureServiceBus(queueName, fullyQualifiedNamespace, credential, serviceBusClientOptions, serviceBusProcessorOptions);
    }

    public Task TestHandleMessageAsync(ProcessMessageEventArgs args, CancellationToken cancellationToken)
        => HandleMessageAsync(args, cancellationToken);

    public Task TestHandleErrorAsync(ProcessErrorEventArgs args)
        => HandleErrorAsync(args);
}
