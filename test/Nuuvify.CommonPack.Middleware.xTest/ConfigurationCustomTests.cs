using Nuuvify.CommonPack.Middleware.Abstraction;
using Nuuvify.CommonPack.Middleware.xTest.Configs;
using Nuuvify.CommonPack.Middleware.xTest.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Nuuvify.CommonPack.Middleware.xTest
{
    public class ConfigurationCustomTests
    {

        private readonly IConfiguration Config;


        public ConfigurationCustomTests()
        {

            Config = AppSettingsConfig.GetConfig();
        }


        [Fact]
        public void CasoSectionNaoExistaNaoDeveRetornarException()
        {

            var configuration = new ConfigurationCustom(Config, null, null);
            var sectionReturn = configuration.GetSection("Teste");

            Assert.NotNull(sectionReturn);

        }

        [Fact]
        public void SectionValueDeveEncontrarDados()
        {

            var configuration = new ConfigurationCustom(Config, null, null);
            var sectionReturn = configuration.GetSectionValue("ApisCredentials:Username");

            Assert.NotNull(sectionReturn);

        }

        [Fact]
        public void CasoGetChildrenNaoExistaNaoDeveRetornarException()
        {

            var configuration = new ConfigurationCustom(Config, null, null);
            var sectionReturn = configuration.GetChildren("Teste:Teste1");

            Assert.NotNull(sectionReturn);

        }

        [Fact]
        public void ConfigurationOptionsDeveRetornarClasseTipada()
        {
            const string ServerHostActual = "smtp.zoho.com";

            var configuration = new ConfigurationCustom(Config, null, null);
            var emailServerConfiguration = configuration.ConfigurationOptions<EmailServerConfiguration>("EmailConfig:EmailServerConfiguration");

            Assert.Equal(emailServerConfiguration.ServerHost, ServerHostActual);

        }
        [Fact]
        public void CasoGetChildrenExistaDeveRetornarLista()
        {
            var listExpected = 2;

            var configuration = new ConfigurationCustom(Config, null, null);
            var sectionReturn = configuration.GetChildren("EmailConfig:EmailAdministradores");

            Assert.Equal(listExpected, sectionReturn.Count);

        }

        [Fact]
        public void GetSectionComPathLongaDeveRetornadarDictionary()
        {
            var configuration = new ConfigurationCustom(Config, null, null);

            var valorSection = configuration.GetSection("JwtTokenOptions:SecretKey");

            Assert.NotNull(valorSection);
        }

        [Fact]
        public void GetSectionComPathCurtaDeveRetornadarDictionary()
        {
            var configuration = new ConfigurationCustom(Config, null, null);

            var valor = configuration.GetSection("VirtualPath");

            valor.TryGetValue("VirtualPath", out string valorRetorno);

            Assert.Equal("api", valorRetorno);

        }

        [Fact]
        public void ConfigurationCustom_ComRequestConfigurationNull_RetornaCorrelationNull()
        {
            var configuration = new ConfigurationCustom(Config, null, null);

            var valor = configuration.GetCorrelationId();

            Assert.Null(valor);

        }

        [Fact]
        public void ConfigurationCustom_ComRequestConfigurationNotNull_RetornaCorrelationId()
        {
            var request = new RequestConfiguration()
            {
                CorrelationId = "Teste_123456"
            };

            var mockRequestconfiguration = new Mock<IOptions<RequestConfiguration>>();
            mockRequestconfiguration.Setup(s => s.Value)
                .Returns(request);


            var configuration = new ConfigurationCustom(Config, null, mockRequestconfiguration.Object);

            var valor = configuration.GetCorrelationId();

            Assert.Equal(request.CorrelationId, valor);

        }

        [Fact]
        public void ConfigurationCustom_ComRequestConfigurationSemCorrelation_RetornaNull()
        {
            var request = new RequestConfiguration();

            var mockRequestconfiguration = new Mock<IOptions<RequestConfiguration>>();
            mockRequestconfiguration.Setup(s => s.Value)
                .Returns(request);


            var configuration = new ConfigurationCustom(Config, null, mockRequestconfiguration.Object);

            var valor = configuration.GetCorrelationId();

            Assert.Null(valor);

        }


    }
}
