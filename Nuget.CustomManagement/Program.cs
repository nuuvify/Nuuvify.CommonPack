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
        builder.Services.AddWindowsService(options =>
        {
            options.ServiceName = ".NET Windows Service";
        });
    }
    else
    {
        builder.Services.AddSystemd();
    }

    var serviceProvider = builder.Services.BuildServiceProvider();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("**** Environment: {EnvironmentName} ****", builder.Environment.EnvironmentName);


    var nugetCustomManagementPackage = new NugetCustomManagementPackage();
    await nugetCustomManagementPackage.DeletePackage(logger, default);


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


    public IDictionary<string, string> Packages { get; set; }

    public NugetCustomManagementPackage()
    {

        Packages = new Dictionary<string, string>
        {

            { "Nuuvify.CommonPack.AutoHistory", "2.0.0-preview.24092002" },
            { "Nuuvify.CommonPack.AzureStorage", "2.0.0-preview.24092002" },
            { "Nuuvify.CommonPack.AzureStorage.Abstraction", "2.0.0-preview.24092002" },
            { "Nuuvify.CommonPack.Domain", "2.0.0-preview.24092002" },
            { "Nuuvify.CommonPack.EF.Exceptions.Common", "2.0.0-preview.24092002" },
            { "Nuuvify.CommonPack.EF.Exceptions.Db2", "2.0.0-preview.24092002" },
            { "Nuuvify.CommonPack.EF.Exceptions.Oracle", "2.0.0-preview.24092002" },
            { "Nuuvify.CommonPack.Email", "2.0.0-preview.24092002" },
            { "Nuuvify.CommonPack.Email.Abstraction", "2.0.0-preview.24092002" },
            { "Nuuvify.CommonPack.Extensions", "2.0.0-preview.24092002" },
            { "Nuuvify.CommonPack.HealthCheck", "2.0.0-preview.24092002" },
            { "Nuuvify.CommonPack.Middleware", "2.0.0-preview.24092002" },
            { "Nuuvify.CommonPack.Middleware.Abstraction", "2.0.0-preview.24092002" },
            { "Nuuvify.CommonPack.OpenApi", "2.0.0-preview.24092002" },
            { "Nuuvify.CommonPack.Security", "2.0.0-preview.24092002" },
            { "Nuuvify.CommonPack.Security.Abstraction", "2.0.0-preview.24092002" },
            { "Nuuvify.CommonPack.Security.JwtCredentials", "2.0.0-preview.24092002" },
            { "Nuuvify.CommonPack.Security.JwtStore.Ef", "2.0.0-preview.24092002" },
            { "Nuuvify.CommonPack.UnitOfWork", "2.0.0-preview.24092002" },
            { "Nuuvify.CommonPack.UnitOfWork.Abstraction", "2.0.0-preview.24092002" },
            { "Nuuvify.CommonPack.StandardHttpClient", "2.0.0-preview.24092002" },
        };

    }

    public async Task DeletePackage(Microsoft.Extensions.Logging.ILogger logger, CancellationToken cancellationToken)
    {

        var nugetConfig = new NuGet.Configuration.PackageSource("https://api.nuget.org/v3/index.json");


        SourceCacheContext cacheNuget = new SourceCacheContext();
        SourceRepository repository = Repository.Factory.GetCoreV3(nugetConfig);
        PackageUpdateResource resource = await repository.GetResourceAsync<PackageUpdateResource>();

        string apiKey = "xxxxxxxxx";

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

            logger.LogInformation("Package deleted: {Id} {Version} Result: {Result}",
                item.Key,
                item.Value,
                result.IsCompleted);
        }

        logger.LogInformation("===========> Completed delete packages");


    }

}