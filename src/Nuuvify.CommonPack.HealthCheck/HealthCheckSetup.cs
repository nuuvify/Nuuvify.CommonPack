using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuuvify.CommonPack.HealthCheck.Helpers;

namespace Nuuvify.CommonPack.HealthCheck
{

    public static class HealthCheckCommonSetup
    {
        /// <summary>
        /// O endpoint para obter o json é /hc no final da url da sua aplicação <br/>
        /// Aqui estão sendo incluidos health cheks para Local Storage, Memory e <br/>
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
                var assemblyName = AssemblyExtension.GetApplicationNameByAssembly;

                services.AddHealthChecksUI(s =>
                {
                    s.AddHealthCheckEndpoint(assemblyName, healthCheckCustomConfiguration.UrlHealthCheck);
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

                    if (!assemblyName.Contains("CwsApi", StringComparison.InvariantCultureIgnoreCase) &&
                        !assemblyName.Contains("Cws.Api", StringComparison.InvariantCultureIgnoreCase) &&
                        !assemblyName.Contains("credential", StringComparison.InvariantCultureIgnoreCase))
                    {
                        services.AddHealthChecks()
                            .AddCheck<HttpCredentialApiHealthCheck>("http-CredentialApi");
                    }

                }
            }

        }
    }
}