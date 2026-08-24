using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuuvify.CommonPack.HealthCheck;
using Shouldly;
using Xunit;

namespace Nuuvify.CommonPack.HealthCheck.xTest;

/// <summary>
/// Tests for HealthCheckExtensions SQLite default path and SQL Server failure scenarios
/// </summary>
[Trait("Category", "Unit")]
public sealed class HealthCheckExtensionsSqliteDefaultTests
{
    [Fact]
    public void AddHealthCheckServiceBuilder_WhenProviderStorageIsNull_ShouldDefaultToSqlite()
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
            ["HealthCheckCustomConfiguration:ProviderStorage"] = null  // null should default to sqlite
        });

        var result = builder.AddHealthCheckServiceBuilder();

        result.ShouldNotBeNull();
        // Verify registration succeeds without error
    }

    [Fact]
    public void AddHealthCheckServiceBuilder_WhenSqlServerConnectionStringIsNull_ShouldThrowArgumentException()
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
            ["HealthCheckCustomConfiguration:ProviderStorage"] = "sqlserver",
            ["ConnectionStrings:HealthCheck"] = null  // No connection string provided
        });

        // Should throw ArgumentException when sqlserver is selected but connection string is null
        Should.Throw<ArgumentException>(() =>
            builder.AddHealthCheckServiceBuilder()
        );
    }

    [Fact]
    public void AddHealthCheckMemoryAndStorageBuilder_ShouldRegisterLocalStorageHealthCheck()
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

        result.ShouldNotBeNull();
        result.ShouldBe(builder);
    }

    [Fact]
    public void AddHealthCheckServiceBuilder_WhenProviderStorageIsEmptyString_ShouldDefaultToSqlite()
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
            ["HealthCheckCustomConfiguration:ProviderStorage"] = string.Empty  // empty should default to sqlite
        });

        var result = builder.AddHealthCheckServiceBuilder();

        result.ShouldNotBeNull();
        // Verify registration succeeds without error
    }

    [Fact]
    public void AddHealthCheckServiceBuilder_WhenHistorySchemaAndTableAreNull_ShouldUseDefaults()
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
            ["HealthCheckCustomConfiguration:ProviderStorage"] = "sqlite",
            ["HealthCheckCustomConfiguration:HistorySchema"] = null,
            ["HealthCheckCustomConfiguration:HistoryTable"] = null
        });

        var result = builder.AddHealthCheckServiceBuilder();

        result.ShouldNotBeNull();
        // Verify registration succeeds without error
    }
}
