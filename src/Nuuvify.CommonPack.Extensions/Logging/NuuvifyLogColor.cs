
using Microsoft.Extensions.Logging;

namespace Nuuvify.CommonPack.Logging;

public sealed class NuuvifyLogColor(
    string name,
    Func<NuuvifyLogColorConfiguration> getCurrentConfig) : ILogger
{

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) =>
        getCurrentConfig().LogLevelToColorMap.ContainsKey(logLevel);



    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;


        NuuvifyLogColorConfiguration config = getCurrentConfig();


        if (config.EventId == 0 || config.EventId == eventId.Id)
        {

            // using (BeginScope<TState>(state))
            // {

            var message = formatter(state, exception);

            // nuuvifyLogFormatter().Write<TState>(
            //         logLevel,
            //         eventId,
            //         state,
            //         exception,
            //         message,
            //         null,
            //         Console.Out,
            //         name);


            var messageLogLevel = $"[ {logLevel,-12} ]";
            Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
            Console.WriteLine(messageLogLevel);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{name.PadLeft(name.Length + 5)}");
            Console.WriteLine($"{message.PadLeft(message.Length + 5)}");


            Console.ResetColor();
            Console.WriteLine();
            // }



        }


    }
}
