using System.Reflection;
using Shouldly;
using Xunit;

namespace Nuuvify.CommonPack.HealthCheck.xTest;

[Trait("Category", "Unit")]
public sealed class HealthCheckAssemblyExtensionTests
{
    [Fact]
    public void AssemblyMetadataProperties_ShouldReturnApplicationValues()
    {
        var assembly = typeof(MemoryHealthCheck).Assembly;
        var extensionType = assembly.GetType("Nuuvify.CommonPack.HealthCheck.Helpers.AssemblyExtension");

        extensionType.ShouldNotBeNull();
        var applicationName = extensionType!.GetProperty("GetApplicationNameByAssembly")!.GetValue(null);
        var buildNumber = extensionType.GetProperty("GetApplicationBuildNumber")!.GetValue(null);
        var applicationVersion = extensionType.GetProperty("GetApplicationVersion")!.GetValue(null);

        applicationName.ShouldNotBeNull();
        buildNumber.ShouldNotBeNull();
        applicationVersion.ShouldNotBeNull();
    }
}
