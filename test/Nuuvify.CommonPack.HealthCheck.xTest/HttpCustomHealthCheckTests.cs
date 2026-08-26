using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nuuvify.CommonPack.HealthCheck;
using Shouldly;
using Xunit;

namespace Nuuvify.CommonPack.HealthCheck.xTest;

[Trait("Category", "Unit")]
public sealed class HttpCustomHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WhenResponseIsOk_ShouldReturnHealthy()
    {
        using var clientHandler = new StubHttpClientHandler(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("healthy")
        });
        var healthCheck = new HttpCustomHealthCheck(
            new Uri("https://example.test/"),
            "health",
            false,
            HealthStatus.Unhealthy,
            clientHandler);

        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        result.Status.ShouldBe(HealthStatus.Healthy);
        result.Description.ShouldContain(nameof(HealthStatus.Healthy));
    }

    [Fact]
    public async Task CheckHealthAsync_WhenResponseIsNotSuccessful_ShouldUseConfiguredFailureStatus()
    {
        using var clientHandler = new StubHttpClientHandler(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.ServiceUnavailable,
            Content = new StringContent("unhealthy")
        });
        var healthCheck = new HttpCustomHealthCheck(
            new Uri("https://example.test/"),
            "health",
            false,
            HealthStatus.Degraded,
            clientHandler);

        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        result.Status.ShouldBe(HealthStatus.Degraded);
        result.Description.ShouldContain(nameof(HealthStatus.Degraded));
    }

    [Fact]
    public async Task CheckHealthAsync_WhenHttpRequestFails_ShouldReturnUnhealthyWithException()
    {
        using var clientHandler = new ThrowingHttpClientHandler(new HttpRequestException("request failed"));
        var healthCheck = new HttpCustomHealthCheck(
            new Uri("https://example.test/"),
            "health",
            false,
            HealthStatus.Degraded,
            clientHandler);

        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        result.Status.ShouldBe(HealthStatus.Unhealthy);
        result.Exception.ShouldBeOfType<HttpRequestException>();
        result.Description.ShouldContain(nameof(HttpRequestException));
    }

    private sealed class StubHttpClientHandler : HttpClientHandler
    {
        private readonly HttpResponseMessage response;

        public StubHttpClientHandler(HttpResponseMessage response)
        {
            this.response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(response);
        }
    }

    private sealed class ThrowingHttpClientHandler : HttpClientHandler
    {
        private readonly Exception exception;

        public ThrowingHttpClientHandler(Exception exception)
        {
            this.exception = exception;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromException<HttpResponseMessage>(exception);
        }
    }
}
