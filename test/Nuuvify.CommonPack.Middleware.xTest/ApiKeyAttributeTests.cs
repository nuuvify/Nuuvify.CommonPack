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

        [Fact]
        public void OnResourceExecuting_ShoultAddASingleLogIfExecuted()
        {

            string keyNameTest = "MyKeyXyz";
            var loggerMock = new Mock<ILogger<ApiKeyFilter>>();

            var mockIConfiguration = new Mock<IConfiguration>();

            mockIConfiguration.Setup(s => s.GetSection(keyNameTest).Value)
                .Returns("xxx1234");


            var actionContext = new ActionContext()
            {
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
            };


            var resourceExecutingContext = new ResourceExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new List<IValueProviderFactory>()
                );


            var apiKeyFilter = new ApiKeyFilter(
                loggerMock.Object,
                mockIConfiguration.Object,
                keyNameTest);

            apiKeyFilter.OnResourceExecuting(resourceExecutingContext);


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
        public void OnActionExecuting_ShoultReturn401IfKeyValueNotMatchInHttpHeader()
        {

            string keyNameTest = "MyKeyXyz";
            string keyValueTest = "xxx1234";
            var loggerMock = new Mock<ILogger<ApiKeyFilter>>();

            var mockIConfiguration = new Mock<IConfiguration>();

            mockIConfiguration.Setup(s => s.GetSection(keyNameTest).Value)
                .Returns(keyValueTest);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("TesteHeader", "1234567");
            httpContext.Request.Headers.Add(keyNameTest, "outrovalor");

            var actionContext = new ActionContext()
            {
                HttpContext = httpContext,
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
            };

            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                new object()
                );


            var apiKeyFilter = new ApiKeyFilter(
                loggerMock.Object,
                mockIConfiguration.Object,
                keyNameTest);

            apiKeyFilter.OnActionExecuting(actionExecutingContext);

            var contentResult = (ContentResult)actionExecutingContext.Result;


            Assert.Equal(expected: 401, actual: contentResult.StatusCode);


        }


        [Fact]
        public void OnActionExecuting_ShoultReturn200IfKeyValueMatchInHttpHeader()
        {

            string keyNameTest = "MyKeyXyz";
            string keyValueTest = "xxx1234";
            var loggerMock = new Mock<ILogger<ApiKeyFilter>>();

            var mockIConfiguration = new Mock<IConfiguration>();

            mockIConfiguration.Setup(s => s.GetSection(keyNameTest).Value)
                .Returns(keyValueTest);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("TesteHeader", "1234567");
            httpContext.Request.Headers.Add(keyNameTest, keyValueTest);

            var actionContext = new ActionContext()
            {
                HttpContext = httpContext,
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
            };

            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                new object()
                );


            var apiKeyFilter = new ApiKeyFilter(
                loggerMock.Object,
                mockIConfiguration.Object,
                keyNameTest);

            apiKeyFilter.OnActionExecuting(actionExecutingContext);

            var contentResult = (ContentResult)actionExecutingContext.Result;


            Assert.Null(contentResult);


        }


    }

}