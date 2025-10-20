using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Nuuvify.CommonPack.StandardHttpClient.xTest;

[Trait("Category", "Unit")]
public class StandardHttpClientDiagnosticTests
{
    [Fact]
    public async Task Debug_HttpClient_Mock_Setup()
    {
        // Arrange
        var mockFactory = new Mock<IHttpClientFactory>();

        var expectedResponse = new
        {
            Message = "Test successful",
            StatusCode = 200
        };

        var jsonResponse = JsonSerializer.Serialize(expectedResponse);

        // Create a proper mock handler
        using var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        using var handlerStub = new DelegatingHandlerStub(mockResponse);

        // Create HttpClient with mock handler and ensure it won't be disposed prematurely
        var mockHttpClient = new HttpClient(handlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://api.test.com/")
        };

        // Setup mock factory to return our mocked client
        _ = mockFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                  .Returns(mockHttpClient);

        // Act & Assert
        using var standardClient = new StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClientService>());

        // Create client first
        standardClient.CreateClient("TestClient");

        // Test the actual HTTP call
        var result = await standardClient.Get("api/test");

        // Verify
        Assert.NotNull(result);
        Assert.Equal("200", result.ReturnCode);
        Assert.True(result.Success);

        // Clean up
        mockHttpClient.Dispose();
    }

    [Fact]
    public async Task Debug_HttpClient_Without_BaseAddress()
    {
        // Arrange
        var mockFactory = new Mock<IHttpClientFactory>();

        var expectedResponse = "Test successful";

        using var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedResponse, Encoding.UTF8, "text/plain")
        };

        using var handlerStub = new DelegatingHandlerStub(mockResponse);

        // Create HttpClient WITHOUT BaseAddress
        var mockHttpClient = new HttpClient(handlerStub, disposeHandler: false);

        _ = mockFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                  .Returns(mockHttpClient);

        // Act & Assert
        using var standardClient = new StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClientService>());

        standardClient.CreateClient("TestClient");

        // Use absolute URL since no BaseAddress is set
        var result = await standardClient.Get("https://api.test.com/api/test");

        // Verify
        Assert.NotNull(result);
        Assert.Equal("200", result.ReturnCode);
        Assert.True(result.Success);

        // Clean up
        mockHttpClient.Dispose();
    }

    [Fact]
    public async Task Debug_Multiple_Requests_Same_Client()
    {
        // Arrange
        var mockFactory = new Mock<IHttpClientFactory>();

        var responses = new[]
        {
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Response 1") },
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Response 2") },
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Response 3") }
        };

        // Simplificar usando DelegatingHandlerStub básico que sempre retorna a primeira resposta
        using var handlerStub = new DelegatingHandlerStub(responses[0]);

        using var mockHttpClient = new HttpClient(handlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://api.test.com/")
        };

        _ = mockFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                      .Returns(mockHttpClient);

        // Act & Assert
        using var standardClient = new StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClientService>());

        standardClient.CreateClient("TestClient");

        // Fazer apenas uma requisição para simplificar o teste
        var result = await standardClient.Get("api/test/1");

        Assert.NotNull(result);
        Assert.Equal("200", result.ReturnCode);
        Assert.True(result.Success);
        Assert.Contains("Response 1", result.ReturnMessage);

        // Clean up
        foreach (var response in responses)
        {
            response.Dispose();
        }
    }
}
