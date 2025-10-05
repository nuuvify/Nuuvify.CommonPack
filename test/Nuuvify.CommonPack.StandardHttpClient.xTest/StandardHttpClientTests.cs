using Nuuvify.CommonPack.StandardHttpClient.Results;

namespace Nuuvify.CommonPack.StandardHttpClient.xTest;

[Trait("Category", "Unit")]
public class StandardHttpClientTests
{

    private readonly Mock<IHttpClientFactory> mockFactory;
    private readonly IConfiguration Config;

    public StandardHttpClientTests()
    {
        mockFactory = new Mock<IHttpClientFactory>();

        Config = AppSettingsConfig.GetConfig();
    }

    [Fact]
    public async Task GetEmApiMockDeveRetornarMensagemOK()
    {
        var fakeClassReturn = new FakeClasseRetorno
        {
            Codigo = 123456,
            DataCadastro = DateTime.Now,
            Descricao = "Isso ã um teste"
        };

        var jsonConverted = JsonSerializer.Serialize(fakeClassReturn);

        var resultDefault = new HttpStandardReturn
        {
            ReturnCode = "200",
            ReturnMessage = jsonConverted,
            Success = true
        };

        // Criar HttpResponseMessage que será retornada pelo mock
        using var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonConverted, Encoding.UTF8, "application/json")
        };

        // Usar DelegatingHandlerStub corretamente
        using var clientHandlerStub = new DelegatingHandlerStub(mockResponse);

        // Criar HttpClient com disposeHandler = false para evitar problemas de disposal
        using var client = new HttpClient(clientHandlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        // Setup do mock - garantir que sempre retorne o mesmo cliente
        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => client);

        var url = "api/externafake";

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());

        // Criar cliente primeiro
        standardClient.CreateClient();

        // Fazer a requisição
        var result = await standardClient
            .WithQueryString("codigo", fakeClassReturn.Codigo)
            .Get(url);

        var urlReturn = "https://meuteste/api/externafake?codigo=123456";

        // Verificações
        Assert.NotNull(result);
        Assert.Equal(resultDefault.ReturnCode, result.ReturnCode);
        Assert.Equal(resultDefault.ReturnMessage, result.ReturnMessage);
        Assert.Equal(resultDefault.Success, result.Success);
        Assert.Equal(urlReturn, standardClient.FullUrl.ToString());
    }

    [Fact]
    public async Task PostDeveRetornarMensagemComSucesso()
    {
        // Arrange
        var fakeClass = new FakeClasseRetorno
        {
            Codigo = 654321,
            DataCadastro = DateTime.Now,
            Descricao = "Post realizado com sucesso"
        };

        var resultDefault = new HttpStandardReturn
        {
            ReturnCode = "201",
            ReturnMessage = "Created successfully",
            Success = true
        };

        var jsonConverted = JsonSerializer.Serialize(resultDefault);

        using var mockResponse = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(jsonConverted, Encoding.UTF8, "application/json")
        };

        using var clientHandlerStub = new DelegatingHandlerStub(mockResponse);
        using var client = new HttpClient(clientHandlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => client);

        var url = "api/cliente";

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());
        standardClient.CreateClient();

        // Act
        var result = await standardClient.Post(url, fakeClass);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(resultDefault.ReturnCode, result.ReturnCode);
        Assert.Equal(resultDefault.Success, result.Success);
    }

    [Fact]
    public async Task DeleteApiRealDeveRetornarMensagemComSucesso()
    {
        var resultDefault = new HttpStandardReturn
        {
            ReturnCode = "200",
            ReturnMessage = "Excluido com sucesso",
            Success = true
        };

        var jsonConverted = JsonSerializer.Serialize(resultDefault);

        using var clientHandlerStub = new DelegatingHandlerStub(new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(jsonConverted, Encoding.UTF8, "application/json")
        });

        using var client = new HttpClient(clientHandlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => client);

        var url = "api/cliente";

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());
        standardClient.CreateClient();
        standardClient.ResetStandardHttpClient();

        var result = await standardClient
            .WithHeader("Accept-Language", "pt-BR")
            .WithCurrelationHeader("zxzxzxzxzxzxzxzxzxz")
            .WithAuthorization("bearer", "xyz")
            .Delete(url);

        Assert.Equal(resultDefault.ReturnCode, result.ReturnCode);

    }

    [Fact]
    public async Task PatchApiRealDeveRetornarMensagemComSucesso()
    {
        var resultDefault = new HttpStandardReturn
        {
            ReturnCode = "200",
            ReturnMessage = "Alterado com sucesso",
            Success = true
        };

        var jsonConverted = JsonSerializer.Serialize(resultDefault);

        using var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonConverted, Encoding.UTF8, "application/json")
        };

        using var clientHandlerStub = new DelegatingHandlerStub(mockResponse);

        using var client = new HttpClient(clientHandlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => client);

        var url = "api/cliente";

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());
        standardClient.CreateClient();
        standardClient.ResetStandardHttpClient();
        var fakeClass = new FakeClasseRetorno
        {
            Codigo = 123456,
            DataCadastro = DateTime.Now,
            Descricao = "Isso ã um teste"
        };

        var result = await standardClient
            .WithHeader("Accept-Language", "pt-BR")
            .WithCurrelationHeader("zxzxzxzxzxzxzxzxzxz")
            .WithAuthorization("bearer", "xyz")
            .Patch(url, fakeClass);

        Assert.Equal(resultDefault.ReturnCode, result.ReturnCode);

    }

    [Fact]
    public void RecebeLoginDiferente_DeveSubstituirDados()
    {
        var usuarioAtual = "FulanoDeTal";
        var created = DateTimeOffset.Now;
        var expires = DateTimeOffset.Now.AddHours(8);
        var password = "XyZ";
        var token = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

        var _credentialToken = new CredentialToken
        {
            LoginId = usuarioAtual,
            Created = created,
            Expires = expires,
            Password = password,
            Token = token,
        };

        _credentialToken.LoginId = "Giropopis";

        Assert.NotEqual(_credentialToken.LoginId, usuarioAtual);
        Assert.NotEqual(_credentialToken.Token, token);

    }

    [Fact]
    public void RecebeLoginIgual_NaoDeveSubstituirDados()
    {
        var usuarioAtual = "FulanoDeTal";
        var created = DateTimeOffset.Now;
        var expires = DateTimeOffset.Now.AddHours(8);
        var password = "XyZ";
        var token = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

        var _credentialToken = new CredentialToken
        {
            LoginId = usuarioAtual,
            Created = created,
            Expires = expires,
            Password = password,
            Token = token,
        };

        _credentialToken.LoginId = "fulanodetal";

        Assert.Equal(_credentialToken.LoginId, usuarioAtual);
        Assert.Equal(_credentialToken.Token, token);

    }

    [Fact]
    public void TokenEstaValido()
    {
        var usuarioAtual = "FulanoDeTal";
        var created = DateTimeOffset.Now;
        var expires = DateTimeOffset.Now.AddHours(8);
        var password = "XyZ";
        var token = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

        var _credentialToken = new CredentialToken
        {
            LoginId = usuarioAtual,
            Created = created,
            Expires = expires,
            Password = password,
            Token = token,
        };

        Assert.True(_credentialToken.IsValidToken());

    }

    [Fact]
    public void TokenEstaInvalido()
    {
        var usuarioAtual = "FulanoDeTal";
        var created = DateTimeOffset.Now.AddMinutes(-7);
        var expires = DateTimeOffset.Now;
        var password = "XyZ";
        var token = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

        var _credentialToken = new CredentialToken
        {
            LoginId = usuarioAtual,
            Created = created,
            Expires = expires,
            Password = password,
            Token = token,
        };

        Assert.False(_credentialToken.IsValidToken());

    }

    [Fact]
    public async Task Caso_HttpContextAccessor_igual_null_nao_deve_retornar_exception()
    {
        var urlLogin = Config.GetSection("AppConfig:AppURLs:UrlLoginApi")?.Value;
        var urlToken = Config.GetSection("AppConfig:AppURLs:UrlLoginApiToken")?.Value;

        var credentialToken = new CredentialToken()
        {
            LoginId = "fulano",
            Password = "xyz9",
            Created = DateTimeOffset.Now,
            Expires = DateTimeOffset.Now.AddMinutes(60),
            Token = "meu token",
        };
        var returnClass = new
        {
            Success = true,
            Data = credentialToken
        };

        var mockConfiguration = new Mock<IConfiguration>();
        _ = mockConfiguration.Setup(x => x.GetSection("AzureAdOpenID:cc:ClientId").Value)
            .Returns(credentialToken.LoginId);
        _ = mockConfiguration.Setup(x => x.GetSection("AzureAdOpenID:cc:ClientSecret").Value)
            .Returns(credentialToken.Password);
        _ = mockConfiguration.Setup(x => x.GetSection("AppConfig:AppURLs:UrlLoginApiToken").Value)
            .Returns(urlToken);
        _ = mockConfiguration.Setup(x => x.GetSection("AppConfig:AppURLs:UrlLoginApi").Value)
            .Returns(urlLogin);

        var mockCredentialToken = new Mock<IOptions<CredentialToken>>();
        _ = mockCredentialToken.Setup(ap => ap.Value)
            .Returns(credentialToken);

        var jsonConverted = JsonSerializer.Serialize(returnClass);

        using var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonConverted, Encoding.UTF8, "application/json")
        };

        using var clientHandlerStub = new DelegatingHandlerStub(mockResponse);
        using var client = new HttpClient(clientHandlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri($"{urlLogin}/{urlToken}")
        };

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>()))
            .Returns(() => client);

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());

        var tokenService = new TokenService(mockCredentialToken.Object, standardClient, mockConfiguration.Object, new NullLogger<TokenService>(), null);
        var newToken = await tokenService.GetToken();

        Assert.True(newToken.IsValidToken());

    }

    [Fact]
    public async Task AutenticacaoBasicDeveGerarBase64()
    {

        var username = "zocateli";
        var password = "Xyz~123$";

        var userPassword = Encoding.ASCII.GetBytes($"{username}:{password}");
        var base64 = Convert.ToBase64String(userPassword);

        var messageTeste = new
        {
            Teste = "valor"
        };

        var resultDefault = new HttpStandardReturn
        {
            ReturnCode = "200",
            ReturnMessage = "Funcionou com sucesso",
            Success = true
        };

        var jsonConverted = JsonSerializer.Serialize(resultDefault);

        using var clientHandlerStub = new DelegatingHandlerStub(new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(jsonConverted, Encoding.UTF8, "application/json")
        });

        using var client = new HttpClient(clientHandlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => client);

        var url = "api/cliente";

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());
        standardClient.CreateClient();
        standardClient.ResetStandardHttpClient();
        standardClient.LogRequest = true;

        _ = await standardClient
            .WithHeader("Accept-Language", "pt-BR")
            .WithCurrelationHeader("zxzxzxzxzxzxzxzxzxz")
            .WithAuthorization("Basic", $"{username}:{password}")
            .Post(url, messageTeste);

        Assert.Equal(standardClient.AuthorizationLog, $"Basic {base64}");

    }

    [Fact]
    public async Task AutenticacaoBearer()
    {
        var token = "xyxyxyxyxyxyxyxyxyxyxyxyxyxy.";

        var messageTeste = new
        {
            Teste = "valor"
        };

        var resultDefault = new HttpStandardReturn
        {
            ReturnCode = "200",
            ReturnMessage = "Funcionou com sucesso",
            Success = true
        };

        var jsonConverted = JsonSerializer.Serialize(resultDefault);

        using var clientHandlerStub = new DelegatingHandlerStub(new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(jsonConverted, Encoding.UTF8, "application/json")
        });

        using var client = new HttpClient(clientHandlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => client);

        var url = "api/cliente";

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());
        standardClient.CreateClient();
        standardClient.ResetStandardHttpClient();
        standardClient.LogRequest = true;

        _ = await standardClient
            .WithHeader("Accept-Language", "pt-BR")
            .WithCurrelationHeader("zxzxzxzxzxzxzxzxzxz")
            .WithAuthorization("bearer", token)
            .Post(url, messageTeste);

        Assert.Equal(standardClient.AuthorizationLog, $"bearer {token}");

    }

    [Fact]
    public async Task PostComMediaTypeCustomizadoDeveRetornarSucesso()
    {
        // Arrange
        var xmlMessage = "<xml><test>value</test></xml>";
        var resultDefault = new HttpStandardReturn
        {
            ReturnCode = "200",
            ReturnMessage = "XML processed successfully",
            Success = true
        };

        var jsonConverted = JsonSerializer.Serialize(resultDefault);

        using var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonConverted, Encoding.UTF8, "application/json")
        };

        using var clientHandlerStub = new DelegatingHandlerStub(mockResponse);
        using var client = new HttpClient(clientHandlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => client);

        var url = "api/xml";

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());
        standardClient.CreateClient();

        // Act
        var result = await standardClient.Post(url, xmlMessage, "application/xml");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(resultDefault.ReturnCode, result.ReturnCode);
        Assert.Equal(resultDefault.Success, result.Success);
    }

    [Fact]
    public async Task PostComMultipartFormDataDeveRetornarSucesso()
    {
        // Arrange
        var resultDefault = new HttpStandardReturn
        {
            ReturnCode = "200",
            ReturnMessage = "File uploaded successfully",
            Success = true
        };

        var jsonConverted = JsonSerializer.Serialize(resultDefault);

        using var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonConverted, Encoding.UTF8, "application/json")
        };

        using var clientHandlerStub = new DelegatingHandlerStub(mockResponse);
        using var client = new HttpClient(clientHandlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => client);

        var url = "api/upload";

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());
        standardClient.CreateClient();

        // Arrange - MultipartFormDataContent
        using var multipartContent = new MultipartFormDataContent();
        multipartContent.Add(new StringContent("test file"), "file", "test.txt");

        // Act
        var result = await standardClient.Post(url, multipartContent);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(resultDefault.ReturnCode, result.ReturnCode);
        Assert.Equal(resultDefault.Success, result.Success);
    }

    [Fact]
    public async Task PostComFormParameterDeveRetornarSucesso()
    {
        // Arrange
        var resultDefault = new HttpStandardReturn
        {
            ReturnCode = "200",
            ReturnMessage = "Form data processed",
            Success = true
        };

        var jsonConverted = JsonSerializer.Serialize(resultDefault);

        using var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonConverted, Encoding.UTF8, "application/json")
        };

        using var clientHandlerStub = new DelegatingHandlerStub(mockResponse);
        using var client = new HttpClient(clientHandlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => client);

        var url = "api/form";

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());
        standardClient.CreateClient();

        // Act
        var result = await standardClient
            .WithFormParameter("key1", "value1")
            .WithFormParameter("key2", "value2")
            .Post(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(resultDefault.ReturnCode, result.ReturnCode);
        Assert.Equal(resultDefault.Success, result.Success);
    }

    [Fact]
    public async Task PutDeveRetornarSucesso()
    {
        // Arrange
        var fakeClass = new FakeClasseRetorno
        {
            Codigo = 789012,
            DataCadastro = DateTime.Now,
            Descricao = "Atualização realizada"
        };

        var resultDefault = new HttpStandardReturn
        {
            ReturnCode = "200",
            ReturnMessage = "Updated successfully",
            Success = true
        };

        var jsonConverted = JsonSerializer.Serialize(resultDefault);

        using var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonConverted, Encoding.UTF8, "application/json")
        };

        using var clientHandlerStub = new DelegatingHandlerStub(mockResponse);
        using var client = new HttpClient(clientHandlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => client);

        var url = "api/cliente";

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());
        standardClient.CreateClient();

        // Act
        var result = await standardClient.Put(url, fakeClass);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(resultDefault.ReturnCode, result.ReturnCode);
        Assert.Equal(resultDefault.Success, result.Success);
    }

    [Fact]
    public async Task PutComFormParameterDeveRetornarSucesso()
    {
        // Arrange
        var resultDefault = new HttpStandardReturn
        {
            ReturnCode = "200",
            ReturnMessage = "Form updated",
            Success = true
        };

        var jsonConverted = JsonSerializer.Serialize(resultDefault);

        using var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonConverted, Encoding.UTF8, "application/json")
        };

        using var clientHandlerStub = new DelegatingHandlerStub(mockResponse);
        using var client = new HttpClient(clientHandlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => client);

        var url = "api/form-put";

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());
        standardClient.CreateClient();

        // Act
        var result = await standardClient
            .WithFormParameter("updateKey", "updateValue")
            .Put(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(resultDefault.ReturnCode, result.ReturnCode);
        Assert.Equal(resultDefault.Success, result.Success);
    }

    [Fact]
    public async Task PatchComFormParameterDeveRetornarSucesso()
    {
        // Arrange
        var resultDefault = new HttpStandardReturn
        {
            ReturnCode = "200",
            ReturnMessage = "Patch completed",
            Success = true
        };

        var jsonConverted = JsonSerializer.Serialize(resultDefault);

        using var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonConverted, Encoding.UTF8, "application/json")
        };

        using var clientHandlerStub = new DelegatingHandlerStub(mockResponse);
        using var client = new HttpClient(clientHandlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => client);

        var url = "api/form-patch";

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());
        standardClient.CreateClient();

        // Act
        var result = await standardClient
            .WithFormParameter("patchKey", "patchValue")
            .Patch(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(resultDefault.ReturnCode, result.ReturnCode);
        Assert.Equal(resultDefault.Success, result.Success);
    }

    [Fact]
    public async Task GetStreamDeveRetornarStreamComSucesso()
    {
        // Arrange
        var streamContent = "This is a test stream content";
        var streamBytes = Encoding.UTF8.GetBytes(streamContent);

        using var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(streamBytes)
        };

        using var clientHandlerStub = new DelegatingHandlerStub(mockResponse);
        using var client = new HttpClient(clientHandlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/octet-stream");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => client);

        var url = "api/download";

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());
        standardClient.CreateClient();

        // Act
        var result = await standardClient.GetStream(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("200", result.ReturnCode);
        Assert.True(result.Success);
        Assert.NotNull(result.ReturnMessage);
    }

    [Fact]
    public async Task PostStreamComMultipartDeveRetornarStreamComSucesso()
    {
        // Arrange
        var streamContent = "Stream response from server";
        var streamBytes = Encoding.UTF8.GetBytes(streamContent);

        using var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(streamBytes)
        };

        using var clientHandlerStub = new DelegatingHandlerStub(mockResponse);
        using var client = new HttpClient(clientHandlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/octet-stream");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => client);

        var url = "api/upload-stream";

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());
        standardClient.CreateClient();

        // Arrange - MultipartFormDataContent
        using var multipartContent = new MultipartFormDataContent();
        multipartContent.Add(new StringContent("test file"), "file", "test.txt");

        // Act
        var result = await standardClient.PostStream(url, multipartContent);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("200", result.ReturnCode);
        Assert.True(result.Success);
        Assert.NotNull(result.ReturnMessage);
    }

    [Fact]
    public async Task PutStreamComMultipartDeveRetornarStreamComSucesso()
    {
        // Arrange
        var streamContent = "Updated stream data";
        var streamBytes = Encoding.UTF8.GetBytes(streamContent);

        using var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(streamBytes)
        };

        using var clientHandlerStub = new DelegatingHandlerStub(mockResponse);
        using var client = new HttpClient(clientHandlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/octet-stream");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => client);

        var url = "api/update-stream";

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());
        standardClient.CreateClient();

        // Arrange - MultipartFormDataContent
        using var multipartContent = new MultipartFormDataContent();
        using var stringContent3 = new StringContent("updated file");
        multipartContent.Add(stringContent3, "file", "updated.txt");

        // Act
        var result = await standardClient.PutStream(url, multipartContent);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("200", result.ReturnCode);
        Assert.True(result.Success);
        Assert.NotNull(result.ReturnMessage);
    }

    [Fact]
    public async Task PatchStreamComMultipartDeveRetornarStreamComSucesso()
    {
        // Arrange
        var streamContent = "Patched stream data";
        var streamBytes = Encoding.UTF8.GetBytes(streamContent);

        using var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(streamBytes)
        };

        using var clientHandlerStub = new DelegatingHandlerStub(mockResponse);
        using var client = new HttpClient(clientHandlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/octet-stream");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => client);

        var url = "api/patch-stream";

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());
        standardClient.CreateClient();

        // Arrange - MultipartFormDataContent
        using var multipartContent = new MultipartFormDataContent();
        using var stringContent4 = new StringContent("patched file");
        multipartContent.Add(stringContent4, "file", "patched.txt");

        // Act
        var result = await standardClient.PatchStream(url, multipartContent);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("200", result.ReturnCode);
        Assert.True(result.Success);
        Assert.NotNull(result.ReturnMessage);
    }

    [Fact]
    public async Task DeleteStreamComMultipartDeveRetornarStreamComSucesso()
    {
        // Arrange
        var streamContent = "Deletion confirmation stream";
        var streamBytes = Encoding.UTF8.GetBytes(streamContent);

        using var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(streamBytes)
        };

        using var clientHandlerStub = new DelegatingHandlerStub(mockResponse);
        using var client = new HttpClient(clientHandlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/octet-stream");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => client);

        var url = "api/delete-stream";

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());
        standardClient.CreateClient();

        // Arrange - MultipartFormDataContent
        using var multipartContent = new MultipartFormDataContent();
        using var stringContent5 = new StringContent("delete confirmation");
        multipartContent.Add(stringContent5, "confirmation", "delete.txt");

        // Act
        var result = await standardClient.DeleteStream(url, multipartContent);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("200", result.ReturnCode);
        Assert.True(result.Success);
        Assert.NotNull(result.ReturnMessage);
    }

    [Fact]
    public void CreateClientComHttpClientHandlerCustomizadoDeveConfigurarCorretamente()
    {
        // Arrange
        using var customHandler = new HttpClientHandler();

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());

        // Act
        standardClient.CreateClient(customHandler);

        // Assert - Verifica se o cliente foi configurado (não há exceções)
        Assert.NotNull(standardClient);
    }

    [Fact]
    public async Task ConfigureDeveAlterarParametrosDoHttpClient()
    {
        // Arrange
        var resultDefault = new HttpStandardReturn
        {
            ReturnCode = "200",
            ReturnMessage = "Configured successfully",
            Success = true
        };

        var jsonConverted = JsonSerializer.Serialize(resultDefault);

        using var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonConverted, Encoding.UTF8, "application/json")
        };

        using var clientHandlerStub = new DelegatingHandlerStub(mockResponse);
        using var client = new HttpClient(clientHandlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => client);

        var url = "api/test";

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());
        standardClient.CreateClient();

        // Act
        standardClient.Configure(
            timeOut: TimeSpan.FromMinutes(5),
            maxResponseContentBufferSize: 1024 * 1024,
            httpCompletionOption: HttpCompletionOption.ResponseHeadersRead);

        var result = await standardClient.Get(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(resultDefault.ReturnCode, result.ReturnCode);
        Assert.Equal(resultDefault.Success, result.Success);
    }

    [Fact]
    public async Task WithHeaderComKeyValuePairDeveAdicionarHeaderCorretamente()
    {
        // Arrange
        var resultDefault = new HttpStandardReturn
        {
            ReturnCode = "200",
            ReturnMessage = "Header added successfully",
            Success = true
        };

        var jsonConverted = JsonSerializer.Serialize(resultDefault);

        using var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonConverted, Encoding.UTF8, "application/json")
        };

        using var clientHandlerStub = new DelegatingHandlerStub(mockResponse);
        using var client = new HttpClient(clientHandlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => client);

        var url = "api/test-header";

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());
        standardClient.CreateClient();

        var customHeader = new KeyValuePair<string, string>("X-Custom-Header", "CustomValue");

        // Act
        var result = await standardClient
            .WithHeader(customHeader)
            .Get(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(resultDefault.ReturnCode, result.ReturnCode);
        Assert.Equal(resultDefault.Success, result.Success);
    }

    [Fact]
    public async Task HandleResponseMessageDeveProcessarRespostaCorretamente()
    {
        // Arrange
        var fakeData = new { Message = "Test data", Id = 123 };
        var jsonContent = JsonSerializer.Serialize(fakeData);

        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());

        // Act
        var result = await standardClient.HandleResponseMessage(response);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("200", result.ReturnCode);
        Assert.Equal(jsonContent, result.ReturnMessage);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task HandleResponseMessageStreamDeveProcessarStreamCorretamente()
    {
        // Arrange
        var streamContent = "Test stream data";
        var streamBytes = Encoding.UTF8.GetBytes(streamContent);

        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(streamBytes)
        };

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());

        // Act
        var result = await standardClient.HandleResponseMessageStream(response);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("200", result.ReturnCode);
        Assert.True(result.Success);
        Assert.NotNull(result.ReturnMessage);
    }

    [Fact]
    public async Task CenarioComErroHttpDeveRetornarResultadoComFalha()
    {
        // Arrange
        var errorMessage = "Internal Server Error";

        using var mockResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent(errorMessage, Encoding.UTF8, "text/plain")
        };

        using var clientHandlerStub = new DelegatingHandlerStub(mockResponse);
        using var client = new HttpClient(clientHandlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => client);

        var url = "api/error";

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());
        standardClient.CreateClient();

        // Act
        var result = await standardClient.Get(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("500", result.ReturnCode);
        Assert.False(result.Success);
        Assert.Contains(errorMessage, result.ReturnMessage);
    }

    [Fact]
    public async Task CenarioComTimeoutDeveRetornarResultadoComFalha()
    {
        // Arrange
        using var mockResponse = new HttpResponseMessage(HttpStatusCode.RequestTimeout)
        {
            Content = new StringContent("Request Timeout", Encoding.UTF8, "text/plain")
        };

        using var clientHandlerStub = new DelegatingHandlerStub(mockResponse);
        using var client = new HttpClient(clientHandlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => client);

        var url = "api/timeout";

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());
        standardClient.CreateClient();

        // Act
        var result = await standardClient.Get(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("408", result.ReturnCode);
        Assert.False(result.Success);
    }

    [Fact]
    public void PropriedadesPublicasDevemRetornarValoresCorretos()
    {
        // Arrange
        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());

        // Act & Assert - Propriedades iniciais
        Assert.False(standardClient.LogRequest);
        Assert.Null(standardClient.FullUrl);
        Assert.True(string.IsNullOrEmpty(standardClient.CorrelationId));
        Assert.True(string.IsNullOrEmpty(standardClient.AuthorizationLog));
        Assert.Null(standardClient.CustomHttpResponseMessage);

        // Test LogRequest property setter
        standardClient.LogRequest = true;
        Assert.True(standardClient.LogRequest);
    }

    [Fact]
    public void ResetStandardHttpClientDeveLimparTodasAsConfiguracoes()
    {
        // Arrange
        using var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("test", Encoding.UTF8, "application/json")
        };

        using var clientHandlerStub = new DelegatingHandlerStub(mockResponse);
        using var client = new HttpClient(clientHandlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(() => client);

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());
        standardClient.CreateClient();

        // Configurar algumas propriedades
        _ = standardClient
            .WithQueryString("test", "value")
            .WithHeader("X-Test", "TestValue")
            .WithCurrelationHeader("test-correlation-id")
            .WithAuthorization("Bearer", "test-token");

        // Act
        standardClient.ResetStandardHttpClient();

        // Assert - Propriedades devem estar limpar
        Assert.True(string.IsNullOrEmpty(standardClient.CorrelationId));
        Assert.Null(standardClient.FullUrl);
        Assert.Null(standardClient.CustomHttpResponseMessage);
    }

    [Fact]
    public void CreateClientComNomeDeveUsarClienteNomeado()
    {
        // Arrange
        var clientName = "TestClient";

        using var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("test", Encoding.UTF8, "application/json")
        };

        using var clientHandlerStub = new DelegatingHandlerStub(mockResponse);
        using var client = new HttpClient(clientHandlerStub, disposeHandler: false)
        {
            BaseAddress = new Uri("https://meuteste/")
        };

        _ = mockFactory.Setup(_ => _.CreateClient(clientName)).Returns(() => client);

        using var standardClient = new StandardHttpClient.StandardHttpClientService(
            mockFactory.Object,
            new NullLogger<StandardHttpClient.StandardHttpClientService>());

        // Act
        standardClient.CreateClient(clientName);

        // Assert - Verificar se o método foi chamado com o nome correto
        mockFactory.Verify(_ => _.CreateClient(clientName), Times.Once);
    }

}
