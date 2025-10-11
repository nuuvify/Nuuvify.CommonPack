using System.Text;
using System.Reflection;

namespace Nuuvify.CommonPack.AzureServiceBus.xTest.Services;

[Trait("Category", "Unit")]
public class ServiceBusMessageSenderMessageCreationTests : IDisposable
{
    private readonly ServiceBusTestFixture _fixture;
    private readonly ServiceBusMessageSender _sender;
    private bool _disposed;

    public ServiceBusMessageSenderMessageCreationTests()
    {
        _fixture = new ServiceBusTestFixture();
        var config = _fixture.CreateValidServiceBusConfiguration();
        var logger = _fixture.CreateMockLogger<ServiceBusMessageSender>();
        _sender = new ServiceBusMessageSender(config, logger.Object);
    }

    #region CreateServiceBusMessage Tests

    [Fact]
    public void CreateServiceBusMessage_WithValidObjectAndOptions_ShouldCreateMessage()
    {
        // Arrange
        var testObject = new { Id = 1, Name = "Test", Value = 100.50 };
        var options = new ServiceBusMessageOptions
        {
            MessageId = "test-message-id",
            ContentType = "application/json",
            TimeToLive = TimeSpan.FromMinutes(30)
        };

        // Act
        var result = InvokeCreateServiceBusMessage(testObject, options);

        // Assert
        _ = result.ShouldNotBeNull();
        result.MessageId.ShouldBe("test-message-id");
        result.ContentType.ShouldBe("application/json");
        result.TimeToLive.ShouldBe(TimeSpan.FromMinutes(30));

        var bodyString = Encoding.UTF8.GetString(result.Body);
        bodyString.ShouldContain("\"Id\":1");
        bodyString.ShouldContain("\"Name\":\"Test\"");
        bodyString.ShouldContain("\"Value\":100.5");
    }

    [Fact]
    public void CreateServiceBusMessage_WithNullOptions_ShouldCreateMessageWithDefaults()
    {
        // Arrange
        var testObject = new { Message = "Hello World" };

        // Act
        var result = InvokeCreateServiceBusMessage(testObject, null!);

        // Assert
        _ = result.ShouldNotBeNull();
        result.ContentType.ShouldBe("application/json");
        result.TimeToLive.ShouldBeGreaterThan(TimeSpan.Zero);

        var bodyString = Encoding.UTF8.GetString(result.Body);
        bodyString.ShouldContain("\"Message\":\"Hello World\"");
    }

    [Fact]
    public void CreateServiceBusMessage_WithCustomEncoding_ShouldUseSpecifiedEncoding()
    {
        // Arrange
        var testObject = new { Text = "Héllo Wörld" };
        var options = new ServiceBusMessageOptions
        {
            Encoding = Encoding.Unicode
        };

        // Act
        var result = InvokeCreateServiceBusMessage(testObject, options);

        // Assert
        _ = result.ShouldNotBeNull();
        var bodyString = Encoding.Unicode.GetString(result.Body);
        // JSON may escape unicode characters, so check for either format
        (bodyString.Contains("\"Text\":\"Héllo Wörld\"") ||
         bodyString.Contains("\"Text\":\"H\\u00E9llo W\\u00F6rld\"")).ShouldBeTrue();
    }

    [Fact]
    public void CreateServiceBusMessage_WithComplexObject_ShouldSerializeCorrectly()
    {
        // Arrange
        var complexObject = new
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Items = new[] { "item1", "item2", "item3" },
            Metadata = new { Version = "1.0", Source = "test" }
        };

        // Act
        var result = InvokeCreateServiceBusMessage(complexObject, null!);

