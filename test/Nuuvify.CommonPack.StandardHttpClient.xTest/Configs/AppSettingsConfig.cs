using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Nuuvify.CommonPack.StandardHttpClient.xTest
{
    public static class AppSettingsConfig
    {

        public static IConfiguration GetConfig(bool integrationTest)
        {
            string projectPath = AppDomain.CurrentDomain.BaseDirectory;
            if (integrationTest)
            {
                projectPath = GetPathSecret();
            }

            var fileConfig = Path.Combine(projectPath, "configTest.json");

            if (!File.Exists(fileConfig))
            {
                throw new FileNotFoundException($"Arquivo de configuração para teste: {fileConfig} não existe.");
            }

            var config = new ConfigurationBuilder()
               .SetBasePath(projectPath)
               .AddJsonFile(fileConfig)
               .Build();


            return config;
        }

        public static string GetPathSecret()
        {
            string userSecret;
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                userSecret = ".microsoft/usersecrets//nuuvify";
            }
            else
            {
                userSecret = "Microsoft/UserSecrets/Nuuvify";
            }

            var pathSecret = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                userSecret);

            if (Directory.Exists(pathSecret))
            {
                return pathSecret;
            }

            throw new DirectoryNotFoundException($"Pasta: {pathSecret} para o arquivo de configuração configTest.json não existe.");

        }

    }
}
