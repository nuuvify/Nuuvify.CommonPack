using Microsoft.Extensions.Logging.Console;

namespace Nuuvify.CommonPack.Logging;

public sealed class NuuvifyLogFormatterOptions : ConsoleFormatterOptions
{
    public string CustomPrefix { get; set; }

}
