using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Nuuvify.CommonPack.Middleware.Abstraction;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

namespace Nuuvify.CommonPack.NLog.CustomNLogConfigs
{
    public static class NLogConfigureLoggingCustom
    {

        public static Logger NLogger { get; set; }
        public static string PathNLogConfig { get; set; }



        /// <summary>
        /// Retorna a Path onde o executavel esta sendo executado. Util para ser utilizado
        /// quando estiver utilizando Log em uma aplicação Worker ou Console
        /// </summary>
        /// <returns></returns>
        public static string GetHostBasePath(string environmentName)
        {
            if (environmentName is null)
            {
                throw new ArgumentNullException(nameof(environmentName));
            }


            var executablePath = Process.GetCurrentProcess().MainModule.FileName;
            var executable = Path.GetFileNameWithoutExtension(executablePath);
            string basePath;

            if ("dotnet".Equals(executable, StringComparison.InvariantCultureIgnoreCase))
            {
                basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }
            else
            {
                basePath = Path.GetDirectoryName(executablePath);
            }


            return basePath;

        }


        /// <summary>
        /// Configura o Log como o arquivo nlog.config, essa configuração espera que esse arquivo esteja 
        /// na mesma pasta onde o executavel esta.
        /// Também popula informações de versão do seu assembler em RequestConfiguration
        /// </summary>
        /// <remarks>
        /// Esse metodo também armazena a pasta de execução da aplicação em RequestConfiguration.BasePath
        /// </remarks>
        /// <param name="loggingBuilder"></param>
        /// <param name="environmentName">Nome do ambiente onde a aplicação esta sendo executada</param>
        /// <param name="autoShutdown">Gets or sets a value indicating whether to automatically call NLog.LogManager.Shutdown
        ///  on AppDomain.Unload or AppDomain.ProcessExit</param>
        /// <param name="consoleLog"></param>
        /// <returns></returns>
        public static Logger SetNLog(ILoggingBuilder loggingBuilder, string environmentName, bool autoShutdown = true, bool consoleLog = false)
        {

            BuilderNLog(loggingBuilder, Path.Combine(GetHostBasePath(environmentName), "nlog.config"), consoleLog);

            LogManager.AutoShutdown = autoShutdown;
            NLogger = LogManager.GetCurrentClassLogger();

            return NLogger;

        }

        /// <summary>
        /// Faz as configuração descritas em SetNLog(ILoggingBuilder loggingBuilder), porém utilizando
        ///  
        /// RequestConfiguration.AppName = hostBuilderContext.HostingEnvironment.ApplicationName;
        /// RequestConfiguration.Environment = hostBuilderContext.HostingEnvironment.EnvironmentName;
        /// </summary>
        /// <remarks>
        /// Esse metodo também armazena a pasta de execução da aplicação em RequestConfiguration.BasePath
        /// </remarks>
        /// <param name="hostBuilderContext"></param>
        /// <param name="loggingBuilder"></param>
        /// <param name="autoShutdown">Gets or sets a value indicating whether to automatically call NLog.LogManager.Shutdown
        ///  on AppDomain.Unload or AppDomain.ProcessExit</param>
        /// <param name="consoleLog"></param>
        /// <returns></returns>
        public static Logger SetNLog(HostBuilderContext hostBuilderContext, ILoggingBuilder loggingBuilder, bool autoShutdown = true, bool consoleLog = false)
        {

            BuilderNLog(loggingBuilder, Path.Combine(hostBuilderContext.HostingEnvironment.ContentRootPath, "nlog.config"), consoleLog);

            LogManager.AutoShutdown = autoShutdown;
            NLogger = LogManager.GetCurrentClassLogger();

            return NLogger;
        }

        private static void BuilderNLog(ILoggingBuilder loggingBuilder, string nlogConfig, bool consoleLog = false)
        {
            PathNLogConfig = nlogConfig;

            loggingBuilder.AddNLog(nlogConfig);
            loggingBuilder.ClearProviders();
            if (consoleLog) loggingBuilder.AddConsole();

        }

        /// <summary>
        /// Executa MappedDiagnosticsLogicalContext.Set(chave, valor) para introduzir valor nas variaveis
        /// mldc: contidas no arquivo nlog.config
        /// </summary>
        public static void CustomMappedDiagnosticsLogicalContext(
            string appName,
            string correlationId,
            string environment,
            string applicationVersion,
            string buildNumber,
            string remoteIp,
            string remotePort,
            string localIp,
            string localPort)
        {

            MappedDiagnosticsLogicalContext.Set(
                nameof(RequestConfiguration.AppName),
                appName);

            MappedDiagnosticsLogicalContext.Set(
                nameof(RequestConfiguration.CorrelationId),
                correlationId);

            MappedDiagnosticsLogicalContext.Set(
                nameof(RequestConfiguration.Environment),
                environment);

            MappedDiagnosticsLogicalContext.Set(
                nameof(RequestConfiguration.ApplicationVersion),
                applicationVersion);

            MappedDiagnosticsLogicalContext.Set(
                nameof(RequestConfiguration.BuildNumber),
                buildNumber);

            MappedDiagnosticsLogicalContext.Set(
                nameof(RequestConfiguration.RemoteIp),
                remoteIp);

            MappedDiagnosticsLogicalContext.Set(
                nameof(RequestConfiguration.RemotePort),
                remotePort);

            MappedDiagnosticsLogicalContext.Set(
                nameof(RequestConfiguration.LocalIp),
                localIp);

            MappedDiagnosticsLogicalContext.Set(
                nameof(RequestConfiguration.LocalPort),
                localPort);

        }


    }
}