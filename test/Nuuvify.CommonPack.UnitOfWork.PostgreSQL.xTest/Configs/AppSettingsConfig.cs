using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Nuuvify.CommonPack.UnitOfWork.PostgreSQL.xTest
{
    public static class AppSettingsConfig
    {


        public static IConfiguration GetConfig()
        {
            string projectPath = AppDomain.CurrentDomain.BaseDirectory
                .Split(new string[] { @"bin\" }, StringSplitOptions.None)[0];

            var config = new ConfigurationBuilder()
               .SetBasePath(projectPath)
               .AddJsonFile(Path.Combine(projectPath, "configTest.json"))
               .Build();


            return config;
        }
    }
}
