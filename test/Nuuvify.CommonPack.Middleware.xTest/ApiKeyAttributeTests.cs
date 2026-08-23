using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Nuuvify.CommonPack.Middleware.Filters;
using Nuuvify.CommonPack.Security;
using Xunit;

namespace Nuuvify.CommonPack.Middleware.xTest;

[Trait("Category", "Unit")]
public class ApiKeyAttributeTests
{

    private ActionContext _actionContext;
    private ActionExecutingContext _actionExecutingContext;
    private ResourceExecutingContext _resourceExecutingContext;
    private DefaultHttpContext _defaultHttpContext;

    private void ArrangeResourceExecutingContextTests()
    {

        _actionContext = new ActionContext()
        {
            HttpContext = new DefaultHttpContext(),
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor()
        };

        _resourceExecutingContext = new ResourceExecutingContext(
            _actionContext,
            new List<IFilterMetadata>(),
            new List<IValueProviderFactory>()
            );

    }

    private void ArrangeActionExecutingContextTests(DefaultHttpContext defaultHttpContext)
    {

        _defaultHttpContext = defaultHttpContext ?? new DefaultHttpContext();

        _actionContext = new ActionContext()
        {
            HttpContext = _defaultHttpContext,
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor()
        };

        _actionExecutingContext = new ActionExecutingContext(
            _actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object>(),
            new object()
            );

    }

