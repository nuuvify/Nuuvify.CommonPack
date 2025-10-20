using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Nuuvify.CommonPack.Logging;

public class NuuvifyLogFormatter : ConsoleFormatter, IDisposable
{

    private readonly IDisposable _optionsReloadToken;
    private NuuvifyLogFormatterOptions _nuuvifyLogOptions;
    private NuuvifyLogColorConfiguration _nuuvifyLogColorConfiguration;

    public NuuvifyLogFormatter(
        IOptionsMonitor<NuuvifyLogFormatterOptions> nuuvifyLogOptions,
        IOptionsMonitor<NuuvifyLogColorConfiguration> nuuvifyLogColorConfiguration)
        : base(nameof(NuuvifyLogFormatter))
    {

        _optionsReloadToken = nuuvifyLogOptions.OnChange(ReloadLoggerOptions);
        _nuuvifyLogOptions = nuuvifyLogOptions.CurrentValue;

        _optionsReloadToken = nuuvifyLogColorConfiguration.OnChange(ReloadLoggerColorConfiguration);
        _nuuvifyLogColorConfiguration = nuuvifyLogColorConfiguration.CurrentValue;

    }

    private void ReloadLoggerOptions(NuuvifyLogFormatterOptions options) =>
        _nuuvifyLogOptions = options;

    private void ReloadLoggerColorConfiguration(NuuvifyLogColorConfiguration config) =>
        _nuuvifyLogColorConfiguration = config;

    public virtual void Write<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        string message,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter,
        string name)
    {

        if (string.IsNullOrWhiteSpace(message)) return;

        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.White;
        textWriter = Console.Out;
        WritePrefix(textWriter);

        var messageLogLevel = $"[ {logLevel,-12} ]";
        Console.ForegroundColor = _nuuvifyLogColorConfiguration.LogLevelToColorMap[logLevel];
        textWriter = Console.Out;
        textWriter.WriteLine(messageLogLevel);

        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.White;
        textWriter = Console.Out;
        textWriter.WriteLine($"{name.PadLeft(name.Length + 5)}");
        textWriter.WriteLine($"{message.PadLeft(message.Length + 5)}");

    }

    public override void Write<TState>(in
        LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter)
    {

        Write<LogEntry<TState>>(
            logEntry.LogLevel,
            logEntry.EventId,
            logEntry,
            logEntry.Exception,
            logEntry.Formatter(logEntry.State, logEntry.Exception),
            scopeProvider,
            textWriter,
            logEntry.Category);

    }

    private void WritePrefix(TextWriter textWriter)
    {
        DateTimeOffset now = _nuuvifyLogOptions.UseUtcTimestamp
            ? DateTimeOffset.UtcNow
            : DateTimeOffset.Now;

        var nowMessage = now.ToString(_nuuvifyLogOptions.TimestampFormat);

        textWriter.Write($"{_nuuvifyLogOptions.CustomPrefix} {nowMessage} ");

    }

    private bool disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                _optionsReloadToken?.Dispose();
            }

            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // public override void Write<TState>(in
    //     LogEntry<TState> logEntry,
    //     IExternalScopeProvider scopeProvider,
    //     TextWriter textWriter)
    // {

    //     var simpleConsoleFormatterOptions = new SimpleConsoleFormatterOptions();
    //     simpleConsoleFormatterOptions.TimestampFormat = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz");

    //     var timestamp = DateTimeOffset.Now.ToString(simpleConsoleFormatterOptions.TimestampFormat);
    //     var logLevel = logEntry.LogLevel.ToString();
    //     var message = logEntry.Formatter(logEntry.State, logEntry.Exception);

    //     var color = logEntry.LogLevel switch
    //     {
    //         LogLevel.Trace => ConsoleColor.Gray,
    //         LogLevel.Debug => ConsoleColor.Blue,
    //         LogLevel.Information => ConsoleColor.Green,
    //         LogLevel.Warning => ConsoleColor.Yellow,
    //         LogLevel.Error => ConsoleColor.Red,
    //         LogLevel.Critical => ConsoleColor.Magenta,
    //         _ => ConsoleColor.White,
    //     };

    //     var logMessage = $"[{timestamp}] {logLevel}: {message}";

    //     System.Console.ForegroundColor = color;
    //     textWriter = System.Console.Out;
    //     textWriter.WriteLine(logMessage);
    //     System.Console.ResetColor();

    //}

}
