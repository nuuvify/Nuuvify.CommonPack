using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace Nuget.CustomManagement.NugetCustomManagementPackage;

/// <summary>
/// Gerencia operações de exclusão de pacotes NuGet em diferentes ambientes.
/// Documentação de referência:
/// - https://github.com/NuGet/Samples/blob/main/NuGetProtocolSamples/Program.cs
/// - https://learn.microsoft.com/en-us/nuget/reference/nuget-client-sdk
/// </summary>
public class NugetCustomManagementPackage
{
    public IDictionary<string, string> Packages { get; set; }
    private string PackageVersion { get; }

    public NugetCustomManagementPackage(string packageVersion)
    {
        PackageVersion = packageVersion;

    }

    public void SetPackageVersionToDelete()
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

    /// <summary>
    /// Executa a exclusão de pacotes NuGet baseado no ambiente especificado.
    /// </summary>
    /// <param name="logger">Logger para registrar operações e erros</param>
    /// <param name="environmentName">Nome do ambiente (Development, Quality, Staging). Production é bloqueado.</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Task representando a operação assíncrona</returns>
    /// <exception cref="InvalidOperationException">Lançado quando o ambiente é Production ou API key não está configurada</exception>
    public async Task DeletePackage(Microsoft.Extensions.Logging.ILogger logger, string environmentName, CancellationToken cancellationToken)
    {
        string apiKey = ValidateAndGetApiKey();
        var nugetConfig = GetNuGetConfigurationForEnvironment(logger, environmentName);

        using var cacheNuget = new SourceCacheContext();
        SourceRepository repository = Repository.Factory.GetCoreV3(nugetConfig);
        PackageUpdateResource resource = await repository.GetResourceAsync<PackageUpdateResource>(cancellationToken);

        ArgumentNullException.ThrowIfNull(resource);

        await DeletePackagesAsync(logger, resource, apiKey, nugetConfig.Source, cancellationToken);
    }

    private static string ValidateAndGetApiKey()
    {
        string apiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("NUGET_API_KEY environment variable is required");
        }
        return apiKey;
    }

    private static NuGet.Configuration.PackageSource GetNuGetConfigurationForEnvironment(Microsoft.Extensions.Logging.ILogger logger, string environmentName)
    {
        const string testEndpoint = "https://apiint.nugettest.org/v3/index.json";

        if (environmentName.Equals("Production", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Operation not allowed in PRODUCTION environment");
        }

        if (environmentName.Equals("Development", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("Using NuGet TEST endpoint for DEVELOPMENT environment");
            return new NuGet.Configuration.PackageSource(testEndpoint);
        }

        if (environmentName.Equals("Quality", StringComparison.OrdinalIgnoreCase) ||
            environmentName.Equals("Staging", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("Using NuGet TEST endpoint for {Environment} environment", environmentName.ToUpperInvariant());
            return new NuGet.Configuration.PackageSource(testEndpoint);
        }

        logger.LogWarning("Unknown environment '{Environment}', defaulting to NuGet TEST endpoint", environmentName);
        return new NuGet.Configuration.PackageSource(testEndpoint);
    }

    private async Task DeletePackagesAsync(Microsoft.Extensions.Logging.ILogger logger, PackageUpdateResource resource, string apiKey, string endpoint, CancellationToken cancellationToken)
    {
        logger.LogInformation("===========> Start delete packages (Total: {Count}) using endpoint: {Endpoint}", Packages.Count, endpoint);

        int successCount = 0;
        int failureCount = 0;

        foreach (var item in Packages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            bool success = await TryDeleteSinglePackage(logger, resource, apiKey, item, cancellationToken);
            if (success)
                successCount++;
            else
                failureCount++;
        }

        logger.LogInformation("===========> Completed delete packages. Success: {Success}, Failures: {Failures}",
            successCount, failureCount);
    }

    private static async Task<bool> TryDeleteSinglePackage(
        Microsoft.Extensions.Logging.ILogger logger,
        PackageUpdateResource resource,
        string apiKey,
        KeyValuePair<string, string> package,
        CancellationToken cancellationToken)
    {
        try
        {
            await resource.Delete(
                package.Key,
                package.Value,
                getApiKey: packageSource => apiKey,
                confirm: packageSource => true,
                noServiceEndpoint: false,
                NullLogger.Instance);

            logger.LogInformation("✅ Package deleted successfully: {Id} {Version}", package.Key, package.Value);
            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "❌ Network error deleting package: {Id} {Version}. Error: {Error}",
                package.Key, package.Value, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogError(ex, "❌ Authorization error deleting package: {Id} {Version}. Check API key. Error: {Error}",
                package.Key, package.Value, ex.Message);
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "❌ Invalid package parameters: {Id} {Version}. Error: {Error}",
                package.Key, package.Value, ex.Message);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            logger.LogError(ex, "❌ Timeout deleting package: {Id} {Version}. Error: {Error}",
                package.Key, package.Value, ex.Message);
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Unexpected error deleting package: {Id} {Version}. Error: {Error}",
                package.Key, package.Value, ex.Message);
        }
#pragma warning restore CA1031 // Do not catch general exception types

        return false;
    }
}
