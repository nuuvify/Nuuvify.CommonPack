using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.HealthCheck;
using Shouldly;
using Xunit;

namespace Nuuvify.CommonPack.HealthCheck.xTest;

[Trait("Category", "Unit")]
public sealed class HealthCheckExtensionsEnabledTests
{
    [Fact]
    public void AddHealthCheckServiceBuilder_WhenChecksAreEnabled_ShouldConfigureSqliteStorage()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["HealthCheckCustomConfiguration:EnableChecksStandard"] = "true",
            ["HealthCheckCustomConfiguration:UrlHealthCheck"] = "/health",
            ["HealthCheckCustomConfiguration:EvaluationTimeInSeconds"] = "10",
            ["HealthCheckCustomConfiguration:MaximumHistoryEntriesPerEndpoint"] = "10",
            ["HealthCheckCustomConfiguration:MinimumSecondsBetweenFailureNotifications"] = "10",
            ["HealthCheckCustomConfiguration:SetApiMaxActiveRequests"] = "2",
            ["HealthCheckCustomConfiguration:ProviderStorage"] = "sqlite"
        });
        using var loggerFactory = LoggerFactory.Create(_ => { });

        var result = builder.AddHealthCheckServiceBuilder(loggerFactory);

        result.ShouldNotBeNull();
        builder.Services.ShouldNotBeEmpty();
    }

    [Fact]
    public void UseConfigurationHealthChecks_WhenEnabledAndUrlsAreMissing_ShouldThrowArgumentException()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["HealthCheckCustomConfiguration:EnableChecksStandard"] = "true"
        });
        using var app = builder.Build();

        var exception = Should.Throw<ArgumentException>(() => app.UseConfigurationHealthChecks());

        exception.Message.ShouldContain("UrlHealthCheck");
    }
}
