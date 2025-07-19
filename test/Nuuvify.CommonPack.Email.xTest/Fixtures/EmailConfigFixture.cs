using System.Globalization;

namespace Nuuvify.CommonPack.Email.xTest.Fixtures;

public class EmailConfigFixture
{

    public (string emailAccount, string emailPassword) GetEmailCredential(IConfiguration config)
    {

        var envEmailUsername = Environment.GetEnvironmentVariable("EmailAccount".ToUpper(CultureInfo.InvariantCulture));
        var envEmailPassword = Environment.GetEnvironmentVariable("EmailPassword".ToUpper(CultureInfo.InvariantCulture));

        envEmailUsername = string.IsNullOrWhiteSpace(envEmailUsername)
            ? config.GetSection("EmailConfig:EmailServerConfiguration:AccountUserName")?.Value
            : envEmailUsername;
        envEmailPassword = string.IsNullOrWhiteSpace(envEmailPassword)
            ? config.GetSection("EmailConfig:EmailServerConfiguration:AccountPassword")?.Value
            : envEmailPassword;

        return (emailAccount: envEmailUsername, emailPassword: envEmailPassword);

    }

}
