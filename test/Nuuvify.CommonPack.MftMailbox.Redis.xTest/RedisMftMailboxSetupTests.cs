using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Nuuvify.CommonPack.MftMailbox;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;
using Nuuvify.CommonPack.MftMailbox.Configuration;
using Nuuvify.CommonPack.MftMailbox.Redis;
using Nuuvify.CommonPack.MftMailbox.Redis.Configuration;
using StackExchange.Redis;
using Xunit;

namespace Nuuvify.CommonPack.MftMailbox.Redis.xTest;

/// <summary>
/// Unit tests for <see cref="RedisMftMailboxSetup"/>.
/// </summary>
[Trait("Category", "Unit")]
public class RedisMftMailboxSetupTests
{
    [Fact]
    public void AddMftMailboxRedis_WithoutConnectionMultiplexer_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddMftMailboxRedis());

        // Assert
        Assert.Contains("IConnectionMultiplexer", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AddMftMailboxRedis_WithImplementationType_RegistersDecorator()
    {
        // Arrange
        var services = CreateBaseServices();
        services.AddSingleton<IMftStatusClient, FakeStatusClient>();

        // Act
        services.AddMftMailboxRedis(options =>
        {
            options.EnableStatusCache = true;
            options.EnableAuditStream = false;
        });

        // Assert
        using var provider = services.BuildServiceProvider();
        var statusClient = provider.GetRequiredService<IMftStatusClient>();

        Assert.IsType<Nuuvify.CommonPack.MftMailbox.Redis.Services.RedisCachedStatusClient>(statusClient);
        var result = await statusClient.GetStatusAsync("integration", "item", CancellationToken.None);
        Assert.NotNull(result);
        Assert.Equal("integration", result!.IntegrationKey);
    }

    [Fact]
    public void AddMftMailboxRedis_WithImplementationFactory_RegistersDecorator()
    {
        // Arrange
        var services = CreateBaseServices();
        services.AddSingleton<IMftStatusClient>(_ => new FakeStatusClient());

        // Act
        services.AddMftMailboxRedis(options =>
        {
            options.EnableStatusCache = true;
            options.EnableAuditStream = false;
        });

        // Assert
        using var provider = services.BuildServiceProvider();
        var statusClient = provider.GetRequiredService<IMftStatusClient>();

        Assert.IsType<Nuuvify.CommonPack.MftMailbox.Redis.Services.RedisCachedStatusClient>(statusClient);
    }

    [Fact]
    public void AddMftMailboxRedis_WithImplementationInstance_RegistersDecorator()
    {
        // Arrange
        var services = CreateBaseServices();
        services.AddSingleton<IMftStatusClient>(new FakeStatusClient());

        // Act
        services.AddMftMailboxRedis(options =>
        {
            options.EnableStatusCache = true;
            options.EnableAuditStream = false;
        });

        // Assert
        using var provider = services.BuildServiceProvider();
        var statusClient = provider.GetRequiredService<IMftStatusClient>();

        Assert.IsType<Nuuvify.CommonPack.MftMailbox.Redis.Services.RedisCachedStatusClient>(statusClient);
    }

    private static ServiceCollection CreateBaseServices()
    {
        var services = new ServiceCollection();

        services.AddOptions();
        services.AddLogging();
        services.AddMftMailboxCore(_ =>
        {
            // Keep default options for unit tests.
        });

        var connectionMultiplexer = new Mock<IConnectionMultiplexer>();
        var database = new Mock<IDatabase>(MockBehavior.Loose);

        connectionMultiplexer
            .Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(database.Object);

        services.AddSingleton(connectionMultiplexer.Object);
        services.AddSingleton<NullLoggerFactory>(_ => NullLoggerFactory.Instance);
        services.AddSingleton<IOptions<MftMailboxOptions>>(Options.Create(new MftMailboxOptions()));
        services.AddSingleton<IOptions<RedisMftMailboxOptions>>(Options.Create(new RedisMftMailboxOptions()));

        return services;
    }

    private sealed class FakeStatusClient : IMftStatusClient
    {
        public Task<TransferStatus?> GetStatusAsync(string integrationKey, string itemId, CancellationToken cancellationToken = default)
        {
            var status = new TransferStatus
            {
                IntegrationKey = integrationKey,
                ItemId = itemId,
                State = TransferState.Succeeded
            };

            return Task.FromResult<TransferStatus?>(status);
        }
    }
}
