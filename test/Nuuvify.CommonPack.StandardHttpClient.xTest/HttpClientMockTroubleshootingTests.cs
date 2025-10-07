using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Nuuvify.CommonPack.StandardHttpClient.xTest;

[Trait("Category", "Unit")]
public class HttpClientMockTroubleshootingTests
{
    private readonly ITestOutputHelper _output;

    public HttpClientMockTroubleshootingTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Troubleshoot_OriginalTest_WithDebugging()
    {
        try
        {
            _output.WriteLine("=== INICIANDO TESTE DE DIAGNÓSTICO ===");

            var mockFactory = new Mock<IHttpClientFactory>();

            var fakeClassReturn = new FakeClasseRetorno
            {
                Codigo = 123456,
                DataCadastro = DateTime.Now,
                Descricao = "Isso é um teste"
            };

            var jsonConverted = JsonSerializer.Serialize(fakeClassReturn);
            _output.WriteLine($"JSON serializado: {jsonConverted}");

            // Criar HttpResponseMessage
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonConverted, Encoding.UTF8, "application/json")
            };
            _output.WriteLine("HttpResponseMessage criada com sucesso");

            // Usar handler stub
            var clientHandlerStub = new DelegatingHandlerStub(mockResponse);
            _output.WriteLine("DelegatingHandlerStub criado");

            // Criar HttpClient
            var client = new HttpClient(clientHandlerStub, disposeHandler: false)
            {
                BaseAddress = new Uri("https://meuteste/")
            };
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            _output.WriteLine($"HttpClient criado com BaseAddress: {client.BaseAddress}");

            // Setup mock
            _ = mockFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
                      .Returns(() =>
                      {
                          _output.WriteLine("Mock CreateClient chamado");
                          return client;
                      });

            // Criar StandardHttpClient
            using var standardClient = new StandardHttpClient.StandardHttpClientService(
                mockFactory.Object,
                new NullLogger<StandardHttpClient.StandardHttpClientService>());

            _output.WriteLine("StandardHttpClientService criado");

            // Criar cliente
            standardClient.CreateClient();
            _output.WriteLine("CreateClient() chamado");

            var url = "api/externafake";
            _output.WriteLine($"URL da requisição: {url}");

            // Fazer requisição
            _output.WriteLine("Fazendo requisição GET...");
            var result = await standardClient
                .WithQueryString("codigo", fakeClassReturn.Codigo)
                .Get(url);

            _output.WriteLine($"Requisição completada. StatusCode: {result.ReturnCode}, Success: {result.Success}");
            _output.WriteLine($"Response Message: {result.ReturnMessage}");
            _output.WriteLine($"Full URL: {standardClient.FullUrl}");

            // Verificações
            Assert.NotNull(result);
            Assert.Equal("200", result.ReturnCode);
            Assert.True(result.Success);

            _output.WriteLine("=== TESTE CONCLUÍDO COM SUCESSO ===");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"=== ERRO NO TESTE ===");
            _output.WriteLine($"Tipo da exceção: {ex.GetType().Name}");
            _output.WriteLine($"Mensagem: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");

            if (ex.InnerException != null)
            {
                _output.WriteLine($"Inner exception: {ex.InnerException.GetType().Name}");
                _output.WriteLine($"Inner message: {ex.InnerException.Message}");
            }

            throw; // Re-throw para que o teste falhe
        }
    }

    [Fact]
    public async Task Test_HttpClient_Directly()
    {
        _output.WriteLine("=== TESTANDO HTTPCLIENT DIRETAMENTE ===");

        try
        {
            var jsonResponse = JsonSerializer.Serialize(new { message = "test" });

            using var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            };

            using var handler = new DelegatingHandlerStub(response);
            using var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://test.com/")
            };

            _output.WriteLine("HttpClient criado diretamente");

            // Fazer requisição direta
            var directResult = await httpClient.GetAsync("api/test");
            var content = await directResult.Content.ReadAsStringAsync();

            _output.WriteLine($"Requisição direta - Status: {directResult.StatusCode}");
            _output.WriteLine($"Content: {content}");

            Assert.Equal(HttpStatusCode.OK, directResult.StatusCode);
            Assert.Contains("test", content);

            _output.WriteLine("=== TESTE DIRETO HTTPCLIENT SUCESSO ===");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"=== ERRO NO TESTE DIRETO ===");
            _output.WriteLine($"Exception: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }
}