    [Fact]
    public void OnResourceExecuting_ShoultAddASingleLogIfExecuted()
    {

        string[] keyNameTest = new[] { "MyKeyXyz" };
        var loggerMock = new Mock<ILogger<ApiKeyFilter>>();

        var mockIConfiguration = new Mock<IConfiguration>();

        _ = mockIConfiguration.Setup(s => s.GetSection(keyNameTest[0]).Value)
            .Returns("xxx1234");

        ArrangeResourceExecutingContextTests();

        var apiKeyFilter = new ApiKeyFilter(
            loggerMock.Object,
            mockIConfiguration.Object,
            keyNameTest);

        apiKeyFilter.OnResourceExecuting(_resourceExecutingContext);

        loggerMock.Verify(
            m => m.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("IResourceFilter.OnResourceExecuting")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            )
        );

    }

    [Fact]
    public void OnActionExecuting_ShoultReturn401IfKeyValueNotFoundInHttpHeader()
    {

        string[] keyNameTest = new[] { "MyKeyXyz" };
        string keyValueTest = "xxx1234";
        var loggerMock = new Mock<ILogger<ApiKeyFilter>>();

        var mockIConfiguration = new Mock<IConfiguration>();

        _ = mockIConfiguration.Setup(s => s.GetSection(keyNameTest[0]).Value)
            .Returns(keyValueTest);

        ArrangeActionExecutingContextTests(new DefaultHttpContext());

        var apiKeyFilter = new ApiKeyFilter(
            loggerMock.Object,
            mockIConfiguration.Object,
            keyNameTest);

        apiKeyFilter.OnActionExecuting(_actionExecutingContext);

        var contentResult = (ContentResult)_actionExecutingContext.Result;

        Assert.Equal(expected: 401, actual: contentResult.StatusCode);

    }

    [Fact]
    public void OnActionExecuting_ShoultReturn401IfKeyValueNotMatchInHttpHeader()
    {

        string[] keyNameTest = new[] { "MyKeyXyz" };
        string keyValueTest = "xxx1234";
        var loggerMock = new Mock<ILogger<ApiKeyFilter>>();

        var mockIConfiguration = new Mock<IConfiguration>();

        _ = mockIConfiguration.Setup(s => s.GetSection(keyNameTest[0]).Value)
            .Returns(keyValueTest);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append("TesteHeader", "1234567");
        httpContext.Request.Headers.Append(keyNameTest[0], "outrovalor");

        ArrangeActionExecutingContextTests(httpContext);

        var apiKeyFilter = new ApiKeyFilter(
            loggerMock.Object,
            mockIConfiguration.Object,
            keyNameTest);

        apiKeyFilter.OnActionExecuting(_actionExecutingContext);

        var contentResult = (ContentResult)_actionExecutingContext.Result;

        Assert.Equal(expected: 401, actual: contentResult.StatusCode);

    }

    [Fact]
    public void OnActionExecuting_ShoultReturn200IfKeyValueMatchInHttpHeader()
    {

        string[] keyNameTest = new[] { "MyKeyXyz" };
        string keyValueTest = "xxx1234";
        var loggerMock = new Mock<ILogger<ApiKeyFilter>>();

        var mockIConfiguration = new Mock<IConfiguration>();

        _ = mockIConfiguration.Setup(s => s.GetSection(keyNameTest[0]).Value)
            .Returns(keyValueTest);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append("TesteHeader", "1234567");
        httpContext.Request.Headers.Append(keyNameTest[0], keyValueTest);

        ArrangeActionExecutingContextTests(httpContext);

        var apiKeyFilter = new ApiKeyFilter(
            loggerMock.Object,
            mockIConfiguration.Object,
            keyNameTest);

        apiKeyFilter.OnActionExecuting(_actionExecutingContext);

        var contentResult = (ContentResult)_actionExecutingContext.Result;

        Assert.Null(contentResult);

    }

    [Fact]
    public void OnActionExecuting_ShoultReturn200IfKeyValueMatchInClaim()
    {

        string[] keyNameTest = new[] { "MyKeyXyz", "OtherKeyWXZ" };
        string keyValueTest = "xxx1234";
        string keyValueTest1 = "98765abc";

        var loggerMock = new Mock<ILogger<ApiKeyFilter>>();

        var mockIConfiguration = new Mock<IConfiguration>();

        _ = mockIConfiguration.Setup(s => s.GetSection(keyNameTest[0]).Value)
            .Returns(keyValueTest);
        _ = mockIConfiguration.Setup(s => s.GetSection(keyNameTest[1]).Value)
            .Returns(keyValueTest1);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append("TesteHeader", "1234567");
        httpContext.Request.Headers.Append(keyNameTest[1], keyValueTest1);

        ArrangeActionExecutingContextTests(httpContext);

        var apiKeyFilter = new ApiKeyFilter(
            loggerMock.Object,
            mockIConfiguration.Object,
            keyNameTest);

        apiKeyFilter.OnActionExecuting(_actionExecutingContext);

        var contentResult = (ContentResult)_actionExecutingContext.Result;

        Assert.Null(contentResult);
        Assert.True(_actionExecutingContext.HttpContext.User.HasClaim(x =>
            x.Type == ApiKeyFilterConstants.ApiKeyInfo &&
            x.Value == keyNameTest[1]));

    }

    [Fact]
    public void OnActionExecuting_RegistersBothLegacyAndCanonicalClaims()
    {
        // Regression test: validates that ApiKeyFilter registers both claim types
        // to maintain backward compatibility during migration from legacy to canonical
        // authentication scheme.

        string[] keyNameTest = new[] { "ApiKeyTest" };
        string keyValueTest = "test-secret-key";
        var loggerMock = new Mock<ILogger<ApiKeyFilter>>();

        var mockIConfiguration = new Mock<IConfiguration>();
        _ = mockIConfiguration.Setup(s => s.GetSection(keyNameTest[0]).Value)
            .Returns(keyValueTest);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Append(keyNameTest[0], keyValueTest);

        ArrangeActionExecutingContextTests(httpContext);

        var apiKeyFilter = new ApiKeyFilter(
            loggerMock.Object,
            mockIConfiguration.Object,
            keyNameTest);

        apiKeyFilter.OnActionExecuting(_actionExecutingContext);

        // Authentication should succeed (no 401 error)
        var contentResult = (ContentResult)_actionExecutingContext.Result;
        Assert.Null(contentResult);

        var user = _actionExecutingContext.HttpContext.User;

        // Verify legacy claim is present (for backward compatibility)
        Assert.True(
            user.HasClaim(ApiKeyFilterConstants.ApiKeyInfo, keyNameTest[0]),
            $"Legacy claim not found. Expected type='{ApiKeyFilterConstants.ApiKeyInfo}', value='{keyNameTest[0]}'");

        // Verify canonical claim is present (for new canonical authentication)
        Assert.True(
            user.HasClaim(ApiKeyAuthenticationDefaults.ClaimType, keyNameTest[0]),
            $"Canonical claim not found. Expected type='{ApiKeyAuthenticationDefaults.ClaimType}', value='{keyNameTest[0]}'");

        // Verify both claims have the same value (key name)
        var legacyClaim = user.FindFirst(ApiKeyFilterConstants.ApiKeyInfo);
        var canonicalClaim = user.FindFirst(ApiKeyAuthenticationDefaults.ClaimType);

        Assert.NotNull(legacyClaim);
        Assert.NotNull(canonicalClaim);
        Assert.Equal(legacyClaim.Value, canonicalClaim.Value);
    }

}


