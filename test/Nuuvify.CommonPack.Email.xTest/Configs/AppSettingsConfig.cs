using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Nuuvify.CommonPack.Email.xTest.Configs
{
    public static class AppSettingsConfig
    {

        public static string ProjectPath { get; private set; }

        public static IConfiguration GetConfig()
        {
            ProjectPath = AppDomain.CurrentDomain.BaseDirectory.Split(new String[] { @"bin\" }, StringSplitOptions.None)[0];

            var config = new ConfigurationBuilder()
               .SetBasePath(ProjectPath)
               .AddJsonFile(Path.Combine(ProjectPath, "configTest.json"))
               .Build();


            return config;
        }
    }
}
