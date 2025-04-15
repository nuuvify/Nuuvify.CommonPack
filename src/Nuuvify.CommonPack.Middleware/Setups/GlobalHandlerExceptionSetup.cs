using Microsoft.Extensions.DependencyInjection;

namespace Nuuvify.CommonPack.Middleware.Handle;

public static class GlobalHandlerExceptionSetup
{
    public static void AddGlobalHandlerExceptionSetup(this IServiceCollection services)
    {

        _ = services.AddScoped<IGlobalHandleException, GlobalHandleException>();

    }
    public static void AddGlobalHandlerExceptionSetupTransient(this IServiceCollection services)
    {

        _ = services.AddTransient<IGlobalHandleException, GlobalHandleException>();

    }
    public static void AddGlobalHandlerExceptionSetupSingleton(this IServiceCollection services)
    {

        _ = services.AddSingleton<IGlobalHandleException, GlobalHandleException>();

    }
}
