using Nuuvify.CommonPack.StandardHttpClient.Results;

namespace Nuuvify.CommonPack.StandardHttpClientService.xTest;

public class StandardHttpClientServiceStreamTests
{

    private readonly Mock<IHttpClientFactory> mockFactory;
    private readonly IConfiguration Config;

    public StandardHttpClientServiceStreamTests()
    {
        mockFactory = new Mock<IHttpClientFactory>();

        Config = AppSettingsConfig.GetConfig();
    }

    [Fact]
    public async Task EnviaArquivoPorStreamComPost()
    {
        string[] FilesToPost = { "ubuntu3d--dark-blue.jpg", "Excel Example File.xlsx" };

        var api = "http://localhost:5000/";
        var pathName = AppDomain.CurrentDomain.BaseDirectory;

        var uri = new Uri(api);

        using var clientHandlerStub = new DelegatingHandlerStub(new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("Testando retorno")
        });
        using var client = new HttpClient(clientHandlerStub, true)
        {
            BaseAddress = uri
        };

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>()))
            .Returns(client);

        var url = $"{api}v1/Anexos";

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());
        standardClient.CreateClient();
        standardClient.ResetStandardHttpClient();

        var anexo = new AnexoFixture("df33819a-1b97-4a9e-b948-e8972ef5990a", "ConsultaAtivos");

        StringContent stringContent;
        ContentDispositionHeaderValue stringHeader;
        HttpStandardReturn result = null;
        ByteArrayContent fileContent;
        string fullPathFileNamePost;

        using (var multipartContent = new MultipartFormDataContent())
        {

            foreach (var file in FilesToPost)
            {
                fullPathFileNamePost = Path.Combine(pathName, "Arrange", file);

                using (var fs = File.OpenRead(fullPathFileNamePost))
                {
                    using (var streamContent = new StreamContent(fs))
                    {
                        fileContent = new ByteArrayContent(await streamContent.ReadAsByteArrayAsync());

                        stringHeader = new ContentDispositionHeaderValue("form-data")
                        {
                            Name = nameof(anexo.AggregateId)
                        };
                        stringContent = new StringContent(anexo.AggregateId);
                        stringContent.Headers.ContentDisposition = stringHeader;
                        multipartContent.Add(stringContent);

                        stringHeader = new ContentDispositionHeaderValue("form-data")
                        {
                            Name = nameof(anexo.TipoAnexo)
                        };
                        stringContent = new StringContent(anexo.TipoAnexo);
                        stringContent.Headers.ContentDisposition = stringHeader;
                        multipartContent.Add(stringContent);

                        stringHeader = new ContentDispositionHeaderValue("form-data")
                        {
                            Name = "Anexos",
                            FileName = file
                        };
                        fileContent.Headers.ContentDisposition = stringHeader;
                        multipartContent.Add(fileContent);

                    }
                }

            }

            result = await standardClient
                .WithAuthorization(schema: "bearer", token: "XYZ")
                .Post(url, multipartContent);

        }

        Assert.True(result.Success);

    }

}
