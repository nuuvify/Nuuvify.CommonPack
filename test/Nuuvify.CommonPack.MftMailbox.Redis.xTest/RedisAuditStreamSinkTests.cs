using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;
using Nuuvify.CommonPack.MftMailbox.Redis.Configuration;
using Nuuvify.CommonPack.MftMailbox.Redis.Services;
using Nuuvify.CommonPack.MftMailbox.Redis.Utilities;
using StackExchange.Redis;
using Xunit;

namespace Nuuvify.CommonPack.MftMailbox.Redis.xTest;

/// <summary>
/// Unit tests for <see cref="RedisAuditStreamSink"/>.
/// </summary>
[Trait("Category", "Unit")]
public class RedisAuditStreamSinkTests
{
    private readonly Mock<IDatabase> _mockDatabase;
    private readonly Mock<IConnectionMultiplexer> _mockConnectionMultiplexer;
    private readonly Mock<ILogger<RedisAuditStreamSink>> _mockLogger;
    private readonly RedisMftMailboxOptions _options;
    private readonly RedisAuditSerializer _serializer;

    public RedisAuditStreamSinkTests()
    {
        _mockDatabase = new Mock<IDatabase>(MockBehavior.Loose);
        _mockConnectionMultiplexer = new Mock<IConnectionMultiplexer>();
        _mockConnectionMultiplexer
            .Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_mockDatabase.Object);

        _mockLogger = new Mock<ILogger<RedisAuditStreamSink>>();
        _options = new RedisMftMailboxOptions { EnableAuditStream = true };
        _serializer = new RedisAuditSerializer();
    }

    [Fact]
    public async Task WriteAsync_WithValidEntry_DoesNotThrow()
    {
        // Arrange
        var sink = new RedisAuditStreamSink(
            _mockConnectionMultiplexer.Object,
            Options.Create(_options),
            _serializer,
            _mockLogger.Object);

        _mockDatabase
            .Setup(db => db.StreamAddAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<NameValueEntry[]>(),
                It.IsAny<RedisValue?>(),
                It.IsAny<int?>(),
                It.IsAny<bool>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync("1-0");

        _mockDatabase
            .Setup(db => db.StreamTrimAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(1L);

        var entry = new TransferAuditEntry
        {
            IntegrationKey = "integration-1",
            ItemId = "item-123"
        };

        // Act & Assert - should not throw
        var exception = await Record.ExceptionAsync(
            () => sink.WriteAsync(entry, CancellationToken.None));

        Assert.Null(exception);
    }

    [Fact]
    public async Task WriteAsync_WithConnectionFailure_SilentlyHandlesException()
    {
        // Arrange
        var sink = new RedisAuditStreamSink(
            _mockConnectionMultiplexer.Object,
            Options.Create(_options),
            _serializer,
            _mockLogger.Object);

        _mockDatabase
            .Setup(db => db.StreamAddAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<NameValueEntry[]>(),
                It.IsAny<RedisValue?>(),
                It.IsAny<int?>(),
                It.IsAny<bool>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync("1-0");

        // Make mock throw on any method call (simulating Redis connection failure)
        _mockDatabase
            .Setup(db => db.StreamTrimAsync(It.IsAny<RedisKey>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisConnectionException(
                ConnectionFailureType.UnableToConnect,
                "Connection lost"));

        var entry = new TransferAuditEntry
        {
            IntegrationKey = "integration-2",
            ItemId = "item-456"
        };

        // Act & Assert - should not throw (fire-and-forget pattern)
        var exception = await Record.ExceptionAsync(
            () => sink.WriteAsync(entry, CancellationToken.None));

        Assert.Null(exception);
    }

    [Fact]
    public async Task WriteAsync_WithDisabledFeature_DoesNotThrow()
    {
        // Arrange
        _options.EnableAuditStream = false;

        var sink = new RedisAuditStreamSink(
            _mockConnectionMultiplexer.Object,
            Options.Create(_options),
            _serializer,
            _mockLogger.Object);

        _mockDatabase
            .Setup(db => db.StreamAddAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<NameValueEntry[]>(),
                It.IsAny<RedisValue?>(),
                It.IsAny<int?>(),
                It.IsAny<bool>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync("1-0");

        _mockDatabase
            .Setup(db => db.StreamTrimAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(1L);

        var entry = new TransferAuditEntry
        {
            IntegrationKey = "integration-3",
            ItemId = "item-789"
        };

        // Act & Assert - should not throw
        var exception = await Record.ExceptionAsync(
            () => sink.WriteAsync(entry, CancellationToken.None));

        Assert.Null(exception);
    }

    [Fact]
    public async Task WriteAsync_WithNullEntry_ThrowsArgumentNullException()
    {
        // Arrange
        var sink = new RedisAuditStreamSink(
            _mockConnectionMultiplexer.Object,
            Options.Create(_options),
            _serializer,
            _mockLogger.Object);

        _mockDatabase
            .Setup(db => db.StreamAddAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<NameValueEntry[]>(),
                It.IsAny<RedisValue?>(),
                It.IsAny<int?>(),
                It.IsAny<bool>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync("1-0");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sink.WriteAsync(null!, CancellationToken.None));
    }
}
