using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Nuuvify.CommonPack.Email.Abstraction;
using Nuuvify.CommonPack.Email.xTest.Configs;
using Xunit;
using Xunit.Abstractions;

namespace Nuuvify.CommonPack.Email.xTest
{
    public class EmailTemplateTests
    {

        private EmailServerConfiguration emailServerConfiguration;

        private readonly IConfiguration config;
        private readonly Mock<IConfiguration> configMock;
        private ConfigureFromConfigurationOptions<EmailServerConfiguration> configEmailServer;
        private readonly ITestOutputHelper _outputHelper;


        public EmailTemplateTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            configMock = new Mock<IConfiguration>();

            config = AppSettingsConfig.GetConfig(false);


            emailServerConfiguration = new EmailServerConfiguration();



            var remetenteMock1 = new Mock<IConfigurationSection>();
            remetenteMock1.Setup(s => s.Path).Returns("EmailConfig:RemetenteEmail");
            remetenteMock1.Setup(s => s.Key).Returns("cat@zzz.com");
            remetenteMock1.Setup(s => s.Value).Returns("Teste Automatizado");

            var remetenteMock = new Mock<IConfigurationSection>();
            remetenteMock.Setup(s => s.GetChildren())
                             .Returns(
                                 new List<IConfigurationSection>
                                    { remetenteMock1.Object }
                             );

            configMock.Setup(c => c.GetSection("EmailConfig:RemetenteEmail"))
                      .Returns(remetenteMock.Object);

            configEmailServer = new ConfigureFromConfigurationOptions<EmailServerConfiguration>(
                config.GetSection("EmailConfig:EmailServerConfigurationFake"));

            configEmailServer.Configure(emailServerConfiguration);


        }


        [Fact]
        [Trait("Nuuvify.CommonPack.Email", nameof(Email))]
        public async Task EnviaEmailComTemplateComAnexoMocado()
        {
            const string assunto = "Teste automatizado da classe de envio de email";



            var mockSmpt = new Mock<SmtpClient>();
            mockSmpt.Setup(s => s.ConnectAsync(config.GetSection("EmailConfig:EmailServerConfigurationFake:ServerHost").Value, 0, MailKit.Security.SecureSocketOptions.Auto, default))
                .Returns(Task.FromResult(true));


            var destinatarios = new Dictionary<string, string>
            {
                { "suporte@nuuve.com.br", "Lincoln" }
            };
            var remetentes = new Dictionary<string, string>
            {
                { "juselino@zzz.com", "Juselino" }
            };


            var testarEnvio = new Email(emailServerConfiguration)
            {
                SmtpCustomClient = mockSmpt.Object
            };

            var variables = new Dictionary<string, string>
            {
                { "@numeroAl", $"Number AL: 4321" },
                { "@emissionDate", $"Emission Date: {DateTimeOffset.Now}" },
                { "@fornecedor", $"Supplier Code: H987654" }
            };
            var message = testarEnvio.GetEmailTemplate(variables, Path.Combine(AppSettingsConfig.TemplatePath, "template-email.html"));

            testarEnvio.WithAttachment(Path.Combine(AppSettingsConfig.TemplatePath, "BB - Layout - CNAB240.pdf"), EmailMidiaType.Application, EmailMidiaSubType.Pdf)
                       .WithAttachment(Path.Combine(AppSettingsConfig.TemplatePath, "SOLID.jpeg"), EmailMidiaType.Image, EmailMidiaSubType.Jpg);


            var emailEnviado = await testarEnvio.EnviarAsync(destinatarios, remetentes, assunto, message);



            Assert.True(emailEnviado.Equals(true));
            Assert.True(testarEnvio.IsValid());

        }

        //[LocalTestFact]
        [Fact]
        [Trait("Nuuvify.CommonPack.Email", nameof(Email))]
        public async Task EnviaEmailComTemplateComAnexoReal()
        {
            const string assunto = "Teste automatizado da classe de envio de email";

            var config = AppSettingsConfig.GetConfig(true);


            configEmailServer = new ConfigureFromConfigurationOptions<EmailServerConfiguration>(
                    config.GetSection("EmailConfig:EmailServerConfiguration"));

            configEmailServer.Configure(emailServerConfiguration);

            var envEmailUsername = Environment.GetEnvironmentVariable("EmailAccount".ToUpper());
            var envEmailPassword = Environment.GetEnvironmentVariable("EmailPassword".ToUpper());

            var destinatarios = new Dictionary<string, string>
            {
                { "lzoca00@gmail.com", "Zoca00" }
            };

            var remetentes = new Dictionary<string, string>
            {
                { envEmailUsername, "dotnet teste" }
            };




            emailServerConfiguration.ServerHost = config.GetSection("EmailConfig:EmailServerConfiguration:ServerHost")?.Value;
            emailServerConfiguration.AccountUserName = string.IsNullOrWhiteSpace(emailServerConfiguration.AccountUserName)
                ? envEmailUsername
                : emailServerConfiguration.AccountUserName;
            emailServerConfiguration.AccountPassword = string.IsNullOrWhiteSpace(emailServerConfiguration.AccountPassword)
                ? envEmailPassword
                : emailServerConfiguration.AccountPassword;

            var testarEnvio = new Email(emailServerConfiguration);


            var variables = new Dictionary<string, string>
            {
                { "@numeroAl", $"Number AL: 123456" },
                { "@emissionDate", $"Emission Date: {DateTimeOffset.Now}" },
                { "@fornecedor", $"Supplier Code: G666M1" }
            };
            var message = testarEnvio.GetEmailTemplate(variables, Path.Combine(AppSettingsConfig.TemplatePath, "template-email.html"));

            testarEnvio.WithAttachment(Path.Combine(AppSettingsConfig.TemplatePath, "BB - Layout - CNAB240.pdf"), EmailMidiaType.Application, EmailMidiaSubType.Pdf)
                       .WithAttachment(Path.Combine(AppSettingsConfig.TemplatePath, "SOLID.jpeg"), EmailMidiaType.Image, EmailMidiaSubType.Jpg);

            var emailEnviado = await testarEnvio.EnviarAsync(destinatarios, remetentes, assunto, message);



            Assert.True(emailEnviado.Equals(true));
            Assert.True(testarEnvio.IsValid());

        }


    }

}
