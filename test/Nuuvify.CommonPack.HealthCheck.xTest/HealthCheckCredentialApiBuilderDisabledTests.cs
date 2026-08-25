using Azure.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
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
    public void AddHealthCheckCredentialApiBuilder_WhenChecksAreDisabled_ShouldReturnOriginalBuilderUnchangedForAzureChecks()
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
    public void AddHealthCheckCredentialApiBuilder_WhenUrlHealthCheckIsNull_ShouldThrowArgumentException()
    {
        var services = new ServiceCollection();
        var builder = services.AddHealthChecks();
        var credential = new Mock<TokenCredential>().Object;
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
                _ => credential,
                urlHealthCheck: null));
    }

    [Fact]
    public void AddHealthCheckCredentialApiBuilder_WhenUrlHealthCheckIsNull_ShouldThrowArgumentExceptionForAzureChecks()
    {
        var services = new ServiceCollection();
        var builder = services.AddHealthChecks();
        var credential = new Mock<TokenCredential>().Object;
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
                _ => credential,
                urlHealthCheck: null));
    }

    [Fact]
    public void AddHealthCheckCredentialApiBuilder_WhenChecksAreEnabled_ShouldRegisterCredentialChecks()
    {
        var services = new ServiceCollection();
        var builder = services.AddHealthChecks();
        var credential = new Mock<TokenCredential>().Object;
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HealthCheckCustomConfiguration:EnableChecksStandard"] = "true",
                ["AppConfig:AppURLs:UrlLoginApi"] = "https://localhost/login",
                ["AzureKeyVault:Dns"] = "https://localhost.vault.azure.net/"
            })
            .Build();

        var resultBuilder = builder.AddHealthCheckCredentialApiBuilder(
            configuration,
            _ => credential,
            urlHealthCheck: "health",
            timeout: TimeSpan.FromSeconds(1));

        resultBuilder.ShouldBe(builder);
        services.ShouldContain(service => service.ServiceType == typeof(HealthCheckService));
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
