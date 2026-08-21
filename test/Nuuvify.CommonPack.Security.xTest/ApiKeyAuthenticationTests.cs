using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Nuuvify.CommonPack.Security;
using Xunit;

namespace Nuuvify.CommonPack.Security.xTest;

[Trait("Category", "Unit")]
public class ApiKeyAuthenticationTests
{
    [Fact]
    public async Task AuthenticateAsync_WhenHeaderMatchesConfiguredKey_ReturnsSuccess()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthentication(ApiKeyAuthenticationDefaults.AuthenticationScheme)
            .AddApiKeyAuthentication(options =>
            {
                options.HeaderName = "X-API-Key";
                options.ValidKeys = new[] { "abc123" };
            });

        var provider = services.BuildServiceProvider();
        var context = new DefaultHttpContext
        {
            RequestServices = provider,
        };
        context.Request.Headers["X-API-Key"] = "abc123";

        var authenticationService = provider.GetRequiredService<IAuthenticationService>();
        var result = await authenticationService.AuthenticateAsync(context, ApiKeyAuthenticationDefaults.AuthenticationScheme);

        Assert.True(result.Succeeded);
        Assert.Equal("X-API-Key", result.Principal?.FindFirst(ApiKeyAuthenticationDefaults.ClaimType)?.Value);
        Assert.DoesNotContain("abc123", result.Principal?.Claims.Select(claim => claim.Value) ?? Array.Empty<string>());
    }

    [Fact]
    public async Task AuthenticateAsync_WhenHeaderDoesNotMatchConfiguredKey_Fails()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthentication(ApiKeyAuthenticationDefaults.AuthenticationScheme)
            .AddApiKeyAuthentication(options =>
            {
                options.HeaderName = "X-API-Key";
                options.ValidKeys = new[] { "abc123" };
            });

        var provider = services.BuildServiceProvider();
        var context = new DefaultHttpContext
        {
            RequestServices = provider,
        };
        context.Request.Headers["X-API-Key"] = "wrong-key";

        var authenticationService = provider.GetRequiredService<IAuthenticationService>();
        var result = await authenticationService.AuthenticateAsync(context, ApiKeyAuthenticationDefaults.AuthenticationScheme);

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Failure);
        Assert.Contains("Invalid API key", result.Failure.Message);
    }
}
