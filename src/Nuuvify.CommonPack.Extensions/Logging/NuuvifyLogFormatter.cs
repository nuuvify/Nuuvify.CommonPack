#nullable enable

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Nuuvify.CommonPack.Logging;

/// <summary>
/// Formata logs de console com prefixo configurável e cores por nível de severidade.
/// </summary>
/// <remarks>
/// A instância observa alterações dinâmicas em <see cref="NuuvifyLogFormatterOptions"/> e
/// <see cref="NuuvifyLogColorConfiguration"/> por meio de <see cref="IOptionsMonitor{TOptions}"/>.
/// </remarks>
public class NuuvifyLogFormatter : ConsoleFormatter, IDisposable
{

    private readonly IDisposable? _formatterOptionsReloadToken;
    private readonly IDisposable? _colorOptionsReloadToken;
    private NuuvifyLogFormatterOptions _nuuvifyLogOptions;
    private NuuvifyLogColorConfiguration _nuuvifyLogColorConfiguration;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="NuuvifyLogFormatter"/>.
    /// </summary>
    /// <param name="nuuvifyLogOptions">Monitor das opções de formatação do logger.</param>
    /// <param name="nuuvifyLogColorConfiguration">Monitor da configuração de cores por nível de log.</param>
    public NuuvifyLogFormatter(
        IOptionsMonitor<NuuvifyLogFormatterOptions> nuuvifyLogOptions,
        IOptionsMonitor<NuuvifyLogColorConfiguration> nuuvifyLogColorConfiguration)
        : base(nameof(NuuvifyLogFormatter))
    {
        if (nuuvifyLogOptions is null)
        {
            throw new ArgumentNullException(nameof(nuuvifyLogOptions));
        }

        if (nuuvifyLogColorConfiguration is null)
        {
            throw new ArgumentNullException(nameof(nuuvifyLogColorConfiguration));
        }

        _formatterOptionsReloadToken = nuuvifyLogOptions.OnChange(ReloadLoggerOptions);
        _nuuvifyLogOptions = nuuvifyLogOptions.CurrentValue ?? new NuuvifyLogFormatterOptions();

        _colorOptionsReloadToken = nuuvifyLogColorConfiguration.OnChange(ReloadLoggerColorConfiguration);
        _nuuvifyLogColorConfiguration = nuuvifyLogColorConfiguration.CurrentValue ?? new NuuvifyLogColorConfiguration();

    }

    private void ReloadLoggerOptions(NuuvifyLogFormatterOptions options) =>
        _nuuvifyLogOptions = options ?? new NuuvifyLogFormatterOptions();

    private void ReloadLoggerColorConfiguration(NuuvifyLogColorConfiguration config) =>
        _nuuvifyLogColorConfiguration = config ?? new NuuvifyLogColorConfiguration();

    /// <summary>
    /// Escreve uma entrada de log formatada com o prefixo e as cores configuradas.
    /// </summary>
    /// <typeparam name="TState">Tipo do estado associado ao log.</typeparam>
    /// <param name="logLevel">Nível do log.</param>
    /// <param name="eventId">Identificador do evento.</param>
    /// <param name="state">Estado associado à entrada de log.</param>
    /// <param name="exception">Exceção opcional associada ao log. Pode ser <see langword="null"/>.</param>
    /// <param name="message">Mensagem já formatada a ser escrita.</param>
    /// <param name="scopeProvider">Provider de escopo ativo. Pode ser <see langword="null"/>.</param>
    /// <param name="textWriter">Destino do texto formatado.</param>
    /// <param name="name">Categoria ou nome do logger.</param>
    public virtual void Write<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
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

    /// <inheritdoc />
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
                _formatterOptionsReloadToken?.Dispose();
                _colorOptionsReloadToken?.Dispose();
            }

            disposed = true;
        }
    }

    /// <summary>
    /// Libera os monitors de opções observados por esta instância.
    /// </summary>
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
