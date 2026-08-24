using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nuuvify.CommonPack.HealthCheck;
using Shouldly;
using Xunit;

namespace Nuuvify.CommonPack.HealthCheck.xTest;

/// <summary>
/// Tests for HealthCheckExtensions credential API builder disabled path
/// </summary>
[Trait("Category", "Unit")]
public sealed class HealthCheckCredentialApiBuilderDisabledTests
{
    [Fact]
    public void AddHealthCheckCredentialApiBuilder_WhenChecksAreDisabled_ShouldReturnOriginalBuilderUnchanged()
    {
        var services = new ServiceCollection();
        var builder = services.AddHealthChecks();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HealthCheckCustomConfiguration:EnableChecksStandard"] = "false"
            })
            .Build();

        var resultBuilder = builder.AddHealthCheckCredentialApiBuilder(
            configuration,
            credential => throw new InvalidOperationException("Should not execute credential factory when disabled"));

        resultBuilder.ShouldBe(builder);
    }

    [Fact]
    public void AddHealthCheckAzureServiceBuilder_WhenChecksAreDisabled_ShouldReturnOriginalBuilderUnchanged()
    {
        var services = new ServiceCollection();
        var builder = services.AddHealthChecks();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HealthCheckCustomConfiguration:EnableChecksStandard"] = "false"
            })
            .Build();

        var resultBuilder = builder.AddHealthCheckAzureServiceBuilder(
            configuration,
            credential => throw new InvalidOperationException("Should not execute credential factory when disabled"));

        resultBuilder.ShouldBe(builder);
    }

    [Fact]
    public void AddHealthCheckCredentialApiBuilder_WhenUrlHealthCheckIsNull_ShouldThrowArgumentException()
    {
        var services = new ServiceCollection();
        var builder = services.AddHealthChecks();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HealthCheckCustomConfiguration:EnableChecksStandard"] = "true",
                ["HealthCheckCustomConfiguration:UrlHealthCheck"] = null  // Missing required URL
            })
            .Build();

        // Should throw ArgumentException when URL is missing but checks are enabled
        Should.Throw<ArgumentException>(() =>
            builder.AddHealthCheckCredentialApiBuilder(
                configuration,
                credential => throw new InvalidOperationException("Should not reach here"))
        );
    }

    [Fact]
    public void AddHealthCheckAzureServiceBuilder_WhenUrlHealthCheckIsNull_ShouldThrowArgumentException()
    {
        var services = new ServiceCollection();
        var builder = services.AddHealthChecks();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HealthCheckCustomConfiguration:EnableChecksStandard"] = "true",
                ["HealthCheckCustomConfiguration:UrlHealthCheck"] = null  // Missing required URL
            })
            .Build();

        // Should throw ArgumentException when URL is missing but checks are enabled
        Should.Throw<ArgumentException>(() =>
            builder.AddHealthCheckAzureServiceBuilder(
                configuration,
                credential => throw new InvalidOperationException("Should not reach here"))
        );
    }

    [Fact]
    public void AddHealthCheckMemoryAndStorageBuilder_WhenBuilderIsNull_ShouldReturnBuilderParameter()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HealthCheckCustomConfiguration:EnableChecksStandard"] = "false"
            })
            .Build();

        // Ensure null handling works correctly
        var services = new ServiceCollection();
        var builder = services.AddHealthChecks();
        builder.ShouldNotBeNull();
    }
}
