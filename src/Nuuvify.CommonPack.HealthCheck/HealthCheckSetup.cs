using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuuvify.CommonPack.HealthCheck.Helpers;

namespace Nuuvify.CommonPack.HealthCheck
{

    public static class HealthCheckCommonSetup
    {
        /// <summary>
        /// O endpoint para obter o json é /hc no final da url da sua aplicação
        /// Aqui estão sendo incluidos health cheks para Local Storage, Memory e
        /// CredentialApi. Configure a tag healthCheckCustomConfiguration no appsettings
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddHealthChecksCommonSetup(this IServiceCollection services, IConfiguration configuration)
        {
            var healthCheckCustomConfiguration = new HealthCheckCustomConfiguration();

            services.Configure<HealthCheckCustomConfiguration>(options =>
               configuration.GetSection(nameof(HealthCheckCustomConfiguration))
                    .Bind(options));

            configuration.GetSection(nameof(HealthCheckCustomConfiguration))
                .Bind(healthCheckCustomConfiguration);

            if (healthCheckCustomConfiguration.IsValid())
            {
                services.AddHealthChecksUI(s =>
                {

                    s.AddHealthCheckEndpoint(AssemblyExtension.GetApplicationNameByAssembly, healthCheckCustomConfiguration.UrlHealthCheck ?? "/hc");
                    if (healthCheckCustomConfiguration.MaximumHistoryEntriesPerEndpoint > 0)
                    {
                        s.MaximumHistoryEntriesPerEndpoint(healthCheckCustomConfiguration.MaximumHistoryEntriesPerEndpoint);
                    }
                    s.SetEvaluationTimeInSeconds(healthCheckCustomConfiguration.EvaluationTimeInSeconds);
                    s.SetMinimumSecondsBetweenFailureNotifications(healthCheckCustomConfiguration.MinimumSecondsBetweenFailureNotifications);
                    s.SetApiMaxActiveRequests(healthCheckCustomConfiguration.SetApiMaxActiveRequests);

                })
                .AddSqliteStorage("Data Source = healthchecks.db");

                if (healthCheckCustomConfiguration.EnableChecksStandard)
                {
                    services.AddHealthChecks()
                        .AddCheck<MemoryHealthCheck>(
                            name: "host-memory",
                            tags: new[] { "memory" })
                        .AddCheck<LocalStorageHealthCheck>(
                            name: "host-storage",
                            tags: new[] { "storage" });

                    if (!AssemblyExtension.GetApplicationNameByAssembly.Contains("CwsApi", StringComparison.InvariantCultureIgnoreCase) &&
                        !AssemblyExtension.GetApplicationNameByAssembly.Contains("Cws.Api", StringComparison.InvariantCultureIgnoreCase) &&
                        !AssemblyExtension.GetApplicationNameByAssembly.Contains("credential", StringComparison.InvariantCultureIgnoreCase))
                    {
                        services.AddHealthChecks().AddCheck<HttpCredentialApiHealthCheck>("http-CredentialApi");
                    }

                }
            }

        }
    }
}