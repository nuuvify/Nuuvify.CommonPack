using System;
using System.IO;
using Nuuvify.CommonPack.Logging;
using Xunit;

namespace Nuuvify.CommonPack.Extensions.xTest.Logging;

[Trait("Category", "Unit")]
public class TextWriterExtensionsTests
{
    [Fact]
    [Trait("CommonApi.Extensions-Logging", nameof(TextWriterExtensions))]
    public void WriteWithColor_SemCores_EscreveAMensagem()
    {
        using var writer = new StringWriter();
        writer.WriteWithColor("teste sem cores", background: null, foreground: null);
        Assert.Contains("teste sem cores", writer.ToString());
    }

    [Fact]
    [Trait("CommonApi.Extensions-Logging", nameof(TextWriterExtensions))]
    public void WriteWithColor_ComBackground_EscreveAMensagem()
    {
        using var writer = new StringWriter();
        writer.WriteWithColor("com background", ConsoleColor.Blue, foreground: null);
        Assert.Contains("com background", writer.ToString());
    }

    [Fact]
    [Trait("CommonApi.Extensions-Logging", nameof(TextWriterExtensions))]
    public void WriteWithColor_ComForeground_EscreveAMensagem()
    {
        using var writer = new StringWriter();
        writer.WriteWithColor("com foreground", background: null, ConsoleColor.Green);
        Assert.Contains("com foreground", writer.ToString());
    }

    [Fact]
    [Trait("CommonApi.Extensions-Logging", nameof(TextWriterExtensions))]
    public void WriteWithColor_ComAmbasCores_EscreveAMensagem()
    {
        using var writer = new StringWriter();
        writer.WriteWithColor("com ambas", ConsoleColor.Black, ConsoleColor.White);
        Assert.Contains("com ambas", writer.ToString());
    }
}
