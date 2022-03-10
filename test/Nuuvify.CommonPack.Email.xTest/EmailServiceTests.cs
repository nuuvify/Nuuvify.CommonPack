using System.Collections.Generic;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Nuuvify.CommonPack.Email.Abstraction;
using Nuuvify.CommonPack.Email.xTest.Configs;
using Xunit;

namespace Nuuvify.CommonPack.Email.xTest
{
    public class EmailServiceTests
    {

        private readonly EmailServerConfiguration emailServerConfiguration;
        private readonly IConfiguration config;
        private readonly Mock<IConfiguration> configMock;
        private ConfigureFromConfigurationOptions<EmailServerConfiguration> configEmailServer;

        public EmailServiceTests()
        {

            configMock = new Mock<IConfiguration>();


            config = AppSettingsConfig.GetConfig();


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
        public void EmailComRemetenteNuloInvalido()
        {

            var destinatarios = new Dictionary<string, string>
            {
                { "fulano@gmail.com", "Fulano" }
            };

            const string assunto = "Teste da classe de envio de email";
            const string mensagem = "Esse é o corpo do email";


            EmailService.Teste = false;

            var mockSmpt = new Mock<SmtpClient>();
            mockSmpt.Setup(s => s.ConnectAsync(config.GetSection("EmailConfig:EmailServerConfigurationFake:ServerHost").Value, 0, MailKit.Security.SecureSocketOptions.Auto, default))
                .Returns(Task.FromResult(EmailService.Teste));

            EmailService.MockSmpt = mockSmpt;




            var testarEnvio = new EmailService(emailServerConfiguration);
            var emailEnviado = testarEnvio.EnviarAsync(destinatarios, null, assunto, mensagem);

            Assert.True(emailEnviado.Result.Equals(EmailService.Teste));
            Assert.False(testarEnvio.IsValid());

        }


        [Fact]
        public void EmailComVariosDestinatariosERemetentesConcatenadosCorreto()
        {

            var destinatarios = new Dictionary<string, string>
            {
                { "fulano@gmail.com, cicrano@gmail.com","Fulano" }
            };
            var remetentes = new Dictionary<string, string>
            {
                { "meuemail@zzz.com, geronimo@zzz.com","Giropopis da Silva" }
            };
            const string assunto = "Teste da classe de envio de email";
            const string mensagem = "Esse é o corpo do email";

            EmailService.Teste = true;


            var mockSmpt = new Mock<SmtpClient>();
            mockSmpt.Setup(s => s.ConnectAsync(config.GetSection("EmailConfig:EmailServerConfigurationFake:ServerHost").Value, 0, MailKit.Security.SecureSocketOptions.Auto, default))
                .Returns(Task.FromResult(EmailService.Teste));

            EmailService.MockSmpt = mockSmpt;



            var testarEnvio = new EmailService(emailServerConfiguration);
            var emailEnviado = testarEnvio.EnviarAsync(destinatarios, remetentes, assunto, mensagem);

            Assert.NotNull(emailEnviado);
            Assert.True(testarEnvio.IsValid());

        }

    }
}
