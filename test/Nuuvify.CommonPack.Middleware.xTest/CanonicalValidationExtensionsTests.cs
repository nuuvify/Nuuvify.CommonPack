using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nuuvify.CommonPack.Middleware.Setups;
using Xunit;

namespace Nuuvify.CommonPack.Middleware.xTest;

[Trait("Category", "Unit")]
public class CanonicalValidationExtensionsTests
{
    [Fact]
    public void AddCanonicalValidation_ConfiguresBadRequestValidationProblemDetails()
    {
        var services = new ServiceCollection();
        services.AddCanonicalValidation();
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ApiBehaviorOptions>>().Value;
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor(),
            new ModelStateDictionary());
        actionContext.ModelState.AddModelError("Name", "Name is required.");

        var result = options.InvalidModelStateResponseFactory(actionContext);
        var objectResult = Assert.IsType<BadRequestObjectResult>(result);
        var problemDetails = Assert.IsType<ValidationProblemDetails>(objectResult.Value);

        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
        Assert.Equal("Name is required.", problemDetails.Errors["Name"].Single());
    }

    [Fact]
    public void AddCanonicalValidation_OnExistingMvcBuilder_ConfiguresValidationFactory()
    {
        var services = new ServiceCollection();
        _ = services.AddControllers().AddCanonicalValidation();
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<ApiBehaviorOptions>>().Value;

        Assert.NotNull(options.InvalidModelStateResponseFactory);
    }
}