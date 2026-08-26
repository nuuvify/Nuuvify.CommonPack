using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace Nuuvify.CommonPack.OpenApi.xTest;

[Trait("Category", "Unit")]
public class SwaggerCoverageTests
{
    [Fact]
    public void SwaggerInfoModel_CreateInfoForApiVersion_UsesConfiguredMetadata()
    {
        var model = Activator.CreateInstance(typeof(SwaggerInfoModel),
            "Dev Team",
            "dev@example.com",
            "MIT",
            "https://example.com/repo",
            "https://example.com/terms",
            "https://example.com/license");

        Assert.NotNull(model);

        var versionProperty = typeof(SwaggerInfoModel).GetProperty("VersionName");
        Assert.NotNull(versionProperty);
        versionProperty.SetValue(model, "v1");

        var createInfoMethod = typeof(SwaggerInfoModel).GetMethod("CreateInfoForApiVersion");
        Assert.NotNull(createInfoMethod);

        var info = (OpenApiInfo)createInfoMethod.Invoke(model, null)!;

        var developerName = typeof(SwaggerInfoModel).GetProperty("DeveloperName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(model);
        var developerEmail = typeof(SwaggerInfoModel).GetProperty("DeveloperEmail", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(model);
        var urlAppValid = typeof(SwaggerInfoModel).GetProperty("UrlAppValid", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(model);
        var urlTermsValid = typeof(SwaggerInfoModel).GetProperty("UrlTermsValid", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(model);
        var licenseType = typeof(SwaggerInfoModel).GetProperty("LicenseType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(model);
        var urlLicenseValid = typeof(SwaggerInfoModel).GetProperty("UrlLicenseValid", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(model);
        var appVersion = typeof(SwaggerInfoModel).GetProperty("AppVersion", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(model);
        var buildVersion = typeof(SwaggerInfoModel).GetProperty("BuildVersion", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(model);

        Assert.Equal("v1", info.Version);
        Assert.Equal(developerName, info.Contact.Name);
        Assert.Equal(developerEmail, info.Contact.Email);
        Assert.Equal(urlAppValid, info.Contact.Url);
        Assert.Equal(urlTermsValid, info.TermsOfService);
        Assert.Equal($"License Type: {licenseType}", info.License.Name);
        Assert.Equal(urlLicenseValid, info.License.Url);
        Assert.Contains("## OS ##", info.Description);
        Assert.Contains(appVersion?.ToString(), info.Description);
        Assert.Contains(buildVersion?.ToString(), info.Description);
    }

    [Fact]
    public void SwaggerGenXmlComments_XmlCommentsFilePath_ResolvesExistingFiles()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempPath);

        try
        {
            var fileName = "sample.xml";
            var filePath = Path.Combine(tempPath, fileName);
            File.WriteAllText(filePath, "<doc />");

            var resolved = SwaggerGenXmlComments.XmlCommentsFilePath(tempPath, fileName);

            Assert.Equal(filePath, resolved);

            var fallbackPath = Path.Combine(tempPath, "docs");
            Directory.CreateDirectory(fallbackPath);
            var fallbackFile = Path.Combine(fallbackPath, "fallback.xml");
            File.WriteAllText(fallbackFile, "<doc />");

            var resolvedFallback = SwaggerGenXmlComments.XmlCommentsFilePath(tempPath, "fallback.xml");

            Assert.Equal(fallbackFile, resolvedFallback);
        }
        finally
        {
            Directory.Delete(tempPath, recursive: true);
        }
    }

    [Fact]
    public void SwaggerGenOptionsConfigure_Configure_CreatesVersionedDocs()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SwaggerInfo:DesenvolvedorNome"] = "Dev Team",
                ["SwaggerInfo:DesenvolvedorEmail"] = "dev@example.com",
                ["SwaggerInfo:LicencaTipo"] = "MIT",
                ["SwaggerInfo:UrlRepositoryVsts"] = "https://example.com/repository",
                ["SwaggerInfo:TermoDeServico"] = "https://example.com/terms",
                ["SwaggerInfo:LicencaUrl"] = "https://example.com/license"
            })
            .Build();

        var provider = new FakeApiVersionDescriptionProvider(new[]
        {
            new ApiVersionDescription(new ApiVersion(1, 0), "v1")
        });

        var sut = new SwaggerGenOptionsConfigure(provider, config);
        var options = new SwaggerGenOptions();

        sut.Configure(options);

        Assert.True(options.SwaggerGeneratorOptions.SwaggerDocs.ContainsKey("v1"));
        Assert.Equal("v1", options.SwaggerGeneratorOptions.SwaggerDocs["v1"].Version);
    }

    [Fact]
    public void SwaggerServiceSetup_AddSwaggerSetup_RegistersSwaggerConfiguration()
    {
        var services = new ServiceCollection();

        services.AddSwaggerSetup();
        using var provider = services.BuildServiceProvider();

        var configureOptions = provider.GetServices<IConfigureOptions<SwaggerGenOptions>>();
        Assert.NotEmpty(configureOptions);

        var options = provider.GetRequiredService<IOptions<SwaggerGenOptions>>().Value;
        Assert.NotNull(options);
    }

    [Fact]
    public void SwaggerDefaultValues_Apply_UsesObsoleteMessageWhenDeprecated()
    {
        var operation = new OpenApiOperation();
        var method = typeof(DeprecatedSampleController).GetMethod(nameof(DeprecatedSampleController.DeprecatedAction));
        Assert.NotNull(method);

        var context = new OperationFilterContext(new ApiDescription(), null!, null!, method);
        var filter = new SwaggerDefaultValues();

        filter.Apply(operation, context);

        Assert.Equal("Use a newer action instead.", operation.Description);
    }

    [Fact]
    public void SwaggerJsonIgnore_Apply_RemovesIgnoredParameters()
    {
        var operation = new OpenApiOperation
        {
            Parameters = new List<OpenApiParameter>
            {
                new() { Name = "Name", In = ParameterLocation.Query },
                new() { Name = "Secret", In = ParameterLocation.Query },
                new() { Name = "Authorization", In = ParameterLocation.Header }
            }
        };

        var method = typeof(JsonIgnoreSampleController).GetMethod(nameof(JsonIgnoreSampleController.Save));
        Assert.NotNull(method);

        var context = new OperationFilterContext(new ApiDescription(), null!, null!, method);

        var filter = new SwaggerJsonIgnore();
        filter.Apply(operation, context);

        Assert.Contains(operation.Parameters, p => p.Name == "Name");
        Assert.DoesNotContain(operation.Parameters, p => p.Name == "Secret");
        Assert.Contains(operation.Parameters, p => p.Name == "Authorization");
    }

    private sealed class FakeApiVersionDescriptionProvider : IApiVersionDescriptionProvider
    {
        public FakeApiVersionDescriptionProvider(IEnumerable<ApiVersionDescription> versions)
        {
            ApiVersionDescriptions = versions.ToArray();
        }

        public IReadOnlyList<ApiVersionDescription> ApiVersionDescriptions { get; }
    }

    private sealed class DeprecatedSampleController
    {
        [Obsolete("Use a newer action instead.")]
        public void DeprecatedAction() { }
    }

    private sealed class JsonIgnoreSampleController
    {
        public void Save(JsonIgnoreSampleRequest request) { }
    }

    private sealed class JsonIgnoreSampleRequest
    {
        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public string Secret { get; set; } = string.Empty;
    }
}
