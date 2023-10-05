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
using Xunit;

namespace Nuuvify.CommonPack.Middleware.xTest
{
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

            mockIConfiguration.Setup(s => s.GetSection(keyNameTest[0]).Value)
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

            mockIConfiguration.Setup(s => s.GetSection(keyNameTest[0]).Value)
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

            mockIConfiguration.Setup(s => s.GetSection(keyNameTest[0]).Value)
                .Returns(keyValueTest);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("TesteHeader", "1234567");
            httpContext.Request.Headers.Add(keyNameTest[0], "outrovalor");

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

            mockIConfiguration.Setup(s => s.GetSection(keyNameTest[0]).Value)
                .Returns(keyValueTest);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("TesteHeader", "1234567");
            httpContext.Request.Headers.Add(keyNameTest[0], keyValueTest);

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

            mockIConfiguration.Setup(s => s.GetSection(keyNameTest[0]).Value)
                .Returns(keyValueTest);
            mockIConfiguration.Setup(s => s.GetSection(keyNameTest[1]).Value)
                .Returns(keyValueTest1);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("TesteHeader", "1234567");
            httpContext.Request.Headers.Add(keyNameTest[1], keyValueTest1);

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
                x.Value == $"{keyNameTest[1]}={keyValueTest1}"));


        }


    }

}