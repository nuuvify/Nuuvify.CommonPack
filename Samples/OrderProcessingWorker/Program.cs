using OrderProcessingWorker;
using Nuuvify.CommonPack.Middleware.Abstraction;
using Nuuvify.CommonPack.Middleware;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

// Registrar o RequestConfiguration
builder.Services.Configure<RequestConfiguration>(builder.Configuration);
builder.Services.AddSingleton<RequestConfiguration>();

// Registrar o IConfigurationCustom
builder.Services.AddSingleton<IConfigurationCustom>(provider =>
    new ConfigurationCustom(
        provider.GetRequiredService<IConfiguration>(),
        provider.GetRequiredService<IHostEnvironment>(),
        provider.GetRequiredService<IOptions<RequestConfiguration>>()));

// Registrar o Background Service
builder.Services.AddHostedService<OrderProcessingBackgroundService>();

var host = builder.Build();
host.Run();
