
using Microsoft.Extensions.Logging;

namespace Nuuvify.CommonPack.Logging;

public sealed class NuuvifyLogColorConfiguration
{
    public int EventId { get; set; }

    public Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap { get; set; } = new()
    {
        [LogLevel.Trace] = ConsoleColor.Gray,
        [LogLevel.Debug] = ConsoleColor.Blue,
        [LogLevel.Information] = ConsoleColor.DarkGreen,
        [LogLevel.Warning] = ConsoleColor.Yellow,
        [LogLevel.Error] = ConsoleColor.Red,
        [LogLevel.Critical] = ConsoleColor.Magenta,

    };
}
