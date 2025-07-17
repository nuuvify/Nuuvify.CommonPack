namespace Nuuvify.CommonPack.Email.xTest;

[Collection(nameof(DataCollection))]
public class EmailTests
{

    private readonly EmailServerConfiguration emailServerConfiguration;
    private Dictionary<string, string> Remetentes { get; set; }
    private readonly IConfiguration config;
    private readonly ConfigureFromConfigurationOptions<EmailServerConfiguration> configEmailServer;

    private readonly EmailConfigFixture _emailConfigFixture;

    public EmailTests(EmailConfigFixture emailConfigFixture)
    {
        _emailConfigFixture = emailConfigFixture;

        config = AppSettingsConfig.GetConfig();

        emailServerConfiguration = new EmailServerConfiguration();

        var remetenteMock1 = new Mock<IConfigurationSection>();
        _ = remetenteMock1.Setup(s => s.Path).Returns("EmailConfig:RemetenteEmail");
        _ = remetenteMock1.Setup(s => s.Key).Returns("cat@zzz.com");
        _ = remetenteMock1.Setup(s => s.Value).Returns("Teste Automatizado");

        var (envEmailUsername, envEmailPassword) = _emailConfigFixture.GetEmailCredential(config);

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

        var emailEnviado = testarEnvio.EnviarAsync(destinatarios, Remetentes, assunto, mensagem);

        Assert.True(emailEnviado.Result.Equals(teste));
        Assert.False(testarEnvio.IsValid());
    }

    [ServerTestFact]
    [Trait("Nuuvify.CommonPack.Email", nameof(Email))]
    public async Task EnviarEmailSemAnexoCorrreto()
    {

        var destinatarios = new Dictionary<string, string>
        {
            { "fulano@gmail.com", "Fulano" },
            { "cicrano@gmail.com", "Cicrano" },
        };
        const string assunto = "Teste da classe de envio de email";
        const string mensagem = "Esse é o corpo do email";

        var testarEnvio = new Email(emailServerConfiguration);

        var emailEnviado = await testarEnvio.EnviarAsync(destinatarios, Remetentes, assunto, mensagem);

        Assert.NotNull(emailEnviado);
        Assert.True(testarEnvio.IsValid());
    }

    [Fact]
    [Trait("Nuuvify.CommonPack.Email", nameof(Email))]
    public async Task EmailComVariosDestinatariosConcatenadosIncorreto()
    {

        var destinatarios = new Dictionary<string, string>
        {
            { "Fulano", "fulano@gmail.com, cicrano@gmail.com" }
        };
        const string assunto = "Teste da classe de envio de email";
        const string mensagem = "Esse é o corpo do email";

        var testarEnvio = new Email(emailServerConfiguration);

        var emailEnviado = await testarEnvio.EnviarAsync(destinatarios, Remetentes, assunto, mensagem);

        Assert.True(emailEnviado.Equals(false));
        Assert.False(testarEnvio.IsValid());

    }

    [ServerTestFact]
    [Trait("Nuuvify.CommonPack.Email", nameof(Email))]
    public async Task EmailComVariosDestinatariosERemetentesConcatenadosCorreto()
    {

        var destinatarios = new Dictionary<string, string>
        {
            { "fulano@gmail.com, cicrano@gmail.com","Fulano" }
        };
        const string assunto = "Teste da classe de envio de email";
        const string mensagem = "Esse é o corpo do email";

        var testarEnvio = new Email(emailServerConfiguration);

        var emailEnviado = await testarEnvio.EnviarAsync(destinatarios, Remetentes, assunto, mensagem);

        Assert.NotNull(emailEnviado);
        Assert.True(testarEnvio.IsValid());

    }

    [Fact]
    [Trait("Nuuvify.CommonPack.Email", nameof(Email))]
    public async Task EmailComVariosDestinatariosERemetentesConcatenadosIncorreto()
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

        var emailEnviado = await testarEnvio.EnviarAsync(destinatarios, remetentes, assunto, mensagem);

        Assert.True(emailEnviado.Equals(teste));
        Assert.False(testarEnvio.IsValid());

    }

    [Fact]
    [Trait("Nuuvify.CommonPack.Email", nameof(Email))]
    public async Task EmailComVariosDestinatariosENenhumRemetenteConcatenadosIncorreto()
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

        var emailEnviado = await testarEnvio.EnviarAsync(destinatarios, remetentes, assunto, mensagem);

        Assert.True(emailEnviado.Equals(teste));
        Assert.False(testarEnvio.IsValid());

    }

    [Fact]
    [Trait("Nuuvify.CommonPack.Email", nameof(Email))]
    public async Task EmailSemDestinatariosENenhumRemetenteConcatenadosDeveSerInvalido()
    {

        var destinatarios = new Dictionary<string, string>();
        var remetentes = new Dictionary<string, string>();

        const string assunto = "Teste da classe de envio de email";
        const string mensagem = "Esse é o corpo do email";
        const bool teste = false;

        var testarEnvio = new Email(emailServerConfiguration);

        var emailEnviado = await testarEnvio.EnviarAsync(destinatarios, remetentes, assunto, mensagem);

        Assert.True(emailEnviado.Equals(teste));
        Assert.False(testarEnvio.IsValid());

    }

    [Fact]
    [Trait("Nuuvify.CommonPack.Email", nameof(Email))]
    public async Task EmailComHostNuloDeveSerInvalido()
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

        var emailEnviado = await testarEnvio.EnviarAsync(destinatarios, remetentes, assunto, mensagem);

        Assert.True(emailEnviado.Equals(teste));

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
        _ = mockSmpt.Setup(s => s.ConnectAsync(config.GetSection("EmailConfig:EmailServerConfigurationFake:ServerHost").Value, 0, MailKit.Security.SecureSocketOptions.Auto, default))
            .Throws<Exception>();

        var testarEnvio = new Email(emailServerConfiguration);

        var emailEnviado = testarEnvio.EnviarAsync(destinatarios, remetentes, assunto, mensagem);

        Assert.True(emailEnviado.Result.Equals(teste));
        _ = Assert.ThrowsAsync<Exception>(() => Task.FromResult(teste));

    }

}
