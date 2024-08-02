using Nuuvify.CommonPack.Middleware.Abstraction;
using Microsoft.Extensions.DependencyInjection;

namespace Nuuvify.CommonPack.Middleware
{
    public static class HandlingHeadersMiddlewareSetup
    {

        public static void AddHandlingHeadersMiddlewareSetup(this IServiceCollection services)
        {
            services.AddScoped<RequestConfiguration>();
        }
        public static void AddHandlingHeadersMiddlewareSingletonSetup(this IServiceCollection services)
        {
            services.AddSingleton<RequestConfiguration>();
        }
        public static void AddHandlingHeadersMiddlewareTransientSetup(this IServiceCollection services)
        {
            services.AddTransient<RequestConfiguration>();
        }

    }
}