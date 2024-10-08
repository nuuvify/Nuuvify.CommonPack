
using System.Net;
using Azure.Core;
using Azure.Storage.Blobs;
using HealthChecks.Azure.Storage.Blobs;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.HealthCheck.Helpers;


namespace Nuuvify.CommonPack.HealthCheck;


public static class HealthCheckExtensions
{


    /// <summary>
    /// Esse metodo utiliza as configurações do appsettings.json para configurar o HealthCheck
    /// 
    /// </summary>
    /// <code>
    /// <example>
    ///   "HealthCheckCustomConfiguration": {
    ///     "EnableChecksStandard": true,
    ///     "UrlHealthCheck": "/aspnet/hc",
    ///     "UrlHealthCheckUI": "/aspnet/hc-ui",
    ///     "UrlHealthCheckApiData": "/aspnet/hc-api-data",
    ///     "EvaluationTimeInSeconds": 600,
    ///     "MinimumSecondsBetweenFailureNotifications": 60,
    ///     "MaximumHistoryEntriesPerEndpoint": 60,
    ///     "SetApiMaxActiveRequests": 3,
    ///     "ProviderStorage": "SqlServer",         // Se não informado sera usado Sqlite
    ///     "HistoryTable": "HealthCheckHistory",   // Se Não informado sera usado HealthCheckHistory
    ///     "HistorySchema": "hc"                   // Se Não informado sera usado hc
    ///  },
    /// "Logging": {
    ///   "LogLevel": {
    ///     "HealthChecks": "Information"           // Se não informado sera usado None
    /// </example>
    /// </code>
    /// <param name="builder"></param>
    /// <param name="loggerFactory">
    /// <code>
    ///   var logHc = LoggerFactory.Create(config =>
    ///       {
    ///            config.AddCustomFormatter(configureFormatter =>
    ///            {
    ///                configureFormatter.IncludeScopes = true;
    ///                configureFormatter.TimestampFormat = "yyyy-MM-dd HH:mm:ss zzz";
    ///            });
    ///            config.AddFilter(level => level >= Enum.Parse{LogLevel}(builder.Configuration.GetSection("Logging:LogLevel:HealthChecks").Value));
    ///        });
    /// </code>
    /// </param>
    public static IHealthChecksBuilder AddHealthCheckServiceBuilder(this WebApplicationBuilder builder, ILoggerFactory loggerFactory = null)
    {

        var enableChecksStandard = builder.Configuration.GetValue<bool>("HealthCheckCustomConfiguration:EnableChecksStandard");
        if (!enableChecksStandard) return builder.Services.AddHealthChecks();


        var uriHc = builder.Configuration.GetSection("HealthCheckCustomConfiguration:UrlHealthCheck")?.Value;
        var setEvaluationTimeInSeconds = builder.Configuration.GetValue<int>("HealthCheckCustomConfiguration:EvaluationTimeInSeconds");
        var webPRoxy = builder.Services.BuildServiceProvider().GetService<IWebProxy>();


        var healthChecksUIBuilder = builder.Services.AddHealthChecksUI(s =>
        {
            s.AddHealthCheckEndpoint(
                name: AssemblyExtension.GetApplicationNameByAssembly,
                uri: uriHc);

            s.MaximumHistoryEntriesPerEndpoint(builder.Configuration.GetValue<int>("HealthCheckCustomConfiguration:MaximumHistoryEntriesPerEndpoint"));
            s.SetEvaluationTimeInSeconds(setEvaluationTimeInSeconds);
            s.SetMinimumSecondsBetweenFailureNotifications(builder.Configuration.GetValue<int>("HealthCheckCustomConfiguration:MinimumSecondsBetweenFailureNotifications"));
            s.SetApiMaxActiveRequests(builder.Configuration.GetValue<int>("HealthCheckCustomConfiguration:SetApiMaxActiveRequests"));
            if (webPRoxy != null)
            {
                s.UseApiEndpointHttpMessageHandler(sp =>
                {
                    return new HttpClientHandler
                    {
                        Proxy = webPRoxy
                    };
                });
            }
        });


        var providerStorage = builder.Configuration.GetSection("HealthCheckCustomConfiguration:ProviderStorage")?.Value;
        var historySchema = builder.Configuration.GetSection("HealthCheckCustomConfiguration:HistorySchema")?.Value;
        var historyTable = builder.Configuration.GetSection("HealthCheckCustomConfiguration:HistoryTable")?.Value;



        historySchema ??= "hc";
        historyTable ??= "HealthCheckHistory";


        if (loggerFactory is null)
        {
            var logLevelSection = builder.Configuration.GetSection("Logging:LogLevel:HealthChecks")?.Value;
            Enum.TryParse<LogLevel>(logLevelSection ?? "None", true, out var logLevel);

            loggerFactory = LoggerFactory.Create(opt =>
            {
                opt.AddSimpleConsole(configureFormatter =>
                {
                    configureFormatter.IncludeScopes = true;
                    configureFormatter.TimestampFormat = "yyyy-MM-dd HH:mm:ss zzz";
                });
                opt.AddFilter(level => level >= logLevel);
            });
        }


        if (string.IsNullOrWhiteSpace(providerStorage) ||
            providerStorage.Equals("sqlite", StringComparison.InvariantCultureIgnoreCase))
        {

            var pathSqlite = Path.Combine(Path.GetTempPath(), $"{AssemblyExtension.GetApplicationNameByAssembly}_healthchecks.db");
            var sqLiteData = $"Data Source = {pathSqlite}";
            healthChecksUIBuilder.AddSqliteStorage(sqLiteData, options =>
            {
                options.EnableDetailedErrors(true);
                options.EnableSensitiveDataLogging(false);
                options.UseLoggerFactory(loggerFactory);

            }, configureSqliteOptions =>
            {
                configureSqliteOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                configureSqliteOptions.MigrationsHistoryTable(historyTable, historySchema);
            });

            return healthChecksUIBuilder.Services.AddHealthChecks();
        }


        var cnnHealthCheck = builder.Configuration.GetConnectionString("HealthCheck")!;
        if (cnnHealthCheck is null)
        {
            throw new ArgumentException("appsettings or your Vault properties ConnectionString--HealthCheck is null");
        }

        healthChecksUIBuilder.AddSqlServerStorage(cnnHealthCheck, options =>
        {
            options.EnableDetailedErrors(true);
            options.EnableSensitiveDataLogging(false);
            options.UseLoggerFactory(loggerFactory);

        }, configureSqlServerOptions =>
        {
            configureSqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            configureSqlServerOptions.MigrationsHistoryTable(historyTable, historySchema);
        });


        return healthChecksUIBuilder.Services.AddHealthChecks();

    }


