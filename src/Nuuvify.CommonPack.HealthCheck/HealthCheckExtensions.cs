
using System.Net;
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
    public static IServiceCollection AddHealthCheckServiceBuilder(this WebApplicationBuilder builder, ILoggerFactory loggerFactory = null)
    {

        var enableChecksStandard = builder.Configuration.GetValue<bool>("HealthCheckCustomConfiguration:EnableChecksStandard");
        if (!enableChecksStandard) return builder.Services;


        var uriHc = builder.Configuration.GetSection("HealthCheckCustomConfiguration:UrlHealthCheck")?.Value;
        var setEvaluationTimeInSeconds = builder.Configuration.GetValue<int>("HealthCheckCustomConfiguration:EvaluationTimeInSeconds");

        var healthChecksUIBuilder = builder.Services.AddHealthChecksUI(s =>
        {
            s.AddHealthCheckEndpoint(
                name: AssemblyExtension.GetApplicationNameByAssembly,
                uri: uriHc);

            s.MaximumHistoryEntriesPerEndpoint(builder.Configuration.GetValue<int>("HealthCheckCustomConfiguration:MaximumHistoryEntriesPerEndpoint"));
            s.SetEvaluationTimeInSeconds(setEvaluationTimeInSeconds);
            s.SetMinimumSecondsBetweenFailureNotifications(builder.Configuration.GetValue<int>("HealthCheckCustomConfiguration:MinimumSecondsBetweenFailureNotifications"));
            s.SetApiMaxActiveRequests(builder.Configuration.GetValue<int>("HealthCheckCustomConfiguration:SetApiMaxActiveRequests"));
            s.UseApiEndpointHttpMessageHandler(sp =>
            {
                return new HttpClientHandler
                {
                    Proxy = WebRequest.DefaultWebProxy
                };
            });
        });


        var providerStorage = builder.Configuration.GetSection("HealthCheckCustomConfiguration:ProviderStorage")?.Value;
        var historySchema = builder.Configuration.GetSection("HealthCheckCustomConfiguration:HistorySchema")?.Value;
        var historyTable = builder.Configuration.GetSection("HealthCheckCustomConfiguration:HistoryTable")?.Value;



        historySchema ??= "hc";
        historyTable ??= "HealthCheckHistory";


        if (loggerFactory is null)
        {
            var healthCheckLogLevel = builder.Configuration.GetSection("Logging:LogLevel:HealthChecks")?.Value;
            Enum.TryParse<LogLevel>(value: healthCheckLogLevel, ignoreCase: true, out var logLevel);

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

            return builder.Services;
        }


        var cnnHealthCheck = builder.Configuration.GetConnectionString("HealthCheck")!;

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


        return builder.Services;

    }


    /// <summary>
    /// Adiciona um health check para a api de credenciais "CredentialApi"
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IServiceCollection AddHealthCheckCredentialApiBuilder(this WebApplicationBuilder builder)
    {
        var enableChecksStandard = builder.Configuration.GetValue<bool>("HealthCheckCustomConfiguration:EnableChecksStandard");
        if (!enableChecksStandard) return builder.Services;

        builder.Services.AddHealthChecks()
            .AddCheck<HttpCredentialApiHealthCheck>(
                name: "CredentialApi",
                failureStatus: HealthStatus.Degraded,
                tags: new string[] { "api-external" });

        return builder.Services;
    }


    /// <summary>
    /// Adiciona um health check para Memory e LocalStorage
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IServiceCollection AddHealthCheckMemoryAndStorageBuilder(this WebApplicationBuilder builder)
    {
        var enableChecksStandard = builder.Configuration.GetValue<bool>("HealthCheckCustomConfiguration:EnableChecksStandard");
        if (!enableChecksStandard) return builder.Services;

        builder.Services.AddHealthChecks()
            .AddCheck<MemoryHealthCheck>(
                name: "host-memory",
                tags: new[] { "memory" })
            .AddCheck<LocalStorageHealthCheck>(
                name: "host-storage",
                tags: new[] { "storage" });

        return builder.Services;
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