        // Assert
        _ = result.ShouldNotBeNull();
        var bodyString = Encoding.UTF8.GetString(result.Body);
        bodyString.ShouldContain("\"Id\":");
        bodyString.ShouldContain("\"CreatedAt\":");
        bodyString.ShouldContain("\"Items\":");
        bodyString.ShouldContain("\"Metadata\":");
    }

    #endregion

    #region ConfigureMessageProperties Tests

    [Fact]
    public void ConfigureMessageProperties_WithAllProperties_ShouldSetAllValues()
    {
        // Arrange
        var message = new ServiceBusMessage("test");
        var options = new ServiceBusMessageOptions
        {
            MessageId = "msg-123",
            CorrelationId = "corr-456",
            PartitionKey = "partition-key",
            SessionId = "session-789",
            ContentType = "application/json",
            Subject = "test-subject",
            ReplyTo = "reply-queue",
            ReplyToSessionId = "reply-session",
            To = "destination",
            TimeToLive = TimeSpan.FromHours(2),
            ApplicationProperties = new Dictionary<string, object>
            {
                { "CustomProp1", "Value1" },
                { "CustomProp2", 42 },
                { "CustomProp3", true }
            }
        };

        // Act
        InvokeConfigureMessageProperties(message, options);

        // Assert
        message.MessageId.ShouldBe("msg-123");
        message.CorrelationId.ShouldBe("corr-456");
        message.PartitionKey.ShouldBe("session-789"); // SessionId takes precedence over PartitionKey
        message.SessionId.ShouldBe("session-789");
        message.ContentType.ShouldBe("application/json");
        message.Subject.ShouldBe("test-subject");
        message.ReplyTo.ShouldBe("reply-queue");
        message.ReplyToSessionId.ShouldBe("reply-session");
        message.To.ShouldBe("destination");
        message.TimeToLive.ShouldBe(TimeSpan.FromHours(2));

        message.ApplicationProperties.Count.ShouldBe(3);
        message.ApplicationProperties["CustomProp1"].ShouldBe("Value1");
        message.ApplicationProperties["CustomProp2"].ShouldBe(42);
        message.ApplicationProperties["CustomProp3"].ShouldBe(true);
    }

    [Fact]
    public void ConfigureMessageProperties_WithNullOptions_ShouldSetDefaults()
    {
        // Arrange
        var message = new ServiceBusMessage("test");

        // Act
        InvokeConfigureMessageProperties(message, null!);

        // Assert
        message.ContentType.ShouldBe("application/json");
        message.TimeToLive.ShouldBeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public void ConfigureMessageProperties_WithEmptyStrings_ShouldIgnoreEmptyValues()
    {
        // Arrange
        var message = new ServiceBusMessage("test");
        var options = new ServiceBusMessageOptions
        {
            MessageId = "",
            CorrelationId = "   ",
            ContentType = "text/plain"
        };

        // Act
        InvokeConfigureMessageProperties(message, options);

        // Assert
        message.MessageId.ShouldNotBe("");
        message.CorrelationId.ShouldNotBe("   ");
        message.ContentType.ShouldBe("text/plain");
    }

    [Fact]
    public void ConfigureMessageProperties_WithScheduledDelivery_ShouldSetScheduledEnqueueTime()
    {
        // Arrange
        var message = new ServiceBusMessage("test");
        var delay = TimeSpan.FromMinutes(15);
        var options = new ServiceBusMessageOptions
        {
            ScheduledEnqueueTimeDelay = delay
        };
        var beforeTest = DateTimeOffset.UtcNow;

        // Act
        InvokeConfigureMessageProperties(message, options);

        // Assert
        var afterTest = DateTimeOffset.UtcNow;
        message.ScheduledEnqueueTime.ShouldBeInRange(
            beforeTest.Add(delay).AddSeconds(-1),
            afterTest.Add(delay).AddSeconds(1));
    }

    [Fact]
    public void ConfigureMessageProperties_WithLabelAndSubject_ShouldPreferSubject()
    {
        // Arrange
        var message = new ServiceBusMessage("test");
        var options = new ServiceBusMessageOptions
        {
            Label = "old-label",
            Subject = "new-subject"
        };

        // Act
        InvokeConfigureMessageProperties(message, options);

        // Assert
        message.Subject.ShouldBe("new-subject");
    }

    #endregion

    #region ConfigureBasicProperties Tests

    [Fact]
    public void ConfigureBasicProperties_WithAllBasicProperties_ShouldSetValues()
    {
        // Arrange
        var message = new ServiceBusMessage("test");
        var options = new ServiceBusMessageOptions
        {
            MessageId = "basic-msg-id",
            CorrelationId = "basic-corr-id",
            PartitionKey = "basic-partition", // Only PartitionKey, no SessionId
            ContentType = "text/xml"
        };

        // Act
        InvokeConfigureBasicProperties(message, options);

        // Assert
        message.MessageId.ShouldBe("basic-msg-id");
        message.CorrelationId.ShouldBe("basic-corr-id");
        message.PartitionKey.ShouldBe("basic-partition");
        message.ContentType.ShouldBe("text/xml");
    }

    [Fact]
    public void ConfigureBasicProperties_WithNullAndWhitespaceValues_ShouldIgnoreThem()
    {
        // Arrange
        var message = new ServiceBusMessage("test");
        var originalMessageId = message.MessageId;

        var options = new ServiceBusMessageOptions
        {
            MessageId = null,
            CorrelationId = "",
            PartitionKey = "   ",
            ContentType = "application/json"
        };

        // Act
        InvokeConfigureBasicProperties(message, options);

        // Assert
        message.MessageId.ShouldBe(originalMessageId); // Should remain unchanged
        message.CorrelationId.ShouldBeNull(); // Should remain unchanged
        message.PartitionKey.ShouldBeNull(); // Should remain unchanged
        message.ContentType.ShouldBe("application/json");
    }

    #endregion

    #region ConfigureAdvancedProperties Tests

    [Fact]
    public void ConfigureAdvancedProperties_WithAllAdvancedProperties_ShouldSetValues()
    {
        // Arrange
        var message = new ServiceBusMessage("test");
        var options = new ServiceBusMessageOptions
        {
            Subject = "advanced-subject",
            ReplyTo = "advanced-reply",
            ReplyToSessionId = "advanced-reply-session",
            To = "advanced-destination",
            TimeToLive = TimeSpan.FromHours(3)
        };

        // Act
        InvokeConfigureAdvancedProperties(message, options);

        // Assert
        message.Subject.ShouldBe("advanced-subject");
        message.ReplyTo.ShouldBe("advanced-reply");
        message.ReplyToSessionId.ShouldBe("advanced-reply-session");
        message.To.ShouldBe("advanced-destination");
        message.TimeToLive.ShouldBe(TimeSpan.FromHours(3));
    }

    [Fact]
    public void ConfigureAdvancedProperties_WithZeroTimeToLive_ShouldNotSetTimeToLive()
    {
        // Arrange
        var message = new ServiceBusMessage("test");
        var originalTtl = message.TimeToLive;

        var options = new ServiceBusMessageOptions
        {
            TimeToLive = TimeSpan.Zero
        };

        // Act
        InvokeConfigureAdvancedProperties(message, options);

        // Assert
        message.TimeToLive.ShouldBe(originalTtl); // Should remain unchanged
    }

    #endregion

    #region ConfigureApplicationProperties Tests

    [Fact]
    public void ConfigureApplicationProperties_WithMultipleProperties_ShouldAddAllProperties()
    {
        // Arrange
        var message = new ServiceBusMessage("test");
        var options = new ServiceBusMessageOptions
        {
            ApplicationProperties = new Dictionary<string, object>
            {
                { "StringProp", "test-string" },
                { "IntProp", 123 },
                { "DoubleProp", 45.67 },
                { "BoolProp", true },
                { "DateProp", DateTime.UtcNow },
                { "GuidProp", Guid.NewGuid() }
            }
        };

        // Act
        InvokeConfigureApplicationProperties(message, options);

        // Assert
        message.ApplicationProperties.Count.ShouldBe(6);
        message.ApplicationProperties["StringProp"].ShouldBe("test-string");
        message.ApplicationProperties["IntProp"].ShouldBe(123);
        message.ApplicationProperties["DoubleProp"].ShouldBe(45.67);
        message.ApplicationProperties["BoolProp"].ShouldBe(true);
        _ = message.ApplicationProperties["DateProp"].ShouldBeOfType<DateTime>();
        _ = message.ApplicationProperties["GuidProp"].ShouldBeOfType<Guid>();
    }

    [Fact]
    public void ConfigureApplicationProperties_WithEmptyDictionary_ShouldNotAddProperties()
    {
        // Arrange
        var message = new ServiceBusMessage("test");
        var options = new ServiceBusMessageOptions
        {
            ApplicationProperties = new Dictionary<string, object>()
        };

        // Act
        InvokeConfigureApplicationProperties(message, options);

        // Assert
        message.ApplicationProperties.Count.ShouldBe(0);
    }

    #endregion

    #region SetDefaultProperties Tests

    [Fact]
    public void SetDefaultProperties_ShouldSetJsonContentTypeAndTtl()
    {
        // Arrange
        var message = new ServiceBusMessage("test");

        // Act
        InvokeSetDefaultProperties(message);

        // Assert
        message.ContentType.ShouldBe("application/json");
        message.TimeToLive.ShouldBeGreaterThan(TimeSpan.Zero);
    }

    #endregion

    #region Helper Methods for Reflection

    private ServiceBusMessage InvokeCreateServiceBusMessage<T>(T message, ServiceBusMessageOptions? options)
    {
        var method = typeof(ServiceBusMessageSender).GetMethod("CreateServiceBusMessage",
            BindingFlags.NonPublic | BindingFlags.Instance);

        return (ServiceBusMessage)method!.MakeGenericMethod(typeof(T))
            .Invoke(_sender, new object?[] { message, options })!;
    }

    private void InvokeConfigureMessageProperties(ServiceBusMessage message, ServiceBusMessageOptions? options)
    {
        var method = typeof(ServiceBusMessageSender).GetMethod("ConfigureMessageProperties",
            BindingFlags.NonPublic | BindingFlags.Instance);

        _ = method!.Invoke(_sender, new object?[] { message, options });
    }

    private static void InvokeConfigureBasicProperties(ServiceBusMessage message, ServiceBusMessageOptions? options)
    {
        var method = typeof(ServiceBusMessageSender).GetMethod("ConfigureBasicProperties",
            BindingFlags.NonPublic | BindingFlags.Static);

        _ = method!.Invoke(null, new object?[] { message, options });
    }

    private static void InvokeConfigureAdvancedProperties(ServiceBusMessage message, ServiceBusMessageOptions? options)
    {
        var method = typeof(ServiceBusMessageSender).GetMethod("ConfigureAdvancedProperties",
            BindingFlags.NonPublic | BindingFlags.Static);

        _ = method!.Invoke(null, new object?[] { message, options });
    }

    private static void InvokeConfigureApplicationProperties(ServiceBusMessage message, ServiceBusMessageOptions? options)
    {
        var method = typeof(ServiceBusMessageSender).GetMethod("ConfigureApplicationProperties",
            BindingFlags.NonPublic | BindingFlags.Static);

        _ = method!.Invoke(null, new object?[] { message, options });
    }

    private void InvokeSetDefaultProperties(ServiceBusMessage message)
    {
        var method = typeof(ServiceBusMessageSender).GetMethod("SetDefaultProperties",
            BindingFlags.NonPublic | BindingFlags.Instance);

        _ = method!.Invoke(_sender, new object[] { message });
    }

    #endregion

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _sender?.Dispose();
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
