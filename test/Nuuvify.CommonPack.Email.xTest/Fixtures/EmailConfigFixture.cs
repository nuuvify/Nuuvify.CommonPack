using System;
using Microsoft.Extensions.Configuration;

namespace Nuuvify.CommonPack.Email.xTest.Fixtures
{
    public class EmailConfigFixture
    {



        public (string emailAccount, string emailPassword) GetEmailCredential(IConfiguration config)
        {

            var envEmailUsername = Environment.GetEnvironmentVariable("EmailAccountUserName".ToUpper());
            var envEmailPassword = Environment.GetEnvironmentVariable("EmailAccountPassword".ToUpper());

            envEmailUsername = string.IsNullOrWhiteSpace(envEmailUsername)
                ? config.GetSection("EmailConfig:EmailServerConfiguration:AccountUserName")?.Value
                : envEmailUsername;
            envEmailPassword = string.IsNullOrWhiteSpace(envEmailPassword)
                ? config.GetSection("EmailConfig:EmailServerConfiguration:AccountPassword")?.Value
                : envEmailPassword;

            return (emailAccount: envEmailUsername, emailPassword: envEmailPassword);

        }


    }


}