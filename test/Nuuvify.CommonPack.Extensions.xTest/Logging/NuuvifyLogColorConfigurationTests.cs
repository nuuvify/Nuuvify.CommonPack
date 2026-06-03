using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.Logging;
using Xunit;

namespace Nuuvify.CommonPack.Extensions.xTest.Logging;

[Trait("Category", "Unit")]
public class NuuvifyLogColorConfigurationTests
{
    [Fact]
    [Trait("CommonApi.Extensions-Logging", nameof(NuuvifyLogColorConfiguration))]
    public void NuuvifyLogColorConfiguration_EventId_PadraoEhZero()
    {
        var config = new NuuvifyLogColorConfiguration();
        Assert.Equal(0, config.EventId);
    }

    [Fact]
    [Trait("CommonApi.Extensions-Logging", nameof(NuuvifyLogColorConfiguration))]
    public void NuuvifyLogColorConfiguration_LogLevelToColorMap_ContemTodasAsentradas()
    {
        var config = new NuuvifyLogColorConfiguration();
        Assert.Contains(LogLevel.Trace, config.LogLevelToColorMap);
        Assert.Contains(LogLevel.Debug, config.LogLevelToColorMap);
        Assert.Contains(LogLevel.Information, config.LogLevelToColorMap);
        Assert.Contains(LogLevel.Warning, config.LogLevelToColorMap);
        Assert.Contains(LogLevel.Error, config.LogLevelToColorMap);
        Assert.Contains(LogLevel.Critical, config.LogLevelToColorMap);
    }

    [Fact]
    [Trait("CommonApi.Extensions-Logging", nameof(NuuvifyLogColorConfiguration))]
    public void NuuvifyLogColorConfiguration_LogLevelToColorMap_PodeSerSubstituido()
    {
        var config = new NuuvifyLogColorConfiguration
        {
            LogLevelToColorMap = new Dictionary<LogLevel, ConsoleColor>
            {
                [LogLevel.Information] = ConsoleColor.Cyan
            }
        };
        Assert.Equal(ConsoleColor.Cyan, config.LogLevelToColorMap[LogLevel.Information]);
        Assert.DoesNotContain(LogLevel.Error, config.LogLevelToColorMap);
    }
}
