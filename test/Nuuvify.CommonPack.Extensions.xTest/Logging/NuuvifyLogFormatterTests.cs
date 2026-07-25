using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Nuuvify.CommonPack.Logging;
using Xunit;

namespace Nuuvify.CommonPack.Extensions.xTest.Logging;

[Trait("Category", "Unit")]
public class NuuvifyLogFormatterTests
{
    [Fact]
    [Trait("CommonApi.Extensions-Logging", nameof(NuuvifyLogFormatter))]
    public void Dispose_ShouldDisposeBothReloadTokens()
    {
        using var formatterMonitor = new TestOptionsMonitor<NuuvifyLogFormatterOptions>(new NuuvifyLogFormatterOptions(), out var formatterToken);
        using var colorMonitor = new TestOptionsMonitor<NuuvifyLogColorConfiguration>(new NuuvifyLogColorConfiguration(), out var colorToken);

        using var formatter = new NuuvifyLogFormatter(formatterMonitor, colorMonitor);

        formatter.Dispose();

        Assert.Equal(1, formatterToken.DisposeCount);
        Assert.Equal(1, colorToken.DisposeCount);
    }

    [Fact]
    [Trait("CommonApi.Extensions-Logging", nameof(NuuvifyLogSetupExtensions))]
    public void AddCustomFormatter_WithConsoleConfiguration_ShouldApplyConsoleOptions()
    {
        var services = new ServiceCollection();

        _ = services.AddLogging(builder =>
        {
            builder.AddCustomFormatter(
                formatter =>
                {
                    formatter.TimestampFormat = "yyyy-MM-dd HH:mm:ss";
                    formatter.CustomPrefix = "TEST";
                },
                console => console.LogToStandardErrorThreshold = LogLevel.Error);
        });

        using var serviceProvider = services.BuildServiceProvider();
        var consoleOptions = serviceProvider.GetRequiredService<IOptionsMonitor<ConsoleLoggerOptions>>().CurrentValue;
        var formatterOptions = serviceProvider.GetRequiredService<IOptionsMonitor<NuuvifyLogFormatterOptions>>().CurrentValue;

        Assert.Equal(nameof(NuuvifyLogFormatter), consoleOptions.FormatterName);
        Assert.Equal(LogLevel.Error, consoleOptions.LogToStandardErrorThreshold);
        Assert.Equal("TEST", formatterOptions.CustomPrefix);
        Assert.Equal("yyyy-MM-dd HH:mm:ss", formatterOptions.TimestampFormat);
    }

    private sealed class TestOptionsMonitor<T> : IOptionsMonitor<T>, IDisposable
    {
        private readonly T _currentValue;
        private readonly TrackingDisposable _token;

        public TestOptionsMonitor(T currentValue, out TrackingDisposable token)
        {
            _currentValue = currentValue;
            _token = new TrackingDisposable();
            token = _token;
        }

        public T CurrentValue => _currentValue;

        public T Get(string name)
        {
            return _currentValue;
        }

        public IDisposable OnChange(Action<T, string> listener)
        {
            return _token;
        }

        public void Dispose()
        {
            _token.Dispose();
        }
    }

    private sealed class TrackingDisposable : IDisposable
    {
        public int DisposeCount { get; private set; }

        public void Dispose()
        {
            DisposeCount++;
        }
    }
}