using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Nuuvify.CommonPack.Security.Abstraction;
using Nuuvify.CommonPack.StandardHttpClient.Polly;
using Nuuvify.CommonPack.StandardHttpClient.Results;
using Nuuvify.CommonPack.StandardHttpClient.xTest.Configs;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Nuuvify.CommonPack.StandardHttpClient.xTest;


public class StandardHttpClientTests
{

    private readonly Mock<IHttpClientFactory> mockFactory;
    private readonly IConfiguration Config;

    public StandardHttpClientTests()
    {
        mockFactory = new Mock<IHttpClientFactory>();

        Config = AppSettingsConfig.GetConfig();
    }

    [Fact, Order(1)]
    public async Task GetEmApiMockDeveRetornarMensagemOK()
    {
        var fakeClassReturn = new FakeClasseRetorno
        {
            Codigo = 123456,
            DataCadastro = DateTime.Now,
            Descricao = "Isso é um teste"
        };

        var jsonConverted = JsonSerializer.Serialize(fakeClassReturn);

        var resultDefault = new HttpStandardReturn
        {
            ReturnCode = "200",
            ReturnMessage = jsonConverted,
            Success = true
        };

        var clientHandlerStub = new DelegatingHandlerStub(new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(jsonConverted, Encoding.UTF8, "application/json")
        });

        var client = new HttpClient(clientHandlerStub, true)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

        var url = "api/externafake";

        var standardClient = new StandardHttpClient(mockFactory.Object, new NullLogger<StandardHttpClient>());
        standardClient.CreateClient();

        var result = await standardClient
            .WithQueryString("codigo", fakeClassReturn.Codigo)
            .Get(url);

        var urlReturn = "https://meuteste/api/externafake?codigo=123456";

