using Microsoft.Extensions.DependencyInjection;
using Nuuvify.CommonPack.Middleware.Abstraction;

namespace Nuuvify.CommonPack.Middleware;

public static class HandlingHeadersMiddlewareSetup
{

    public static void AddHandlingHeadersMiddlewareSetup(this IServiceCollection services)
    {
        _ = services.AddScoped<RequestConfiguration>();
    }
    public static void AddHandlingHeadersMiddlewareSingletonSetup(this IServiceCollection services)
    {
        _ = services.AddSingleton<RequestConfiguration>();
    }
    public static void AddHandlingHeadersMiddlewareTransientSetup(this IServiceCollection services)
    {
        _ = services.AddTransient<RequestConfiguration>();
    }

}
