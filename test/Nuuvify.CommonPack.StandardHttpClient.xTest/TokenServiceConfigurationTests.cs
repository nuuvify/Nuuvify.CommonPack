using Microsoft.Extensions.Logging.Abstractions;
using Nuuvify.CommonPack.StandardHttpClient.Results;

namespace Nuuvify.CommonPack.StandardHttpClient.xTest;

[Trait("Category", "Unit")]
public class TokenServiceConfigurationTests
{
    private const string GuardMessageFragment = "não deveria estar branco";
    private const string UrlLoginApi = "https://login/";
    private const string UrlLoginApiTokenPath = "connect/token";
    private const string ExpectedTokenUrl = UrlLoginApi + UrlLoginApiTokenPath;

    private static void SetupSection(Mock<IConfiguration> configuration, string key, string value)
    {
        var section = new Mock<IConfigurationSection>();
        _ = section.Setup(s => s.Value).Returns(value);
        _ = configuration.Setup(c => c.GetSection(key)).Returns(section.Object);
    }

    private static TokenService CreateTokenService(
        Mock<IConfiguration> configuration,
        Mock<IStandardHttpClient> httpClient)
    {
        var options = new Mock<IOptions<CredentialToken>>();
        _ = options.Setup(o => o.Value).Returns(new CredentialToken());

        var accessor = new Mock<IHttpContextAccessor>();

        return new TokenService(
            options.Object,
            httpClient.Object,
            configuration.Object,
            NullLogger<TokenService>.Instance,
            accessor.Object);
    }

    [Fact]
    public async Task GetToken_ComChavesDuploHifen_DeveResolverCredenciaisEChamarApi()
    {
        var configuration = new Mock<IConfiguration>();
        SetupSection(configuration, "AzureAdOpenID--cc--ClientId", "client-id-literal");
        SetupSection(configuration, "AzureAdOpenID--cc--ClientSecret", "client-secret-literal");
        SetupSection(configuration, "AppConfig:AppURLs:UrlLoginApi", UrlLoginApi);
        SetupSection(configuration, "AppConfig:AppURLs:UrlLoginApiToken", UrlLoginApiTokenPath);

        var httpClient = new Mock<IStandardHttpClient>();
        _ = httpClient.Setup(x => x.WithHeader(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(httpClient.Object);
        _ = httpClient.Setup(x => x.Post(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpStandardReturn
            {
                Success = false,
                ReturnCode = "500",
                ReturnMessage = "indisponivel"
            });

        var service = CreateTokenService(configuration, httpClient);

        _ = await service.GetToken();

        httpClient.Verify(x => x.Post(
            It.Is<string>(url => url == ExpectedTokenUrl),
            It.IsAny<object>(),
            It.IsAny<CancellationToken>()), Times.Once);

        Assert.DoesNotContain(
            service.Notifications,
            n => n.Message.Contains(GuardMessageFragment, StringComparison.Ordinal));
    }

    [Fact]
    public async Task GetToken_SemNenhumaCredencial_DeveRetornarNullSemChamarApi()
    {
        var configuration = new Mock<IConfiguration>();
        SetupSection(configuration, "AppConfig:AppURLs:UrlLoginApi", UrlLoginApi);
        SetupSection(configuration, "AppConfig:AppURLs:UrlLoginApiToken", UrlLoginApiTokenPath);

        var httpClient = new Mock<IStandardHttpClient>();

        var service = CreateTokenService(configuration, httpClient);

        var result = await service.GetToken();

        Assert.Null(result);
        httpClient.Verify(x => x.Post(
            It.IsAny<string>(),
            It.IsAny<object>(),
            It.IsAny<CancellationToken>()), Times.Never);
        Assert.Contains(
            service.Notifications,
            n => n.Message.Contains(GuardMessageFragment, StringComparison.Ordinal));
    }

    [Fact]
    public async Task GetToken_ComChavesColonEDuploHifen_DevePriorizarChaveColon()
    {
        var configuration = new Mock<IConfiguration>();
        SetupSection(configuration, "AzureAdOpenID:cc:ClientId", "client-id-colon");
        SetupSection(configuration, "AzureAdOpenID:cc:ClientSecret", "client-secret-colon");
        SetupSection(configuration, "AzureAdOpenID--cc--ClientId", "client-id-literal");
        SetupSection(configuration, "AzureAdOpenID--cc--ClientSecret", "client-secret-literal");
        SetupSection(configuration, "AppConfig:AppURLs:UrlLoginApi", UrlLoginApi);
        SetupSection(configuration, "AppConfig:AppURLs:UrlLoginApiToken", UrlLoginApiTokenPath);

        object capturedBody = null;
        var httpClient = new Mock<IStandardHttpClient>();
        _ = httpClient.Setup(x => x.WithHeader(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(httpClient.Object);
        _ = httpClient.Setup(x => x.Post(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, object, CancellationToken>((_, body, _) => capturedBody = body)
            .ReturnsAsync(new HttpStandardReturn
            {
                Success = false,
                ReturnCode = "500",
                ReturnMessage = "indisponivel"
            });

        var service = CreateTokenService(configuration, httpClient);

        _ = await service.GetToken();

        Assert.NotNull(capturedBody);
        var loginProperty = capturedBody.GetType().GetProperty("Login");
        Assert.NotNull(loginProperty);
        Assert.Equal("client-id-colon", loginProperty.GetValue(capturedBody));
    }
}
