
using System.Security.Authentication;

namespace Nuuvify.CommonPack.Middleware.xTest.Entities
{
    /// <summary>
    /// Incluir a tag EmailServerConfiguration no appsettings.json
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