using System;
using System.Net.Http;
using System.Threading.Tasks;
using Nuuvify.CommonPack.Extensions.Notificator;
using Nuuvify.CommonPack.Security.Abstraction;
using Nuuvify.CommonPack.StandardHttpClient.Polly;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;


namespace Nuuvify.CommonPack.StandardHttpClient.xTest.Configs
{
    public class TokenFactory : NotifiableR
    {

        private readonly Mock<IHttpClientFactory> mockFactory;
        private readonly Mock<IConfiguration> mockConfiguration;
        private readonly Mock<IUserAuthenticated> mockUserAuthenticated;
        private readonly IConfiguration Config;

        public TokenFactory()
        {
            mockFactory = new Mock<IHttpClientFactory>();
            mockConfiguration = new Mock<IConfiguration>();
            mockUserAuthenticated = new Mock<IUserAuthenticated>();


            Config = AppSettingsConfig.GetConfig();

        }


        public async Task<string> ObtemTokenValido(string cws = null, string password = null)
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


            if (string.IsNullOrWhiteSpace(cws) || string.IsNullOrWhiteSpace(password))
            {
                cws = Config.GetSection("ApisCredentials:Username")?.Value;
                password = Config.GetSection("ApisCredentials:Password")?.Value;
            }

            var urlCws = Config.GetSection("AppConfig:AppURLs:UrlCredentialApi")?.Value;
            var urlToken = Config.GetSection("AppConfig:AppURLs:UrlCredentialApiToken")?.Value;


            mockConfiguration.Setup(x => x.GetSection("ApisCredentials:Username").Value)
                .Returns(cws);
            mockConfiguration.Setup(x => x.GetSection("ApisCredentials:Password").Value)
                .Returns(password);
            mockConfiguration.Setup(x => x.GetSection("AppConfig:AppURLs:UrlCredentialApiToken").Value)
                .Returns(urlToken);
            mockConfiguration.Setup(x => x.GetSection("AppConfig:AppURLs:UrlCredentialApi").Value)
                .Returns(urlCws);

            var credentialToken = new CredentialToken()
            {
                LoginId = cws,
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

            foreach (var item in tokenService?.Notifications)
            {
                AddNotification(item.Property, item.Message);
            }


            return tokenService.GetActualToken()?.Token;

        }


    }
}