using System.Collections.Generic;
using Nuuvify.CommonPack.Middleware.Abstraction.Extensions;


namespace Nuuvify.CommonPack.Middleware.Abstraction
{
    /// <summary>
    /// Essa classe é utilizada para armazenar informações de uma request, para ser utilizado pelo Logger
    /// </summary>
    public class RequestConfiguration
    {
        public string AppName { get; set; }
        public string ApplicationVersion { get; set; }
        public string Environment { get; set; }
        public string BuildNumber { get; set; }
        /// <summary>
        /// Conteudo da chave x-user-claim caso for informada no Header de uma request
        /// </summary>
        /// <value></value>
        public string UserClaim { get; set; }

        /// <summary>
        /// Retorna as propriedades ApplicationVersion e BuildNumber concatenadas com hifem
        /// </summary>
        /// <example>1.0.0-2021.11.01.1959</example>
        public string ApplicationRelease
        {
            get
            {
                return $"{ApplicationVersion}-{BuildNumber}";
            }
        }


        /// <summary>
        /// Captura a chave "CorrelationId" enviada no Header de uma request
        /// ou cria um Guid caso esteja null. Essa propriedade é populada por 
        /// HandlingHeadersMiddleware
        /// </summary>
        /// <value></value>
        public string CorrelationId { get; set; }
        public string RemoteIp { get; set; }
        public string RemotePort { get; set; }
        public string LocalIp { get; set; }
        public string LocalPort { get; set; }
        public string BasePath { get; set; }
        public string HostName { get; set; }


        public void SetAppVersion()
        {
            AppName = AssemblyExtension.GetApplicationNameByAssembly;

            BuildNumber = AssemblyExtension.GetApplicationBuildNumber;

            ApplicationVersion = AssemblyExtension.GetApplicationVersion;

        }

        public void SetRequestData(
            string remoteIp,
            string remotePort,
            string localIp,
            string localPort,
            string basePath = "",
            string hostName = "none")
        {
            RemoteIp = remoteIp;
            RemotePort = remotePort;
            LocalIp = localIp;
            LocalPort = localPort;
            if (!string.IsNullOrWhiteSpace(basePath))
            {
                BasePath = basePath;
            }
            HostName ??= hostName;

        }

        public Dictionary<string, object> MapLoggerContext()
        {
            var logHeader = new Dictionary<string, object>
            {
                {nameof(AppName), AppName},
                {nameof(CorrelationId), CorrelationId},
                {nameof(Environment), Environment},
                {nameof(ApplicationVersion), ApplicationVersion},
                {nameof(ApplicationRelease), ApplicationRelease},
                {nameof(RemoteIp), RemoteIp},
                {nameof(RemotePort), RemotePort},
                {nameof(LocalIp), LocalIp},
                {nameof(LocalPort), LocalPort},
                {nameof(UserClaim), UserClaim},
                {nameof(HostName), HostName}
            };

            return logHeader;
        }

    }
}