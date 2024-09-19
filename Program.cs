using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using Startup.Custom;


var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(options =>
    {
        options.IncludeScopes = true;
        // options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss ";
    })
//     .AddConsole()
    .AddConfiguration(builder.Configuration.GetSection("Logging"));





using ILoggerFactory loggerFactory =
    LoggerFactory.Create(config =>
    {
        config.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            // options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
        })
        .AddConfiguration(builder.Configuration.GetSection("Logging"));
        // config.AddConsole()
        // .AddFilter((provider, category, logLevel) =>
        // {

        //     var logLevelConfig = builder.Configuration.GetSection("Logging:Console:LogLevel:Default").Value;
        //     return logLevel >= Enum.Parse<LogLevel>(logLevelConfig);
        // });
        //   .AddColorConsoleLogger();
        // .AddCustomFormatter(configureFormatter =>
        // {
        //     configureFormatter.IncludeScopes = true;
        //     configureFormatter.TimestampFormat = "yyyy-MM-dd HH:mm:ss zzz";
        // });

        // var logLevel = builder.Configuration.GetSection("Logging:LogLevel:Default").Value;
        // if (!string.IsNullOrWhiteSpace(logLevel))
        // {
        //     config.SetMinimumLevel(Enum.Parse<LogLevel>(logLevel));
        // }
    });



ILogger<Program> logger = loggerFactory.CreateLogger<Program>();

using (logger.BeginScope("[Scopo de inicializacao]"))
{
    logger.LogTrace("**** Starting Log Trace ****");
    logger.LogDebug("**** Starting Log Debug ****");
    logger.LogInformation("**** Starting Application Environment: {EnvironmentName} ****", builder.Environment.EnvironmentName);



    var configureProxy = new System.Net.WebProxy();
    configureProxy.Address = new Uri(builder.Configuration.GetSection("AppConfig:AppURLs:UrlProxy").Value);
    configureProxy.BypassProxyOnLocal = true;
    configureProxy.BypassList = new string[] { "cat.com" };

    WebRequest.DefaultWebProxy = configureProxy;


    builder.AddLoadDotEnvBuilder(args, logger: logger);
    builder.AddVaultServiceBuilder(logger);
    builder.AddCacheSetup(logger);
    builder.AddMvcSetup();
    builder.AddHealthCheckServiceBuilder();
    builder.AddRegisterServicesBuilder();


    var testNuget = new NugetCustomManagementPackage();
    await testNuget.DeletePackage(logger, default);

}



var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseConfigurationHealthChecks();

app.MapControllerRoute(
    name: "default",
    pattern: "aspnet/{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();


/// <summary>
/// Documentação; https://github.com/NuGet/Samples/blob/main/NuGetProtocolSamples/Program.cs
/// <p>https://learn.microsoft.com/en-us/nuget/reference/nuget-client-sdk</p>
/// </summary>
public class NugetCustomManagementPackage
{


    public IDictionary<string, string> Packages { get; set; }

    public NugetCustomManagementPackage()
    {

        Packages = new Dictionary<string, string>
        {

            { "Nuuvify.CommonPack.AutoHistory", "2.0.0-preview.24091901" },
            { "Nuuvify.CommonPack.AzureStorage", "2.0.0-preview.24091901" },
            { "Nuuvify.CommonPack.AzureStorage.Abstraction", "2.0.0-preview.24091901" },
            { "Nuuvify.CommonPack.Domain", "2.0.0-preview.24091901" },
            { "Nuuvify.CommonPack.EF.Exceptions.Common", "2.0.0-preview.24091901" },
            { "Nuuvify.CommonPack.EF.Exceptions.Db2", "2.0.0-preview.24091901" },
            { "Nuuvify.CommonPack.EF.Exceptions.Oracle", "2.0.0-preview.24091901" },
            { "Nuuvify.CommonPack.Email", "2.0.0-preview.24091901" },
            { "Nuuvify.CommonPack.Email.Abstraction", "2.0.0-preview.24091901" },
            { "Nuuvify.CommonPack.Extensions", "2.0.0-preview.24091901" },
            { "Nuuvify.CommonPack.HealthCheck", "2.0.0-preview.24091901" },
            { "Nuuvify.CommonPack.Middleware", "2.0.0-preview.24091901" },
            { "Nuuvify.CommonPack.Middleware.Abstraction", "2.0.0-preview.24091901" },
            { "Nuuvify.CommonPack.OpenApi", "2.0.0-preview.24091901" },
            { "Nuuvify.CommonPack.Security", "2.0.0-preview.24091901" },
            { "Nuuvify.CommonPack.Security.Abstraction", "2.0.0-preview.24091901" },
            { "Nuuvify.CommonPack.Security.JwtCredentials", "2.0.0-preview.24091901" },
            { "Nuuvify.CommonPack.Security.JwtStore.Ef", "2.0.0-preview.24091901" },
            { "Nuuvify.CommonPack.UnitOfWork", "2.0.0-preview.24091901" },
            { "Nuuvify.CommonPack.UnitOfWork.Abstraction", "2.0.0-preview.24091901" },
            { "Nuuvify.CommonPack.StandardHttpClient", "2.0.0-preview.24091901" },
        };

    }

    public async Task DeletePackage(Microsoft.Extensions.Logging.ILogger logger, CancellationToken cancellationToken)
    {

        var nugetConfig = new NuGet.Configuration.PackageSource("https://api.nuget.org/v3/index.json");


        SourceCacheContext cacheNuget = new SourceCacheContext();
        SourceRepository repository = Repository.Factory.GetCoreV3(nugetConfig);
        PackageUpdateResource resource = await repository.GetResourceAsync<PackageUpdateResource>();

        string apiKey = "XXXXXXXXX";

        logger.LogInformation("===========> Start delete packages");

        foreach (var item in Packages)
        {

            var result = resource.Delete(
                item.Key,
                item.Value,
                getApiKey: packageSource => apiKey,
                confirm: packageSource => true,
                noServiceEndpoint: false,
                NullLogger.Instance).GetAwaiter();

            result.GetResult();

            logger.LogInformation("Package deleted: {Id} {Version} Result: {Result}",
                item.Key,
                item.Value,
                result.IsCompleted);
        }

        logger.LogInformation("===========> Completed delete packages");


    }

}