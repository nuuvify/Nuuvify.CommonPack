using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Nuuvify.CommonPack.Mediator.Implementation;
using Nuuvify.CommonPack.Security.Abstraction;
using Nuuvify.CommonPack.StandardHttpClient.Polly;
using Nuuvify.CommonPack.StandardHttpClient.xTest.Configs;
using Nuuvify.CommonPack.StandardHttpClient.xTest.Fixtures;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Nuuvify.CommonPack.StandardHttpClient.xTest;

[Order(2)]
public class BaseHttpCustomTests : NotifiableR
{
    private readonly Mock<IHttpClientFactory> mockFactory;
    private readonly Mock<IConfiguration> mockConfiguration;
    private readonly Mock<IHttpContextAccessor> mockUserAuthenticated;

    private readonly IConfiguration Config;

    public BaseHttpCustomTests()
    {
        mockFactory = new Mock<IHttpClientFactory>();
        mockConfiguration = new Mock<IConfiguration>();
        mockUserAuthenticated = new Mock<IHttpContextAccessor>();

        Config = AppSettingsConfig.GetConfig();
    }

    [Fact, Order(1)]
    public async Task QuandoObterTokenRetornarErroNotificationDeveSerLancada()
    {

        var handler = new HttpClientHandler();
        var client = new HttpClient(handler, true)
        {
            BaseAddress = new Uri(Config.GetSection("AppConfig:AppURLs:UrlLoginApi")?.Value)
        };
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>()))
            .Returns(client);

        var standardClient = new StandardHttpClient(mockFactory.Object, new NullLogger<StandardHttpClient>());

        var username = Config.GetSection("AzureAdOpenID:cc:ClientId")?.Value;
        var password = Config.GetSection("AzureAdOpenID:cc:ClientSecret")?.Value;
        var urlusername = Config.GetSection("AppConfig:AppURLs:UrlLoginApi")?.Value;
        var urlToken = "api/urlfake";

        _ = mockConfiguration.Setup(x => x.GetSection("AzureAdOpenID:cc:ClientId").Value)
            .Returns(username);
        _ = mockConfiguration.Setup(x => x.GetSection("AzureAdOpenID:cc:ClientSecret").Value)
            .Returns(password);
        _ = mockConfiguration.Setup(x => x.GetSection("AppConfig:AppURLs:UrlLoginApiToken").Value)
            .Returns(urlToken);

        var credentialToken = new CredentialToken()
        {
            LoginId = username,
            Password = password,
            Expires = DateTimeOffset.Now.AddMinutes(-10)
        };
        var mockCredentialToken = new Mock<IOptions<CredentialToken>>();
        _ = mockCredentialToken.Setup(ap => ap.Value)
            .Returns(credentialToken);

        var user = new UserFixture().GetUserPrincipalFake();

        _ = mockUserAuthenticated.Setup(_ => _.HttpContext.User)
            .Returns(user);

        var tokenService = new TokenService(mockCredentialToken.Object, standardClient, mockConfiguration.Object, new NullLogger<TokenService>(), mockUserAuthenticated.Object);
        var retorno = await tokenService.GetToken();

        Assert.Null(tokenService.GetActualToken()?.Token);
        Assert.Null(retorno);
        Assert.True(tokenService.Notifications.Count > 0);
    }

    [LocalTestFact, Order(2)]
    public async Task ObtemCadastroPessoaValido()
    {
        var config = AppSettingsConfig.GetConfig();

        var tokenFactory = new TokenFactory(config);
        var tokenValido = await tokenFactory.ObtemTokenValido(
            loginId: config.GetSection("AzureAdOpenID:cc:ClientId")?.Value,
            password: config.GetSection("AzureAdOpenID:cc:ClientSecret")?.Value
        );

        var notification = tokenFactory?.Notifications.LastOrDefault();
        Assert.True(string.IsNullOrWhiteSpace(notification?.Message), notification?.Message);

        var urlSynchro = config.GetSection("AppConfig:AppURLs:UrlPessoasApi")?.Value;

        var handler = new HttpClientHandler();
        var client = new HttpClient(handler, true)
        {
            BaseAddress = new Uri(urlSynchro)
        };
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        _ = mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>()))
            .Returns(client);

        var url = $"api/v2/Pessoas/cpfcnpj";

        var standardClientSynchro = new StandardHttpClient(mockFactory.Object, new NullLogger<StandardHttpClient>());
        standardClientSynchro.CreateClient();
        standardClientSynchro.ResetStandardHttpClient();

        var returnSynchro = await standardClientSynchro
            .WithCurrelationHeader("teste1234")
            .WithQueryString("codigo", "61064911000177")
            .WithQueryString("vigenciaInicial", "2020-01-19")
            .WithQueryString("vigenciaFinal", "2020-01-19")
            .WithAuthorization("bearer", tokenValido)
            .Get(url);

        var expected = "200";
        var actual = returnSynchro?.ReturnCode;

        Assert.Equal(expected, actual);

    }

}
