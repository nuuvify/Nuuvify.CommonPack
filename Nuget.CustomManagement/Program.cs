using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

try
{
    var builder = Host.CreateApplicationBuilder(args);
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        _ = builder.Services.AddWindowsService(options =>
        {
            options.ServiceName = ".NET Windows Service";
        });
    }
    else
    {
        _ = builder.Services.AddSystemd();
    }

    var serviceProvider = builder.Services.BuildServiceProvider();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("**** Environment: {EnvironmentName} ****", builder.Environment.EnvironmentName);

    var nugetCustomManagementPackage = new NugetCustomManagementPackage();
    await nugetCustomManagementPackage.DeletePackage(logger, builder.Environment.EnvironmentName, default);

    var host = builder.Build();

    // await host.StartAsync();
    // await host.WaitForShutdownAsync();

}
catch (TaskCanceledException ex)
{
    var message = $"Cancelamento de Task foi detectado, saindo do programa = {ex.Message}";

    Console.WriteLine(message);
}
catch (Exception ex)
{
    var message = $"Erro na inicialização ou finalização do programa = {ex.Message} StackTrace = {ex.StackTrace}";

    Console.WriteLine(message);
    if (!(ex.InnerException is null))
    {
        Console.WriteLine("#### InnerException: {0}", ex.InnerException?.Message);
        Console.WriteLine("#### InnerException->StackTrace: {0}", ex.InnerException?.StackTrace);
    }
}

/// <summary>
/// Documentação; https://github.com/NuGet/Samples/blob/main/NuGetProtocolSamples/Program.cs
/// <p>https://learn.microsoft.com/en-us/nuget/reference/nuget-client-sdk</p>
/// </summary>
public class NugetCustomManagementPackage
{
    private const string PackageVersion = "2.1.0-test.25092903";

    public IDictionary<string, string> Packages { get; set; }

    public NugetCustomManagementPackage()
    {

        Packages = new Dictionary<string, string>
        {

            { "Nuuvify.CommonPack.AutoHistory", PackageVersion },
            { "Nuuvify.CommonPack.AzureStorage", PackageVersion },
            { "Nuuvify.CommonPack.AzureStorage.Abstraction", PackageVersion },
            { "Nuuvify.CommonPack.Domain", PackageVersion },
            { "Nuuvify.CommonPack.EF.Exceptions.Common", PackageVersion },
            { "Nuuvify.CommonPack.EF.Exceptions.Db2", PackageVersion },
            { "Nuuvify.CommonPack.EF.Exceptions.Oracle", PackageVersion },
            { "Nuuvify.CommonPack.Email", PackageVersion },
            { "Nuuvify.CommonPack.Email.Abstraction", PackageVersion },
            { "Nuuvify.CommonPack.Extensions", PackageVersion },
            { "Nuuvify.CommonPack.HealthCheck", PackageVersion },
            { "Nuuvify.CommonPack.Middleware", PackageVersion },
            { "Nuuvify.CommonPack.Middleware.Abstraction", PackageVersion },
            { "Nuuvify.CommonPack.OpenApi", PackageVersion },
            { "Nuuvify.CommonPack.Security", PackageVersion },
            { "Nuuvify.CommonPack.Security.Abstraction", PackageVersion },
            { "Nuuvify.CommonPack.Security.JwtCredentials", PackageVersion },
            { "Nuuvify.CommonPack.Security.JwtStore.Ef", PackageVersion },
            { "Nuuvify.CommonPack.UnitOfWork", PackageVersion },
            { "Nuuvify.CommonPack.UnitOfWork.Abstraction", PackageVersion },
            { "Nuuvify.CommonPack.StandardHttpClient", PackageVersion }
        };

    }

    public async Task DeletePackage(Microsoft.Extensions.Logging.ILogger logger, string environmentName, CancellationToken cancellationToken)
    {

        NuGet.Configuration.PackageSource nugetConfig;
        if (environmentName.Equals("Production", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Operation not allowed in PRODUCTION environment");
        }
        else if (environmentName.Equals("Development", StringComparison.OrdinalIgnoreCase))
        {
            nugetConfig = new NuGet.Configuration.PackageSource("https://apiint.nugettest.org/v3/index.json");
            logger.LogWarning("Operation not allowed in DEVELOPMENT environment");
        }
        else
        {
            nugetConfig = new NuGet.Configuration.PackageSource("https://api.nuget.org/v3/index.json");
            logger.LogWarning("Operation not allowed in QUALITY environment");
        }

        using (var cacheNuget = new SourceCacheContext())
        {
            SourceRepository repository = Repository.Factory.GetCoreV3(nugetConfig);
            PackageUpdateResource resource = await repository.GetResourceAsync<PackageUpdateResource>();

            string apiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY");

            logger.LogInformation("===========> Start delete packages");

            foreach (var item in Packages)
            {

                var result = resource.Delete(
                    item.Key,
                    item.Value,
                    getApiKey: packageSource => apiKey,
                    confirm: packageSource => true,
                    noServiceEndpoint: false,
                    NullLogger.Instance).GetAwaiter();

                result.GetResult();

                logger.LogInformation("Package deleted = {Id} {Version} Result = {Result}",
                    item.Key,
                    item.Value,
                    result.IsCompleted);
            }

            logger.LogInformation("===========> Completed delete packages");

        }

    }
}
