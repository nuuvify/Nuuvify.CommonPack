using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Nuuvify.CommonPack.Middleware.xTest.Configs
{
    public static class AppSettingsConfig
    {

        public static IConfiguration GetConfig()
        {
            string projectPath = AppDomain.CurrentDomain.BaseDirectory.Split(new String[] { @"bin\" }, StringSplitOptions.None)[0];

            var config = new ConfigurationBuilder()
               .SetBasePath(projectPath)
               .AddJsonFile(Path.Combine(projectPath, "configTest.json"))
               .Build();


            return config;
        }
    }
}
