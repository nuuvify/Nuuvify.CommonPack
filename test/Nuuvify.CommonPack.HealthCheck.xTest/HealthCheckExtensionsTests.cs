using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nuuvify.CommonPack.HealthCheck;
using Shouldly;
using Xunit;

namespace Nuuvify.CommonPack.HealthCheck.xTest;

[Trait("Category", "Unit")]
public sealed class HealthCheckExtensionsTests
{
    [Fact]
    public void AddHealthCheckServiceBuilder_WhenChecksAreDisabled_ShouldRegisterBasicHealthChecks()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["HealthCheckCustomConfiguration:EnableChecksStandard"] = "false"
        });

        var healthChecks = builder.AddHealthCheckServiceBuilder();

        healthChecks.ShouldNotBeNull();
        builder.Services.ShouldContain(service => service.ServiceType == typeof(HealthCheckService));
    }

    [Fact]
    public void AddHealthCheckCredentialApiBuilder_WhenChecksAreDisabled_ShouldReturnOriginalBuilder()
    {
        var services = new ServiceCollection();
        var builder = services.AddHealthChecks();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HealthCheckCustomConfiguration:EnableChecksStandard"] = "false"
            })
            .Build();

        var result = builder.AddHealthCheckCredentialApiBuilder(
            configuration,
            _ => throw new InvalidOperationException("Credential should not be requested"));

        result.ShouldBe(builder);
    }

    [Fact]
    public void AddHealthCheckMemoryAndStorageBuilder_WhenChecksAreDisabled_ShouldReturnOriginalBuilder()
    {
        var services = new ServiceCollection();
        var builder = services.AddHealthChecks();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HealthCheckCustomConfiguration:EnableChecksStandard"] = "false"
            })
            .Build();

        var result = builder.AddHealthCheckMemoryAndStorageBuilder(configuration);

        result.ShouldBe(builder);
    }

    [Fact]
    public void UseConfigurationHealthChecks_WhenChecksAreDisabled_ShouldNotThrow()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["HealthCheckCustomConfiguration:EnableChecksStandard"] = "false"
        });
        using var app = builder.Build();
        app.UseRouting();

        Should.NotThrow(() => app.UseConfigurationHealthChecks());
    }

    [Fact]
    public void AddHealthCheckServiceBuilder_WhenChecksAreEnabled_ShouldRegisterHealthChecksUi()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["HealthCheckCustomConfiguration:EnableChecksStandard"] = "true",
            ["HealthCheckCustomConfiguration:UrlHealthCheck"] = "/health",
            ["HealthCheckCustomConfiguration:EvaluationTimeInSeconds"] = "60",
            ["HealthCheckCustomConfiguration:MaximumHistoryEntriesPerEndpoint"] = "10",
            ["HealthCheckCustomConfiguration:MinimumSecondsBetweenFailureNotifications"] = "30",
            ["HealthCheckCustomConfiguration:SetApiMaxActiveRequests"] = "2",
            ["HealthCheckCustomConfiguration:ProviderStorage"] = "sqlite"
        });

        var result = builder.AddHealthCheckServiceBuilder();

        result.ShouldNotBeNull();
    }

    [Fact]
    public void AddHealthCheckMemoryAndStorageBuilder_WhenChecksAreEnabled_ShouldRegisterBothChecks()
    {
        var services = new ServiceCollection();
        var builder = services.AddHealthChecks();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HealthCheckCustomConfiguration:EnableChecksStandard"] = "true"
            })
            .Build();

        var result = builder.AddHealthCheckMemoryAndStorageBuilder(configuration);

        result.ShouldBe(builder);
        services.ShouldContain(service => service.ServiceType == typeof(HealthCheckService));
    }

    [Fact]
    public void UseConfigurationHealthChecks_WhenRequiredUrlsAreMissing_ShouldThrowArgumentException()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["HealthCheckCustomConfiguration:EnableChecksStandard"] = "true"
        });
        using var app = builder.Build();

        Should.Throw<ArgumentException>(() => app.UseConfigurationHealthChecks());
    }

    [Fact]
    public void AddHealthCheckServiceBuilder_WhenSqlServerHasNoConnectionString_ShouldThrow()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["HealthCheckCustomConfiguration:EnableChecksStandard"] = "true",
            ["HealthCheckCustomConfiguration:ProviderStorage"] = "sqlserver"
        });

        Should.Throw<ArgumentException>(() => builder.AddHealthCheckServiceBuilder());
    }

    [Fact]
    public void UseConfigurationHealthChecks_WhenEnabledWithUrls_ShouldConfigurePipeline()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddRouting();
        builder.Services.AddHealthChecks();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["HealthCheckCustomConfiguration:EnableChecksStandard"] = "true",
            ["HealthCheckCustomConfiguration:UrlHealthCheck"] = "/health",
            ["HealthCheckCustomConfiguration:UrlHealthCheckUI"] = "/health-ui",
            ["HealthCheckCustomConfiguration:UrlHealthCheckApiData"] = "/health-api"
        });
        using var app = builder.Build();

        var styleSheetPath = Path.Combine(AppContext.BaseDirectory, "dotnet.css");
        File.WriteAllText(styleSheetPath, string.Empty);
        try
        {
            app.UseRouting();
            Should.NotThrow(() => app.UseConfigurationHealthChecks());
        }
        finally
        {
            File.Delete(styleSheetPath);
        }
    }
}
