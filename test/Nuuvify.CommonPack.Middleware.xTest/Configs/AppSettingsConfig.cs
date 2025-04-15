using Microsoft.Extensions.Configuration;

namespace Nuuvify.CommonPack.Middleware.xTest.Configs;

public static class AppSettingsConfig
{

    private static string ProjectPath { get; set; }
    public static string TemplatePath { get; set; }

    public static IConfiguration GetConfig()
    {

        ProjectPath = GetPathSecret();
        TemplatePath = AppDomain.CurrentDomain.BaseDirectory;

        var fileConfig = Path.Combine(ProjectPath, "configTest.json");

        var config = new ConfigurationBuilder()
           .SetBasePath(ProjectPath)
           .AddJsonFile(fileConfig)
           .Build();

        return config;
    }

    public static string GetPathSecret()
    {
        string userSecret;
        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            userSecret = ".microsoft/usersecrets/nuuvify";
        }
        else
        {
            userSecret = "Microsoft/UserSecrets/Nuuvify";
        }

        var pathSecret = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            userSecret);

        if (DirectoryExists(pathSecret))
        {
            return pathSecret;
        }

        pathSecret = pathSecret.Replace("/.config", "");

        if (DirectoryExists(pathSecret))
        {
            return pathSecret;
        }

        return AppDomain.CurrentDomain.BaseDirectory;

    }

    private static bool DirectoryExists(string pathSecret)
    {
        return Directory.Exists(pathSecret);
    }

}
