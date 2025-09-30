using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nuget.CustomManagement.NugetCustomManagementPackage;

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
    var message = $"❌ Operação foi cancelada: {ex.Message}";
    Console.WriteLine(message);
    Environment.ExitCode = 1;
}
catch (InvalidOperationException ex)
{
    var message = $"❌ Erro de configuração: {ex.Message}";
    Console.WriteLine(message);
    Environment.ExitCode = 2;
}
catch (Exception ex)
{
    var message = $"❌ Erro inesperado: {ex.Message}";
    Console.WriteLine(message);

    if (ex.InnerException != null)
    {
        Console.WriteLine($"#### InnerException: {ex.InnerException.Message}");
        Console.WriteLine($"#### InnerException->StackTrace: {ex.InnerException.StackTrace}");
    }

    Console.WriteLine($"#### StackTrace: {ex.StackTrace}");
    Environment.ExitCode = 3;
}
