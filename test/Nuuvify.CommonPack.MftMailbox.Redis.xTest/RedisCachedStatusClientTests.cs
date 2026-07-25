using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Nuuvify.CommonPack.MftMailbox.Abstraction;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;
using Nuuvify.CommonPack.MftMailbox.Redis.Configuration;
using Nuuvify.CommonPack.MftMailbox.Redis.Services;
using Nuuvify.CommonPack.MftMailbox.Redis.Utilities;
using StackExchange.Redis;
using Xunit;

namespace Nuuvify.CommonPack.MftMailbox.Redis.xTest;

/// <summary>
/// Unit tests for <see cref="RedisCachedStatusClient"/>.
/// </summary>
[Trait("Category", "Unit")]
public class RedisCachedStatusClientTests
{
    private readonly Mock<IDatabase> _mockDatabase;
    private readonly Mock<IConnectionMultiplexer> _mockConnectionMultiplexer;
    private readonly Mock<IMftStatusClient> _mockInnerClient;
    private readonly Mock<ILogger<RedisCachedStatusClient>> _mockLogger;
    private readonly RedisMftMailboxOptions _options;
    private readonly RedisStatusSerializer _serializer;

    public RedisCachedStatusClientTests()
    {
        _mockDatabase = new Mock<IDatabase>(MockBehavior.Loose);
        _mockConnectionMultiplexer = new Mock<IConnectionMultiplexer>();
        _mockConnectionMultiplexer
            .Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_mockDatabase.Object);

        _mockInnerClient = new Mock<IMftStatusClient>();
        _mockLogger = new Mock<ILogger<RedisCachedStatusClient>>();
        _options = new RedisMftMailboxOptions { EnableStatusCache = true };
        _serializer = new RedisStatusSerializer();
    }

    [Fact]
    public async Task GetStatusAsync_WithCacheHit_ReturnsCachedValue()
    {
        // Arrange
        var client = new RedisCachedStatusClient(
            _mockInnerClient.Object,
            _mockConnectionMultiplexer.Object,
            Options.Create(_options),
            _serializer,
            _mockLogger.Object);

        var status = new TransferStatus
        {
            IntegrationKey = "integration-1",
            ItemId = "item-123",
            State = TransferState.Succeeded
        };
        var cachedJson = System.Text.Json.JsonSerializer.Serialize(status);

        _mockDatabase
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(cachedJson);

        // Act
        var result = await client.GetStatusAsync("integration-1", "item-123", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(status.IntegrationKey, result.IntegrationKey);
        Assert.Equal(status.State, result.State);
        _mockInnerClient.Verify(
            x => x.GetStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetStatusAsync_WithCacheMiss_CallsInnerClient()
    {
        // Arrange
        var client = new RedisCachedStatusClient(
            _mockInnerClient.Object,
            _mockConnectionMultiplexer.Object,
            Options.Create(_options),
            _serializer,
            _mockLogger.Object);

        _mockDatabase
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        var status = new TransferStatus
        {
            IntegrationKey = "integration-2",
            ItemId = "item-456",
            State = TransferState.InProgress
        };
        _mockInnerClient
            .Setup(x => x.GetStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(status);

        _mockDatabase
            .Setup(x => x.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        var result = await client.GetStatusAsync("integration-2", "item-456", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(status.IntegrationKey, result.IntegrationKey);
        _mockInnerClient.Verify(
            x => x.GetStatusAsync("integration-2", "item-456", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetStatusAsync_WithNullInnerResponse_CachesNull()
    {
        // Arrange
        var mockDb = new Mock<IDatabase>(MockBehavior.Loose);
        mockDb.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>())).ReturnsAsync(RedisValue.Null);
        mockDb
            .Setup(x => x.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var mockConnMulti = new Mock<IConnectionMultiplexer>();
        mockConnMulti.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDb.Object);

        var mockInnerClient = new Mock<IMftStatusClient>();
        mockInnerClient.Setup(x => x.GetStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((TransferStatus)null!);

        var client = new RedisCachedStatusClient(
            mockInnerClient.Object,
            mockConnMulti.Object,
            Options.Create(_options),
            new RedisStatusSerializer(),
            new Mock<ILogger<RedisCachedStatusClient>>().Object);

        // Act
        var result = await client.GetStatusAsync("integration-null", "item-null", CancellationToken.None);

        // Assert - verify null response is returned and cached
        // (Implementation caches null with 1-min TTL via SetNegativeCacheAsync)
        Assert.Null(result);
        mockInnerClient.Verify(x => x.GetStatusAsync("integration-null", "item-null", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStatusAsync_WithConnectionFailure_FallsBackToInnerClient()
    {
        // Arrange
        var client = new RedisCachedStatusClient(
            _mockInnerClient.Object,
            _mockConnectionMultiplexer.Object,
            Options.Create(_options),
            _serializer,
            _mockLogger.Object);

        _mockDatabase
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisConnectionException(
                ConnectionFailureType.UnableToConnect,
                "Redis unavailable"));

        var status = new TransferStatus
        {
            IntegrationKey = "integration-3",
            ItemId = "item-789",
            State = TransferState.Failed
        };
        _mockInnerClient
            .Setup(x => x.GetStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(status);

        // Act
        var result = await client.GetStatusAsync("integration-3", "item-789", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(status.IntegrationKey, result.IntegrationKey);
        _mockInnerClient.Verify(
            x => x.GetStatusAsync("integration-3", "item-789", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetStatusAsync_WithDisabledCache_AlwaysCallsInnerClient()
    {
        // Arrange
        _options.EnableStatusCache = false;

        var client = new RedisCachedStatusClient(
            _mockInnerClient.Object,
            _mockConnectionMultiplexer.Object,
            Options.Create(_options),
            _serializer,
            _mockLogger.Object);

        var status = new TransferStatus
        {
            IntegrationKey = "integration-4",
            ItemId = "item-999",
            State = TransferState.Pending
        };

        _mockDatabase
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);  // Cache miss

        _mockInnerClient
            .Setup(x => x.GetStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(status);

        // Act
        var result = await client.GetStatusAsync("integration-4", "item-999", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(status.IntegrationKey, result.IntegrationKey);
        _mockInnerClient.Verify(
            x => x.GetStatusAsync("integration-4", "item-999", It.IsAny<CancellationToken>()),
            Times.Once);

        // Com cache desabilitado, Redis não deve ser consultado.
        _mockDatabase.Verify(
            x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()),
            Times.Never);
    }
}
