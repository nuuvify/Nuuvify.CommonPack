using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.Console;

namespace Nuuvify.CommonPack.Logging;

/// <summary>
/// Fornece extensões para registrar os componentes de logging customizado do pacote.
/// </summary>
/// <remarks>
/// Os métodos desta classe registram o formatter <see cref="NuuvifyLogFormatter"/> e, opcionalmente,
/// a configuração de cores e opções do provider de console padrão do .NET.
/// </remarks>
public static class NuuvifyLogSetupExtensions
{
    //https://learn.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter
    /// <summary>
    /// Registra o provider de console colorido do pacote.
    /// </summary>
    /// <param name="builder">Builder de logging que receberá o provider.</param>
    /// <returns>O mesmo <see cref="ILoggingBuilder"/> para encadeamento.</returns>
    public static ILoggingBuilder AddColorConsoleLogger(
        this ILoggingBuilder builder)
    {
        builder.AddConfiguration();

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, NuuvifyLogColorProvider>());

        LoggerProviderOptions.RegisterProviderOptions
            <NuuvifyLogColorConfiguration, NuuvifyLogColorProvider>(builder.Services);

        return builder;
    }

    /// <summary>
    /// Registra o provider de console colorido do pacote com customização adicional de cores.
    /// </summary>
    /// <param name="builder">Builder de logging que receberá o provider.</param>
    /// <param name="configureColor">Ação para customizar o mapeamento de cores por nível de log.</param>
    /// <returns>O mesmo <see cref="ILoggingBuilder"/> para encadeamento.</returns>
    public static ILoggingBuilder AddColorConsoleLogger(
        this ILoggingBuilder builder,
        Action<NuuvifyLogColorConfiguration> configureColor)
    {
        _ = builder.AddColorConsoleLogger();
        _ = builder.Services.Configure(configureColor);

        return builder;
    }

    /// <summary>
    /// Registra o formatter customizado do pacote usando o provider de console padrão do .NET.
    /// </summary>
    /// <param name="builder">Builder de logging que receberá o formatter.</param>
    /// <param name="configureFormatter">Ação para customizar as opções do formatter.</param>
    /// <param name="configureColor">Ação opcional para customizar o mapeamento de cores por nível de log.</param>
    /// <returns>O mesmo <see cref="ILoggingBuilder"/> para encadeamento.</returns>
    public static ILoggingBuilder AddCustomFormatter(
        this ILoggingBuilder builder,
        Action<NuuvifyLogFormatterOptions> configureFormatter,
        Action<NuuvifyLogColorConfiguration> configureColor = default)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configureFormatter is null)
        {
            throw new ArgumentNullException(nameof(configureFormatter));
        }

        return builder.AddCustomFormatter(configureFormatter, configureConsole: null, configureColor);
    }

    /// <summary>
    /// Registra o formatter customizado do pacote e permite customizar as opções do provider de console padrão.
    /// </summary>
    /// <param name="builder">Builder de logging que receberá o formatter.</param>
    /// <param name="configureFormatter">Ação para customizar as opções do formatter.</param>
    /// <param name="configureConsole">Ação opcional para customizar o provider de console padrão do .NET.</param>
    /// <param name="configureColor">Ação opcional para customizar o mapeamento de cores por nível de log.</param>
    /// <returns>O mesmo <see cref="ILoggingBuilder"/> para encadeamento.</returns>
    /// <remarks>
    /// Esse overload é útil quando o consumidor precisa preservar o formatter do pacote, mas controlar
    /// detalhes do provider de console como o limiar de saída em <c>stderr</c> durante o bootstrap.
    /// </remarks>
    public static ILoggingBuilder AddCustomFormatter(
        this ILoggingBuilder builder,
        Action<NuuvifyLogFormatterOptions> configureFormatter,
        Action<ConsoleLoggerOptions> configureConsole,
        Action<NuuvifyLogColorConfiguration> configureColor = default)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configureFormatter is null)
        {
            throw new ArgumentNullException(nameof(configureFormatter));
        }

        if (configureColor != null)
            _ = builder.Services.Configure(configureColor);

        _ = builder.AddConsole(options =>
        {
            options.FormatterName = nameof(NuuvifyLogFormatter);
            configureConsole?.Invoke(options);
        })
               .AddConsoleFormatter<NuuvifyLogFormatter, NuuvifyLogFormatterOptions>(configureFormatter);

        return builder;
    }

    /// <summary>
    /// Mantém compatibilidade com versões anteriores que registravam o formatter via AddNuuvifyConsoleFormatter.
    /// </summary>
    /// <param name="builder">Builder de logging que receberá o formatter.</param>
    /// <param name="configureFormatter">Ação para customizar as opções do formatter.</param>
    /// <param name="configureColor">Ação opcional para customizar o mapeamento de cores por nível de log.</param>
    /// <returns>O mesmo <see cref="ILoggingBuilder"/> para encadeamento.</returns>
    public static ILoggingBuilder AddNuuvifyConsoleFormatter(
        this ILoggingBuilder builder,
        Action<NuuvifyLogFormatterOptions> configureFormatter,
        Action<NuuvifyLogColorConfiguration> configureColor = default)
    {
        return builder.AddCustomFormatter(configureFormatter, configureColor);
    }

    /// <summary>
    /// Mantém compatibilidade com versões anteriores que registravam o formatter via AddNuuvifyConsoleFormatter.
    /// </summary>
    /// <param name="builder">Builder de logging que receberá o formatter.</param>
    /// <param name="configureFormatter">Ação para customizar as opções do formatter.</param>
    /// <param name="configureConsole">Ação opcional para customizar o provider de console padrão do .NET.</param>
    /// <param name="configureColor">Ação opcional para customizar o mapeamento de cores por nível de log.</param>
    /// <returns>O mesmo <see cref="ILoggingBuilder"/> para encadeamento.</returns>
    public static ILoggingBuilder AddNuuvifyConsoleFormatter(
        this ILoggingBuilder builder,
        Action<NuuvifyLogFormatterOptions> configureFormatter,
        Action<ConsoleLoggerOptions> configureConsole,
        Action<NuuvifyLogColorConfiguration> configureColor = default)
    {
        return builder.AddCustomFormatter(configureFormatter, configureConsole, configureColor);
    }

}
