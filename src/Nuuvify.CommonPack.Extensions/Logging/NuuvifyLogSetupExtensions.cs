using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace Nuuvify.CommonPack.Logging;

public static class NuuvifyLogSetupExtensions
{
    //https://learn.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter
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

    public static ILoggingBuilder AddColorConsoleLogger(
        this ILoggingBuilder builder,
        Action<NuuvifyLogColorConfiguration> configureColor)
    {
        _ = builder.AddColorConsoleLogger();
        _ = builder.Services.Configure(configureColor);

        return builder;
    }

    public static ILoggingBuilder AddCustomFormatter(
        this ILoggingBuilder builder,
        Action<NuuvifyLogFormatterOptions> configureFormatter,
        Action<NuuvifyLogColorConfiguration> configureColor = default)
    {
        if (configureColor != null)
            _ = builder.Services.Configure(configureColor);

        _ = builder.AddConsole(options => options.FormatterName = nameof(NuuvifyLogFormatter))
               .AddConsoleFormatter<NuuvifyLogFormatter, NuuvifyLogFormatterOptions>(configureFormatter);

        return builder;
    }

}
