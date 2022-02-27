using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Nuuvify.CommonPack.Extensions.Notificator;
using Nuuvify.CommonPack.Security.Abstraction;
using Nuuvify.CommonPack.StandardHttpClient.Polly;
using Nuuvify.CommonPack.StandardHttpClient.xTest.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Nuuvify.CommonPack.StandardHttpClient.xTest
{
    [Order(2)]
    public class BaseHttpCustomTests : NotifiableR
    {
        private readonly Mock<IHttpClientFactory> mockFactory;
        private readonly Mock<IConfiguration> mockConfiguration;
        private readonly Mock<IUserAuthenticated> mockUserAuthenticated;

        private readonly IConfiguration Config;

        public BaseHttpCustomTests()
        {
            mockFactory = new Mock<IHttpClientFactory>();
            mockConfiguration = new Mock<IConfiguration>();
            mockUserAuthenticated = new Mock<IUserAuthenticated>();


            Config = AppSettingsConfig.GetConfig();
        }




        [Fact, Order(1)]
        public async Task QuandoObterTokenRetornarErroNotificationDeveSerLancada()
        {

            var handler = new HttpClientHandler();
            var client = new HttpClient(handler, true)
            {
                BaseAddress = new Uri(Config.GetSection("AppConfig:AppURLs:UrlCredentialApi")?.Value)
            };
            client.DefaultRequestHeaders.Add("Accept", "application/json");


            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(client);


            var standardClient = new StandardHttpClient(mockFactory.Object, new NullLogger<StandardHttpClient>());


            var username = Config.GetSection("ApisCredentials:Username")?.Value;
            var password = Config.GetSection("ApisCredentials:Password")?.Value;
            var urlusername = Config.GetSection("AppConfig:AppURLs:UrlCredentialApi")?.Value;
            var urlToken = "api/urlfake";


            mockConfiguration.Setup(x => x.GetSection("ApisCredentials:Username").Value)
                .Returns(username);
            mockConfiguration.Setup(x => x.GetSection("ApisCredentials:Password").Value)
                .Returns(password);
            mockConfiguration.Setup(x => x.GetSection("AppConfig:AppURLs:UrlCredentialApiToken").Value)
                .Returns(urlToken);

            var credentialToken = new CredentialToken()
            {
                LoginId = username,
                Password = password,
                Expires = DateTimeOffset.Now.AddMinutes(-10)
            };
            var mockCredentialToken = new Mock<IOptions<CredentialToken>>();
            mockCredentialToken.Setup(ap => ap.Value)
                .Returns(credentialToken);

            mockUserAuthenticated.Setup(_ => _.GetClaimValue(It.IsAny<string>()))
                .Returns("");
            mockUserAuthenticated.Setup(_ => _.Username())
                .Returns("Anonymous");

            var tokenService = new TokenService(mockCredentialToken.Object, standardClient, mockConfiguration.Object, new NullLogger<TokenService>(), mockUserAuthenticated.Object);
            var retorno = await tokenService.GetToken();


            Assert.Null(tokenService.GetActualToken()?.Token);
            Assert.Null(retorno);
            Assert.True(tokenService.Notifications.Count > 0);
        }



        [LocalTestFact, Order(2)]
        public async Task ObtemCadastroPessoaValido()
        {
            var tokenFactory = new TokenFactory();
            var tokenValido = await tokenFactory.ObtemTokenValido();

            var notification = tokenFactory?.Notifications.LastOrDefault();
            Assert.True(string.IsNullOrWhiteSpace(notification?.Message), notification?.Message);


            var urlSynchro = Config.GetSection("AppConfig:AppURLs:UrlSynchroApi")?.Value;

            var handler = new HttpClientHandler();
            var client = new HttpClient(handler, true)
            {
                BaseAddress = new Uri(urlSynchro)
            };
            client.DefaultRequestHeaders.Add("Accept", "application/json");


            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>()))
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
}
