using System;
using System.Collections.Generic;
using System.Linq;
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
    public class EmailTests
    {

        private EmailServerConfiguration emailServerConfiguration;
        private Dictionary<string, string> Remetentes { get; set; }
        private readonly IConfiguration config;
        private ConfigureFromConfigurationOptions<EmailServerConfiguration> configEmailServer;



        public EmailTests()
        {


            config = AppSettingsConfig.GetConfig(false);

            emailServerConfiguration = new EmailServerConfiguration();


            var remetenteMock1 = new Mock<IConfigurationSection>();
            remetenteMock1.Setup(s => s.Path).Returns("EmailConfig:RemetenteEmail");
            remetenteMock1.Setup(s => s.Key).Returns("cat@zzz.com");
            remetenteMock1.Setup(s => s.Value).Returns("Teste Automatizado");

            var envEmailUsername = Environment.GetEnvironmentVariable("EMAILACCOUNTUSERNAME");
            Remetentes = new Dictionary<string, string>
            {
                { envEmailUsername, "dotnet teste" }
            };



            configEmailServer = new ConfigureFromConfigurationOptions<EmailServerConfiguration>(
                config.GetSection("EmailConfig:EmailServerConfigurationFake"));


            configEmailServer.Configure(emailServerConfiguration);


        }


        [Fact]
        [Trait("Nuuvify.CommonPack.Email", nameof(Email))]
        public void EmailComRemetenteNuloInvalido()
        {

            var destinatarios = new Dictionary<string, string>
            {
                { "fulano@gmail.com", "Fulano" }
            };




            const string assunto = "Teste da classe de envio de email";
            const string mensagem = "Esse é o corpo do email";

            const bool teste = false;



            var testarEnvio = new Email(emailServerConfiguration);
            //testarEnvio.EmailServerConfiguration = emailServerConfiguration;

            var emailEnviado = testarEnvio.EnviarAsync(destinatarios, null, assunto, mensagem);

            Assert.True(emailEnviado.Result.Equals(teste));
            Assert.False(testarEnvio.IsValid());

        }

        [Fact]
        [Trait("Nuuvify.CommonPack.Email", nameof(Email))]
        public void EmailComEnderecoDoDestinatarioNuloDeveSerInvalido()
        {

            var destinatarios = new Dictionary<string, string>
            {
                { string.Empty, "Fulano" }
            };

            const string assunto = "Teste da classe de envio de email";
            const string mensagem = "Esse é o corpo do email";


            const bool teste = false;


            var testarEnvio = new Email(emailServerConfiguration);
            //testarEnvio.EmailServerConfiguration = emailServerConfiguration;

            var emailEnviado = testarEnvio.EnviarAsync(destinatarios, Remetentes, assunto, mensagem);

            Assert.True(emailEnviado.Result.Equals(teste));
            Assert.False(testarEnvio.IsValid());

        }

        [Fact]
        [Trait("Nuuvify.CommonPack.Email", nameof(Email))]
        public void EnviarEmailSemAnexoIncorreto()
        {

            var destinatarios = new Dictionary<string, string>
            {
                { "Fulano", "fulano@gmail.com" },
                { "Cicrano", "cicrano@gmail.com" },
            };
            const string assunto = "Teste da classe de envio de email";
            const string mensagem = "Esse é o corpo do email";

            const bool teste = false;


            var testarEnvio = new Email(emailServerConfiguration);
            //testarEnvio.EmailServerConfiguration = emailServerConfiguration;

            var emailEnviado = testarEnvio.EnviarAsync(destinatarios, Remetentes, assunto, mensagem);

            Assert.True(emailEnviado.Result.Equals(teste));
            Assert.False(testarEnvio.IsValid());
        }

        [Fact]
        [Trait("Nuuvify.CommonPack.Email", nameof(Email))]
        public void EnviarEmailSemAnexoCorrreto()
        {

            var destinatarios = new Dictionary<string, string>
            {
                { "fulano@gmail.com", "Fulano" },
                { "cicrano@gmail.com", "Cicrano" },
            };
            const string assunto = "Teste da classe de envio de email";
            const string mensagem = "Esse é o corpo do email";



            var testarEnvio = new Email(emailServerConfiguration);
            //testarEnvio.EmailServerConfiguration = emailServerConfiguration;


            var emailEnviado = testarEnvio.EnviarAsync(destinatarios, Remetentes, assunto, mensagem);

            Assert.NotNull(emailEnviado);
            Assert.True(testarEnvio.IsValid());
        }
        [Fact]
        [Trait("Nuuvify.CommonPack.Email", nameof(Email))]
        public void EmailComVariosDestinatariosConcatenadosIncorreto()
        {

            var destinatarios = new Dictionary<string, string>
            {
                { "Fulano", "fulano@gmail.com, cicrano@gmail.com" }
            };
            const string assunto = "Teste da classe de envio de email";
            const string mensagem = "Esse é o corpo do email";


            var testarEnvio = new Email(emailServerConfiguration);
            //testarEnvio.EmailServerConfiguration = emailServerConfiguration;


            var emailEnviado = testarEnvio.EnviarAsync(destinatarios, Remetentes, assunto, mensagem);


            Assert.True(emailEnviado.Result.Equals(false));
            Assert.False(testarEnvio.IsValid());

        }

        [Fact]
        [Trait("Nuuvify.CommonPack.Email", nameof(Email))]
        public void EmailComVariosDestinatariosERemetentesConcatenadosCorreto()
        {

            var destinatarios = new Dictionary<string, string>
            {
                { "fulano@gmail.com, cicrano@gmail.com","Fulano" }
            };
            const string assunto = "Teste da classe de envio de email";
            const string mensagem = "Esse é o corpo do email";


            var testarEnvio = new Email(emailServerConfiguration);
            //testarEnvio.EmailServerConfiguration = emailServerConfiguration;


            var emailEnviado = testarEnvio.EnviarAsync(destinatarios, Remetentes, assunto, mensagem);

            Assert.NotNull(emailEnviado);
            Assert.True(testarEnvio.IsValid());

        }

        [Fact]
        [Trait("Nuuvify.CommonPack.Email", nameof(Email))]
        public void EmailComVariosDestinatariosERemetentesConcatenadosIncorreto()
        {

            var destinatarios = new Dictionary<string, string>
            {
                { "Fulano","fulano@gmail.com, cicrano@gmail.com" }
            };
            var remetentes = new Dictionary<string, string>
            {
                { "Zezito","fulano@gmail.com, cicrano@gmail.com" }
            };
            const string assunto = "Teste da classe de envio de email";
            const string mensagem = "Esse é o corpo do email";
            const bool teste = false;


            var testarEnvio = new Email(emailServerConfiguration);
            //testarEnvio.EmailServerConfiguration = emailServerConfiguration;


            var emailEnviado = testarEnvio.EnviarAsync(destinatarios, remetentes, assunto, mensagem);

            Assert.True(emailEnviado.Result.Equals(teste));
            Assert.False(testarEnvio.IsValid());

        }

        [Fact]
        [Trait("Nuuvify.CommonPack.Email", nameof(Email))]
        public void EmailComVariosDestinatariosENenhumRemetenteConcatenadosIncorreto()
        {

            var destinatarios = new Dictionary<string, string>
            {
                { "fulano@gmail.com, ermenegildo@gmail.com", "Fulano" },
                { "meuemail@gmail.com, cicrano@gmail.com", "Cigrano" }
            };
            var remetentes = new Dictionary<string, string>();

            const string assunto = "Teste da classe de envio de email";
            const string mensagem = "Esse é o corpo do email";
            const bool teste = false;


            var testarEnvio = new Email(emailServerConfiguration);
            //testarEnvio.EmailServerConfiguration = emailServerConfiguration;


            var emailEnviado = testarEnvio.EnviarAsync(destinatarios, remetentes, assunto, mensagem);


            Assert.True(emailEnviado.Result.Equals(teste));
            Assert.False(testarEnvio.IsValid());

        }

        [Fact]
        [Trait("Nuuvify.CommonPack.Email", nameof(Email))]
        public void EmailSemDestinatariosENenhumRemetenteConcatenadosDeveSerInvalido()
        {

            var destinatarios = new Dictionary<string, string>();
            var remetentes = new Dictionary<string, string>();

            const string assunto = "Teste da classe de envio de email";
            const string mensagem = "Esse é o corpo do email";
            const bool teste = false;


            var testarEnvio = new Email(emailServerConfiguration);
            //testarEnvio.EmailServerConfiguration = emailServerConfiguration;


            var emailEnviado = testarEnvio.EnviarAsync(destinatarios, remetentes, assunto, mensagem);


            Assert.True(emailEnviado.Result.Equals(teste));
            Assert.False(testarEnvio.IsValid());

        }

        [Fact]
        [Trait("Nuuvify.CommonPack.Email", nameof(Email))]
        public void EmailComHostNuloDeveSerInvalido()
        {
            var destinatarios = new Dictionary<string, string>
            {
                { "fulano@gmail.com", "Fulano" }
            };
            var remetentes = new Dictionary<string, string>
            {
                { "eu@gmail.com", "Eu" }
            };
            const string assunto = "Teste da classe de envio de email";
            const string mensagem = "Esse é o corpo do email";
            const bool teste = false;



            var testarEnvio = new Email(emailServerConfiguration);
            //testarEnvio.EmailServerConfiguration = emailServerConfiguration;


            var emailEnviado = testarEnvio.EnviarAsync(destinatarios, remetentes, assunto, mensagem);

            Assert.True(emailEnviado.Result.Equals(teste));

        }

        [Fact]
        [Trait("Nuuvify.CommonPack.Email", nameof(Email))]
        public void EmailComHostIncorretoDeveGerarException()
        {
            var destinatarios = new Dictionary<string, string>
            {
                { "fulano@gmail.com", "Fulano" }
            };
            var remetentes = new Dictionary<string, string>
            {
                { "eu@gmail.com", "Eu" }
            };
            const string assunto = "Teste da classe de envio de email";
            const string mensagem = "Esse é o corpo do email";
            const bool teste = false;



            var mockSmpt = new Mock<SmtpClient>();
            mockSmpt.Setup(s => s.ConnectAsync(config.GetSection("EmailConfig:EmailServerConfigurationFake:ServerHost").Value, 0, MailKit.Security.SecureSocketOptions.Auto, default))
                .Throws<Exception>();


            var testarEnvio = new Email(emailServerConfiguration);
            //testarEnvio.EmailServerConfiguration = emailServerConfiguration;


            var emailEnviado = testarEnvio.EnviarAsync(destinatarios, remetentes, assunto, mensagem);

            Assert.True(emailEnviado.Result.Equals(teste));
            Assert.ThrowsAsync<Exception>(() => Task.FromResult(teste));

        }

    }

}
