using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Nuuvify.CommonPack.Logging;

// [UnsupportedOSPlatform("browser")]
[ProviderAlias("ColorConsole")]
public sealed class NuuvifyLogColorProvider : ILoggerProvider
{
    private readonly IDisposable _onChangeToken;
    private NuuvifyLogColorConfiguration _currentConfig;
    private readonly ConcurrentDictionary<string, NuuvifyLogColor> _loggers =
        new(StringComparer.OrdinalIgnoreCase);

    public NuuvifyLogColorProvider(
        IOptionsMonitor<NuuvifyLogColorConfiguration> config)
    {
        _currentConfig = config.CurrentValue;
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);

    }

    public ILogger CreateLogger(string categoryName)
    {
        _ = _loggers.GetOrAdd(categoryName, name =>
            new NuuvifyLogColor(
                name,
                GetCurrentConfig));

        return _loggers[categoryName];
    }

    private NuuvifyLogColorConfiguration GetCurrentConfig() => _currentConfig;

    public void Dispose()
    {
        _loggers.Clear();
        _onChangeToken?.Dispose();
    }

}