        Assert.Equal(resultDefault.ReturnCode, result.ReturnCode);
        Assert.Equal(resultDefault.ReturnMessage, result.ReturnMessage);
        Assert.Equal(resultDefault.Success, result.Success);
        Assert.Equal(urlReturn, standardClient.FullUrl.ToString());

    }

    [LocalTestFact, Order(2)]
    public void PostApiRealDeveRetornarMensagemComSucesso()
    {
        var config = AppSettingsConfig.GetConfig();
        var tokenFactory = new TokenFactory(config);
        var tokenValido = tokenFactory.ObtemTokenValido(
            loginId: config.GetSection("AzureAdOpenID:cc:ClientId")?.Value,
            password: config.GetSection("AzureAdOpenID:cc:ClientSecret")?.Value
        ).Result;

        var notification = tokenFactory?.Notifications.LastOrDefault();
        var expected = false;
        var actual = string.IsNullOrWhiteSpace(tokenValido);

        Assert.True(string.IsNullOrWhiteSpace(notification?.Message), notification?.Message);
        Assert.Equal(expected, actual);

    }

    [Fact, Order(3)]
    public void DeleteApiRealDeveRetornarMensagemComSucesso()
    {
        var resultDefault = new HttpStandardReturn
        {
            ReturnCode = "200",
            ReturnMessage = "Excluido com sucesso",
            Success = true
        };

        var jsonConverted = JsonSerializer.Serialize(resultDefault);

        var clientHandlerStub = new DelegatingHandlerStub(new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(jsonConverted, Encoding.UTF8, "application/json")
        });

        var client = new HttpClient(clientHandlerStub, true)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

        var url = "api/cliente";

        var standardClient = new StandardHttpClient(mockFactory.Object, new NullLogger<StandardHttpClient>());
        standardClient.CreateClient();
        standardClient.ResetStandardHttpClient();

        var result = standardClient
            .WithHeader("Accept-Language", "pt-BR")
            .WithCurrelationHeader("zxzxzxzxzxzxzxzxzxz")
            .WithAuthorization("bearer", "xyz")
            .Delete(url).Result;

        Assert.Equal(resultDefault.ReturnCode, result.ReturnCode);

    }

    [Fact, Order(4)]
    public void PatchApiRealDeveRetornarMensagemComSucesso()
    {
        var resultDefault = new HttpStandardReturn
        {
            ReturnCode = "200",
            ReturnMessage = "Alterado com sucesso",
            Success = true
        };

        var jsonConverted = JsonSerializer.Serialize(resultDefault);

        var clientHandlerStub = new DelegatingHandlerStub(new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(jsonConverted, Encoding.UTF8, "application/json")
        });

        var client = new HttpClient(clientHandlerStub, true)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

        var url = "api/cliente";

        var standardClient = new StandardHttpClient(mockFactory.Object, new NullLogger<StandardHttpClient>());
        standardClient.CreateClient();
        standardClient.ResetStandardHttpClient();
        var fakeClass = new FakeClasseRetorno
        {
            Codigo = 123456,
            DataCadastro = DateTime.Now,
            Descricao = "Isso é um teste"
        };

        var result = standardClient
            .WithHeader("Accept-Language", "pt-BR")
            .WithCurrelationHeader("zxzxzxzxzxzxzxzxzxz")
            .WithAuthorization("bearer", "xyz")
            .Patch(url, fakeClass).Result;

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

        var resultDefault = new HttpStandardReturn
        {
            ReturnCode = "200",
            ReturnMessage = jsonConverted,
            Success = true
        };
        var clientHandlerStub = new DelegatingHandlerStub(new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(jsonConverted, Encoding.UTF8, "application/json")
        });
        var client = new HttpClient(clientHandlerStub, true)
        {
            BaseAddress = new Uri($"{urlLogin}/{urlToken}")
        };

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>()))
            .Returns(client);

        var standardClient = new StandardHttpClient(mockFactory.Object, new NullLogger<StandardHttpClient>());

        var tokenService = new TokenService(mockCredentialToken.Object, standardClient, mockConfiguration.Object, new NullLogger<TokenService>(), null);
        var newToken = await tokenService.GetToken();

        Assert.True(newToken.IsValidToken());

    }

    [Fact]
    public void AutenticacaoBasicDeveGerarBase64()
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

        var clientHandlerStub = new DelegatingHandlerStub(new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(jsonConverted, Encoding.UTF8, "application/json")
        });

        var client = new HttpClient(clientHandlerStub, true)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

        var url = "api/cliente";

        var standardClient = new StandardHttpClient(mockFactory.Object, new NullLogger<StandardHttpClient>());
        standardClient.CreateClient();
        standardClient.ResetStandardHttpClient();
        standardClient.LogRequest = true;

        var result = standardClient
            .WithHeader("Accept-Language", "pt-BR")
            .WithCurrelationHeader("zxzxzxzxzxzxzxzxzxz")
            .WithAuthorization("Basic", $"{username}:{password}")
            .Post(url, messageTeste).Result;

        Assert.Equal(standardClient.AuthorizationLog, $"Basic {base64}");

    }

    [Fact]
    public void AutenticacaoBearer()
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

        var clientHandlerStub = new DelegatingHandlerStub(new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(jsonConverted, Encoding.UTF8, "application/json")
        });

        var client = new HttpClient(clientHandlerStub, true)
        {
            BaseAddress = new Uri("https://meuteste/")
        };
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

        var url = "api/cliente";

        var standardClient = new StandardHttpClient(mockFactory.Object, new NullLogger<StandardHttpClient>());
        standardClient.CreateClient();
        standardClient.ResetStandardHttpClient();
        standardClient.LogRequest = true;

        var result = standardClient
            .WithHeader("Accept-Language", "pt-BR")
            .WithCurrelationHeader("zxzxzxzxzxzxzxzxzxz")
            .WithAuthorization("bearer", token)
            .Post(url, messageTeste).Result;

        Assert.Equal(standardClient.AuthorizationLog, $"bearer {token}");

    }

}
