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
    [Fact]
    public void AddDotEnvConfiguration_ParsesFirstSeparatorAndKeepsEnvironmentPrecedence()
    {
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        var path = Path.Combine(directory.FullName, ".env");
        File.WriteAllLines(path, new[]
        {
            "Database__Password=dotenv=tail",
            "# ignored",
            "EMPTY=",
            "INVALID"
        });

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
}
