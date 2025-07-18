using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Nuuvify.CommonPack.Extensions.Notificator;
using Nuuvify.CommonPack.Security.Abstraction;
using Nuuvify.CommonPack.StandardHttpClient.Polly;
using Nuuvify.CommonPack.StandardHttpClient.xTest.Fixtures;

namespace Nuuvify.CommonPack.StandardHttpClient.xTest.Configs
{
    public class TokenFactory : NotifiableR
    {

        private readonly Mock<IHttpClientFactory> mockFactory;
        private readonly Mock<IConfiguration> mockConfiguration;
        private readonly Mock<IHttpContextAccessor> mockUserAuthenticated;
        private readonly IConfiguration Config;

        public TokenFactory(IConfiguration config)
        {
            mockFactory = new Mock<IHttpClientFactory>();
            mockConfiguration = new Mock<IConfiguration>();
            mockUserAuthenticated = new Mock<IHttpContextAccessor>();


            Config = config;

        }



        public async Task<string> ObtemTokenValido(string loginId = null, string password = null)
        {
            var urlLogin = Config.GetSection("AppConfig:AppURLs:UrlLoginApi")?.Value;
            var urlToken = Config.GetSection("AppConfig:AppURLs:UrlLoginApiToken")?.Value;


            mockConfiguration.Setup(x => x.GetSection("AzureAdOpenID:cc:ClientId").Value)
                .Returns(loginId);
            mockConfiguration.Setup(x => x.GetSection("AzureAdOpenID:cc:ClientSecret").Value)
                .Returns(password);
            mockConfiguration.Setup(x => x.GetSection("AppConfig:AppURLs:UrlLoginApiToken").Value)
                .Returns(urlToken);
            mockConfiguration.Setup(x => x.GetSection("AppConfig:AppURLs:UrlLoginApi").Value)
                .Returns(urlLogin);

            var credentialToken = new CredentialToken()
            {
                LoginId = loginId,
                Password = password,
                Expires = DateTimeOffset.Now.AddMinutes(-10)
            };
            var mockCredentialToken = new Mock<IOptions<CredentialToken>>();
            mockCredentialToken.Setup(ap => ap.Value)
                .Returns(credentialToken);

            var user = new UserFixture().GetUserPrincipalFake();
            mockUserAuthenticated.Setup(_ => _.HttpContext.User)
                .Returns(user);


            var handler = new HttpClientHandler();
            var client = new HttpClient(handler, true)
            {
                BaseAddress = new Uri(Config.GetSection("AppConfig:AppURLs:UrlLoginApi")?.Value)
            };
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(client);
            var standardClient = new StandardHttpClient(mockFactory.Object, new NullLogger<StandardHttpClient>());


            var tokenService = new TokenService(mockCredentialToken.Object, standardClient, mockConfiguration.Object, new NullLogger<TokenService>(), mockUserAuthenticated.Object);
            await tokenService.GetToken();

            foreach (var item in tokenService?.Notifications)
            {
                AddNotification(item.Property, item.Message);
            }


            return tokenService.GetActualToken()?.Token;

        }


    }
}