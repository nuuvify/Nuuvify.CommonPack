using Nuuvify.CommonPack.AzureServiceBus.Abstraction.Configuration;
using Nuuvify.CommonPack.AzureServiceBus.Abstraction.Interfaces;
using Nuuvify.CommonPack.AzureServiceBus.Services;

namespace Nuuvify.CommonPack.AzureServiceBus.Extensions;

/// <summary>
/// Extensões para configuração do Azure Service Bus no container de DI
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adiciona serviços do Azure Service Bus ao container de DI
    /// </summary>
    /// <param name="services">Coleção de serviços do DI</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <param name="sectionName">Nome da seção de configuração (padrão: "ServiceBus-SuaAplicacao")</param>
    /// <returns>IServiceCollection para chaining</returns>
    /// <remarks>
    /// <para>Registra os seguintes serviços:</para>
    /// <list type="bullet">
    /// <item><description>ServiceBusConfiguration como singleton configurado via IOptions</description></item>
    /// <item><description>IServiceBusMessageSender como singleton thread-safe</description></item>
    /// </list>
    /// <para>A configuração deve estar presente na seção especificada (padrão "ServiceBus-SuaAplicacao").</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Em Program.cs ou Startup.cs
    /// services.AddAzureServiceBus(configuration);
    ///
    /// // Com seção customizada
    /// services.AddAzureServiceBus(configuration, "MyServiceBusConfig");
    ///
    /// // Uso em um controller ou serviço
    /// public class MyService
    /// {
    ///     private readonly IServiceBusMessageSender _sender;
    ///
    ///     public MyService(IServiceBusMessageSender sender)
    ///     {
    ///         _sender = sender;
    ///     }
    ///
    ///     public async Task SendMessageAsync()
    ///     {
    ///         await _sender.SendMessageToQueueAsync("my-queue", new { Message = "Hello" });
    ///     }
    /// }
    /// </code>
    /// </example>
    public static IServiceCollection AddAzureServiceBus(this IServiceCollection services, IConfiguration configuration, string sectionName = "ServiceBus-SuaAplicacao")
    {
        // Configuração do Service Bus via Options Pattern
        _ = services.Configure<ServiceBusConfiguration>(configuration.GetSection(sectionName));

        // Registro do sender como singleton (thread-safe)
        _ = services.AddSingleton<IServiceBusMessageSender, ServiceBusMessageSender>();

        return services;
    }

    /// <summary>
    /// Adiciona serviços do Azure Service Bus com configuração customizada
    /// </summary>
    /// <param name="services">Coleção de serviços do DI</param>
    /// <param name="configureOptions">Action para configurar ServiceBusConfiguration</param>
    /// <returns>IServiceCollection para chaining</returns>
    /// <remarks>
    /// Permite configuração programática sem depender de arquivos de configuração.
    /// Útil para cenários de teste ou configuração dinâmica.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddAzureServiceBus(config =>
    /// {
    ///     config.ConnectionString = "Endpoint=sb://...";
    ///     config.MaxRetryAttempts = 5;
    ///     config.OperationTimeoutSeconds = 60;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddAzureServiceBus(this IServiceCollection services, Action<ServiceBusConfiguration> configureOptions)
    {
        // Configuração programática
        _ = services.Configure(configureOptions);

        // Registro do sender como singleton (thread-safe)
        _ = services.AddSingleton<IServiceBusMessageSender, ServiceBusMessageSender>();

        return services;
    }

    /// <summary>
    /// Adiciona serviços do Azure Service Bus com configuração via delegate
    /// </summary>
    /// <param name="services">Coleção de serviços do DI</param>
    /// <param name="configurationFactory">Factory para criar ServiceBusConfiguration</param>
    /// <returns>IServiceCollection para chaining</returns>
    /// <remarks>
    /// Permite configuração baseada em outros serviços registrados no DI.
    /// Útil quando a configuração depende de outros serviços ou lógica complexa.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddAzureServiceBus(provider =>
    /// {
    ///     var keyVault = provider.GetRequiredService&lt;IKeyVaultService&gt;();
    ///     return new ServiceBusConfiguration
    ///     {
    ///         ConnectionString = keyVault.GetSecret("ServiceBusConnectionString")
    ///     };
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddAzureServiceBus(this IServiceCollection services, Func<IServiceProvider, ServiceBusConfiguration> configurationFactory)
    {
        // Configuração via factory com acesso ao service provider
        _ = services.Configure<ServiceBusConfiguration>(options =>
        {
            // Não podemos acessar o provider aqui, então registramos um factory diferente
        });

        // Registro customizado que usa o factory
        _ = services.AddSingleton<ServiceBusConfiguration>(configurationFactory);
        _ = services.AddSingleton<IServiceBusMessageSender, ServiceBusMessageSender>();

        return services;
    }

    #region Métodos Avançados com ServiceBusClientConfiguration

    /// <summary>
    /// Adiciona ServiceBusMessageSender com configuração avançada de cliente
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="configureBasicOptions">Action para configurar ServiceBusConfiguration</param>
    /// <param name="configureClientOptions">Action para configurar ServiceBusClientConfiguration</param>
    /// <returns>IServiceCollection para método chaining</returns>
    /// <remarks>
    /// Oferece controle total sobre configuração do ServiceBusClient,
    /// incluindo tipo de transporte, retry policies, proxy settings e cliente pré-configurado.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddAzureServiceBusAdvanced(
    ///     basicConfig => basicConfig.ConnectionString = "Endpoint=sb://...",
    ///     clientConfig =>
    ///     {
    ///         clientConfig.TransportType = ServiceBusTransportType.AmqpTcp;
    ///         clientConfig.RetryOptions = new ServiceBusRetryOptions
    ///         {
    ///             MaxRetries = 5,
    ///             Delay = TimeSpan.FromSeconds(2)
    ///         };
    ///     });
    /// </code>
    /// </example>
    public static IServiceCollection AddAzureServiceBusAdvanced(
        this IServiceCollection services,
        Action<ServiceBusConfiguration> configureBasicOptions,
        Action<ServiceBusClientConfiguration> configureClientOptions)
    {
        _ = services.Configure(configureBasicOptions);
        _ = services.Configure(configureClientOptions);

        _ = services.AddSingleton<IServiceBusMessageSender>(provider =>
        {
            var basicOptions = provider.GetRequiredService<IOptions<ServiceBusConfiguration>>();
            var clientOptions = provider.GetRequiredService<IOptions<ServiceBusClientConfiguration>>();
            var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ServiceBusMessageSender>>();

            return new ServiceBusMessageSender(basicOptions, logger, clientOptions);
        });

        return services;
    }

    /// <summary>
    /// Adiciona ServiceBusMessageSender com cliente Service Bus pré-configurado
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="serviceBusClient">Cliente Service Bus já configurado</param>
    /// <param name="configureOptions">Action opcional para configurar outras opções</param>
    /// <returns>IServiceCollection para método chaining</returns>
    /// <remarks>
    /// Útil quando você já tem um ServiceBusClient configurado e quer reutilizá-lo.
    /// O ciclo de vida do cliente deve ser gerenciado externamente.
    /// </remarks>
    /// <example>
    /// <code>
    /// var client = new ServiceBusClient("Endpoint=sb://...", new ServiceBusClientOptions
    /// {
    ///     TransportType = ServiceBusTransportType.AmqpTcp
    /// });
    ///
    /// services.AddAzureServiceBusWithClient(client, config =>
    /// {
    ///     config.OperationTimeoutSeconds = 45;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddAzureServiceBusWithClient(
        this IServiceCollection services,
        ServiceBusClient serviceBusClient,
        Action<ServiceBusConfiguration> configureOptions = null)
    {
        if (configureOptions != null)
        {
            _ = services.Configure(configureOptions);
        }
        else
        {
            // Configuração mínima padrão
            _ = services.Configure<ServiceBusConfiguration>(config =>
            {
                config.ConnectionString = "PreConfiguredClient"; // Placeholder
            });
        }

        _ = services.Configure<ServiceBusClientConfiguration>(config =>
        {
            config.PreConfiguredClient = serviceBusClient;
        });

        _ = services.AddSingleton<IServiceBusMessageSender>(provider =>
        {
            var basicOptions = provider.GetRequiredService<IOptions<ServiceBusConfiguration>>();
            var clientOptions = provider.GetRequiredService<IOptions<ServiceBusClientConfiguration>>();
            var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ServiceBusMessageSender>>();

            return new ServiceBusMessageSender(basicOptions, logger, clientOptions);
        });

        return services;
    }

    /// <summary>
    /// Adiciona ServiceBusMessageSender com factory customizada para cliente
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="connectionString">Connection string do Azure Service Bus</param>
    /// <param name="clientFactory">Factory para criação customizada do cliente</param>
    /// <param name="configureOptions">Action opcional para configurar outras opções</param>
    /// <returns>IServiceCollection para método chaining</returns>
    /// <remarks>
    /// Permite lógica totalmente customizada para criação do ServiceBusClient,
    /// útil para cenários avançados como multi-tenancy ou integração com sistemas externos.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddAzureServiceBusWithFactory(
    ///     "Endpoint=sb://...",
    ///     (connectionString, options) =>
    ///     {
    ///         // Lógica customizada para criar cliente
    ///         var customOptions = new ServiceBusClientOptions
    ///         {
    ///             TransportType = DetermineTransportType(),
    ///             RetryOptions = CreateCustomRetryOptions()
    ///         };
    ///         return new ServiceBusClient(connectionString, customOptions);
    ///     });
    /// </code>
    /// </example>
    public static IServiceCollection AddAzureServiceBusWithFactory(
        this IServiceCollection services,
        string connectionString,
        Func<string, ServiceBusClientOptions, ServiceBusClient> clientFactory,
        Action<ServiceBusConfiguration> configureOptions = null)
    {
        _ = services.Configure<ServiceBusConfiguration>(config =>
        {
            config.ConnectionString = connectionString;
            configureOptions?.Invoke(config);
        });

        _ = services.Configure<ServiceBusClientConfiguration>(config =>
        {
            config.ConnectionString = connectionString;
            config.ClientFactory = clientFactory;
        });

        _ = services.AddSingleton<IServiceBusMessageSender>(provider =>
        {
            var basicOptions = provider.GetRequiredService<IOptions<ServiceBusConfiguration>>();
            var clientOptions = provider.GetRequiredService<IOptions<ServiceBusClientConfiguration>>();
            var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ServiceBusMessageSender>>();

            return new ServiceBusMessageSender(basicOptions, logger, clientOptions);
        });

        return services;
    }

    #endregion
}
