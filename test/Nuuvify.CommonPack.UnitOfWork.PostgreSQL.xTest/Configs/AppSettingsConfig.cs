using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Nuuvify.CommonPack.UnitOfWork.PostgreSQL.xTest
{
    public static class AppSettingsConfig
    {


        public static IConfiguration GetConfig()
        {
            string projectPath = AppDomain.CurrentDomain.BaseDirectory;

            var config = new ConfigurationBuilder()
               .SetBasePath(projectPath)
               .AddJsonFile(Path.Combine(projectPath, "configTest.json"))
               .Build();


            return config;
        }
    }
}