    /// <summary>
    /// Adiciona um health check para a api de credenciais "CredentialApi"
    /// <p>AzureKeyVault para AzureAdOpenID--cc--ClientSecret e ApisCredentials--Password</p>
    /// <p>AzureBlobStorage para BlobDotnetDataProtection</p> 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configuration"></param>
    /// <param name="azureTokenCredential">sp => sp.GetRequiredService{TokenCredential}()</param>
    /// <param name="uriCredential">new Uri(configuration.GetSection("AppConfig:AppURLs:UrlLoginApi")?.Value)</param>
    /// <param name="urlHealthCheck">hc ou qualquer outro endpoint de healthcheck, ou null para cancelar</param>
    /// <param name="httpClientHandler">new MyHttpClientHandler(builder.Services).MyClientHandler</param>
    /// <param name="timeout">Default = 2 seconds</param>
    /// <returns></returns>
    public static IHealthChecksBuilder AddHealthCheckCredentialApiBuilder(
        this IHealthChecksBuilder builder,
        IConfiguration configuration,
        Func<IServiceProvider, TokenCredential> azureTokenCredential,
        Uri uriCredential = null,
        string urlHealthCheck = "hc",
        HttpClientHandler httpClientHandler = null,
        TimeSpan? timeout = default)
    {
        var enableChecksStandard = configuration.GetValue<bool>("HealthCheckCustomConfiguration:EnableChecksStandard");
        if (!enableChecksStandard) return builder;


        var tokenCredential = azureTokenCredential.Invoke(builder.Services.BuildServiceProvider());
        if (!timeout.HasValue)
        {
            timeout = TimeSpan.FromSeconds(2);
        }

        if (!string.IsNullOrWhiteSpace(urlHealthCheck))
        {

            uriCredential ??= new Uri(configuration.GetSection("AppConfig:AppURLs:UrlLoginApi")?.Value);

            builder.Services.AddHealthChecks()
                .AddTypeActivatedCheck<HttpCustomHealthCheck>(
                    name: "CredentialApi",
                    failureStatus: null,
                    tags: new[] { "api" },
                    timeout: timeout.Value,
                    args: new object[]
                    {
                        uriCredential,
                        urlHealthCheck,
                        HealthStatus.Unhealthy,
                        httpClientHandler ?? new HttpClientHandler()
                    });
        }


        builder.Services.AddHealthChecks()
            .AddAzureKeyVault(
                name: $"AzureKeyVault",
                keyVaultServiceUri: new Uri(configuration.GetSection("AzureKeyVault:Dns")?.Value),
                credential: tokenCredential,
                setup: options =>
                {
                    options.AddSecret("ApisCredentials--Password");
                    options.AddSecret("AzureAdOpenID--cc--ClientSecret");
                },
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "azure", "keyvault" },
                timeout: timeout.Value)
            .AddAzureBlobStorage(
                name: "BlobDotnetDataProtection",
                clientFactory: (sp) =>
                {
                    return sp.GetRequiredKeyedService<BlobServiceClient>("BlobDotnetDataProtection");
                },
                optionsFactory: (sp) =>
                {
                    return new AzureBlobStorageHealthCheckOptions()
                    {
                        ContainerName = "dotnetdataprotection",
                    };
                },
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "azure", "blob", "dotnetdataprotection" },
                timeout: timeout.Value);


        return builder;

    }


    /// <summary>
    /// Adiciona um health check para Memory e LocalStorage
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IHealthChecksBuilder AddHealthCheckMemoryAndStorageBuilder(this IHealthChecksBuilder builder, IConfiguration configuration)
    {
        var enableChecksStandard = configuration.GetValue<bool>("HealthCheckCustomConfiguration:EnableChecksStandard");
        if (!enableChecksStandard) return builder;

        builder.Services.AddHealthChecks()
            .AddCheck<MemoryHealthCheck>(
                name: "host-memory",
                tags: new[] { "memory" })
            .AddCheck<LocalStorageHealthCheck>(
                name: "host-storage",
                tags: new[] { "storage" });

        return builder;
    }




    /// <summary>
    /// Esse metodo usa os parametros do appsettings.json 
    /// <p>HealthCheckCustomConfiguration:UrlHealthCheck</p>
    /// <p>HealthCheckCustomConfiguration:UrlHealthCheckUI</p>
    /// <p>HealthCheckCustomConfiguration:UrlHealthCheckApiData</p>
    /// </summary>
    /// <param name="app"></param>
    public static void UseConfigurationHealthChecks(this IApplicationBuilder app)
    {

        var configuration = app.ApplicationServices.GetService<IConfiguration>();
        var enableChecksStandard = configuration.GetValue<bool>("HealthCheckCustomConfiguration:EnableChecksStandard");

        if (!enableChecksStandard) return;


        var uriHc = configuration.GetSection("HealthCheckCustomConfiguration:UrlHealthCheck")?.Value;
        var uriHcUi = configuration.GetSection("HealthCheckCustomConfiguration:UrlHealthCheckUI")?.Value;
        var uriHcApiData = configuration.GetSection("HealthCheckCustomConfiguration:UrlHealthCheckApiData")?.Value;

        if (string.IsNullOrWhiteSpace(uriHc) ||
            string.IsNullOrWhiteSpace(uriHcUi) ||
            string.IsNullOrWhiteSpace(uriHcApiData))
        {
            throw new ArgumentException("appsettings properties HealthCheckCustomConfiguration:UrlHealthCheck, HealthCheckCustomConfiguration:UrlHealthCheckUI, HealthCheckCustomConfiguration:UrlHealthCheckApiData is null");
        }


        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks(uriHc, new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            endpoints.MapHealthChecksUI(options =>
            {
                options.UIPath = uriHcUi;
                options.ApiPath = uriHcApiData;
                options.AddCustomStylesheet("dotnet.css");
            });

        });

    }

}
