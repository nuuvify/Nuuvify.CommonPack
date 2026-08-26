using Microsoft.Extensions.Hosting;
using Xunit;

namespace Nuuvify.CommonPack.Middleware.xTest;

[Trait("Category", "Unit")]
public class HostApplicationBuilderExtensionsTests
{
    [Fact]
    public void GetContainerSecretsPath_UsesCustomPathForCurrentPlatform()
    {
        var builder = Host.CreateApplicationBuilder();
        var customPath = OperatingSystem.IsWindows() ? "C:\\secrets" : "/tmp/secrets";

        var result = builder.GetContainerSecretsPath(
            customPathWindows: customPath,
            customPathLinux: customPath);

        Assert.Equal(customPath, result);
    }
}