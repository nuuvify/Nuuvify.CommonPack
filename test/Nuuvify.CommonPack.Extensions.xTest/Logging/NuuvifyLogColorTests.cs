using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.Logging;
using Xunit;

namespace Nuuvify.CommonPack.Extensions.xTest.Logging;

[Trait("Category", "Unit")]
public class NuuvifyLogColorTests
{
    private static NuuvifyLogColor Build(NuuvifyLogColorConfiguration config, string name = "TestLogger")
        => new(name, () => config);

    [Fact]
    [Trait("CommonApi.Extensions-Logging", nameof(NuuvifyLogColor))]
    public void IsEnabled_LogLevelNoMapa_RetornaTrue()
    {
        var logger = Build(new NuuvifyLogColorConfiguration());
        Assert.True(logger.IsEnabled(LogLevel.Information));
    }

    [Fact]
    [Trait("CommonApi.Extensions-Logging", nameof(NuuvifyLogColor))]
    public void IsEnabled_LogLevelNoneNaoNoMapa_RetornaFalse()
    {
        var logger = Build(new NuuvifyLogColorConfiguration());
        Assert.False(logger.IsEnabled(LogLevel.None));
    }

    [Fact]
    [Trait("CommonApi.Extensions-Logging", nameof(NuuvifyLogColor))]
    public void BeginScope_RetornaDefault()
    {
        var logger = Build(new NuuvifyLogColorConfiguration());
        var scope = logger.BeginScope("algum estado");
        Assert.Null(scope);
    }

    [Fact]
    [Trait("CommonApi.Extensions-Logging", nameof(NuuvifyLogColor))]
    public void Log_NivelDesativado_NaoEscreve()
    {
        var config = new NuuvifyLogColorConfiguration
        {
            LogLevelToColorMap = new System.Collections.Generic.Dictionary<LogLevel, ConsoleColor>
            {
                [LogLevel.Error] = ConsoleColor.Red
            }
        };
        var logger = Build(config);

        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            logger.Log(LogLevel.Information, new EventId(1), "estado", null, (s, ex) => s);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        Assert.Empty(writer.ToString());
    }

    [Fact]
    [Trait("CommonApi.Extensions-Logging", nameof(NuuvifyLogColor))]
    public void Log_NivelAtivado_EscreveLogLevel()
    {
        var logger = Build(new NuuvifyLogColorConfiguration(), "MeuServico");

        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            logger.Log(LogLevel.Warning, new EventId(0), "mensagem de aviso", null, (s, ex) => s);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        var output = writer.ToString();
        Assert.Contains("Warning", output);
        Assert.Contains("mensagem de aviso", output);
    }
}
