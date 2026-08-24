using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nuuvify.CommonPack.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace Nuuvify.CommonPack.OpenApi.xTest;

[Trait("Category", "Unit")]
public class SwaggerGenSecurityTests
{
    [Fact]
    public void ConfigurationApiKey_RegistersSwaggerServices()
    {
        var services = new ServiceCollection();

        services.ConfigurationApiKey("X-Test-Key");

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

        Assert.Contains("ApiKey", options.SwaggerGeneratorOptions.SecuritySchemes.Keys);
    }

    [Fact]
    public void Configuration_RegistersSwaggerServices()
    {
        var services = new ServiceCollection();

        services.Configuration();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

        Assert.Contains("Bearer", options.SwaggerGeneratorOptions.SecuritySchemes.Keys);
    }
}
