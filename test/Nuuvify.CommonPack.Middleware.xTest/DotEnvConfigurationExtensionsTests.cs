using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Nuuvify.CommonPack.Middleware.Extensions;
using Xunit;

namespace Nuuvify.CommonPack.Middleware.xTest;

[Trait("Category", "Unit")]
public class DotEnvConfigurationExtensionsTests
{
    private static readonly string[] s_dotEnvLines =
    [
        "Database__Password=dotenv=tail",
        "# ignored",
        "EMPTY=",
        "INVALID"
    ];

    [Fact]
    public void AddDotEnvConfiguration_ParsesFirstSeparatorAndKeepsEnvironmentPrecedence()
    {
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        var path = Path.Combine(directory.FullName, ".env");
        File.WriteAllLines(path, s_dotEnvLines);

        try
        {
            var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
            {
                ContentRootPath = directory.FullName
            });
            builder.Configuration.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("Database:Password", "mounted-secret")
            });

            builder.AddDotEnvConfiguration(path);

            Assert.Equal("mounted-secret", builder.Configuration["Database:Password"]);
            Assert.Equal(string.Empty, builder.Configuration["EMPTY"]);
            Assert.Null(Environment.GetEnvironmentVariable("Database__Password"));
        }
        finally
        {
            Directory.Delete(directory.FullName, recursive: true);
        }
    }

    [Fact]
    public void AddDotEnvConfiguration_RequiredFileMissing_Throws()
    {
        var path = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.env");
        var builder = Host.CreateApplicationBuilder();

        Assert.Throws<FileNotFoundException>(() => builder.AddDotEnvConfiguration(path, optional: false));
    }

    [Fact]
    public void AddDotEnvConfiguration_DefaultPathUsesContentRoot()
    {
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        var path = Path.Combine(directory.FullName, ".env");
        File.WriteAllText(path, "DEFAULT_VALUE=loaded");

        try
        {
            var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
            {
                ContentRootPath = directory.FullName
            });

            builder.AddDotEnvConfiguration();

            Assert.Equal("loaded", builder.Configuration["DEFAULT_VALUE"]);
        }
        finally
        {
            Directory.Delete(directory.FullName, recursive: true);
        }
    }
}
