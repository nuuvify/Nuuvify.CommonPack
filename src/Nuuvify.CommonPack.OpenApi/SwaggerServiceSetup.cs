using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Nuuvify.CommonPack.OpenApi
{
    public static class SwaggerServiceSetup
    {

        public static void AddSwaggerSetup(this IServiceCollection services)
        {

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerGenOptionsConfigure>();


            SwaggerGenXmlComments.Configuration(services);
            SwaggerGenSecurity.Configuration(services);
            SwaggerGenJsonIgnore.Configuration(services);

        }
    }
}
