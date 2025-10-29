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

    Console.WriteLine("Gerenciador de Pacotes NuGet");
    Console.WriteLine("Digite ':q' para sair a qualquer momento");
    Console.WriteLine();

    while (true)
    {
        Console.Write("Informe a versão do pacote a ser excluída (ou ':q' para sair): ");

        ConsoleKeyInfo keyInfo;
        string packageVersion = "";

        // Lê a entrada do usuário caractere por caractere para detectar :q
        while (true)
        {
            keyInfo = Console.ReadKey(intercept: true);

            // Verifica se é :q (dois pontos seguido de q)
            if (keyInfo.KeyChar == ':')
            {
                packageVersion += keyInfo.KeyChar;
                Console.Write(keyInfo.KeyChar);

                // Aguarda o próximo caractere
                var nextKeyInfo = Console.ReadKey(intercept: true);
                if (nextKeyInfo.KeyChar == 'q' || nextKeyInfo.KeyChar == 'Q')
                {
                    Console.Write(nextKeyInfo.KeyChar);
                    Console.WriteLine();
                    Console.WriteLine("Operação cancelada pelo usuário.");
                    goto ExitLoop;
                }
                else
                {
                    // Se não for 'q', adiciona os dois caracteres normalmente
                    packageVersion += nextKeyInfo.KeyChar;
                    Console.Write(nextKeyInfo.KeyChar);
                }
                continue;
            }

            // Verifica se é Enter
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }

            // Verifica se é Backspace
            if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (packageVersion.Length > 0)
                {
                    packageVersion = packageVersion[..^1];
                    Console.Write("\b \b");
                }
            }
            // Adiciona caracteres normais
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                packageVersion += keyInfo.KeyChar;
                Console.Write(keyInfo.KeyChar);
            }
        }

        if (!string.IsNullOrWhiteSpace(packageVersion))
        {
            try
            {
                var nugetCustomManagementPackage = new NugetCustomManagementPackage(packageVersion);
                nugetCustomManagementPackage.SetPackageVersionToDelete();
                await nugetCustomManagementPackage.DeletePackage(logger, builder.Environment.EnvironmentName, default);
                Console.WriteLine($"✅ Pacote versão {packageVersion} processado com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao processar pacote versão {packageVersion}: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("⚠️ Versão não informada. Tente novamente.");
        }

        Console.WriteLine();
    }

ExitLoop:
    var host = builder.Build();

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
