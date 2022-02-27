using Microsoft.Extensions.DependencyInjection;

namespace Nuuvify.CommonPack.Middleware.Handle
{
    public static class GlobalHandlerExceptionSetup
    {
        public static void AddGlobalHandlerExceptionSetup(this IServiceCollection services)
        {

            services.AddScoped<IGlobalHandleException, GlobalHandleException>();

        }
    }
}
