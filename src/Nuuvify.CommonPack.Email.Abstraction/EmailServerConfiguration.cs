using System.Security.Authentication;

namespace Nuuvify.CommonPack.Email.Abstraction
{
    /// <summary>
    /// Incluir a tag EmailConfig:EmailServerConfiguration no appsettings.json
    /// Você pode injetar as configurações de email conforme exemplo:
    /// <example>
    /// <code>
    ///     services.AddScoped{IEmail, Email}().Configure{EmailServerConfiguration}(
    ///             configuration.GetSection("EmailConfig:EmailServerConfiguration"));
    /// </code>
    /// </example>
    /// </summary>
    public class EmailServerConfiguration
    {

        public string ServerHost { get; set; }
        public int Port { get; set; }
        public SslProtocols Security { get; set; }
        public string AccountUserName { get; set; }
        public string AccountPassword { get; set; }
        
    }

}