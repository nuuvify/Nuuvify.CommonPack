using Microsoft.Extensions.DependencyInjection;

namespace Nuuvify.CommonPack.StandardHttpClient.WebServices
{
    public static class StandardWebServiceSetup
    {
        /// <summary>
        /// Inclua esse setup no seu Startup, ele injetara IStandardWebService para ser usado
        /// quando consumir WebServices Soap
        /// </summary>
        /// <param name="services"></param>
        public static void AddStandardWebServiceSetup(this IServiceCollection services)
        {

            services.AddScoped<IStandardWebService, StandardWebService>();

        }

        /// <summary>
        /// Inclua esse setup no seu Startup, ele injetara IStandardWebService para ser usado
        /// quando consumir WebServices Soap
        /// Singleton é para ser usado no caso de Workers ou aplicações sem contexto, como WindowsForms
        /// </summary>
        /// <param name="services"></param>
        public static void AddStandardWebServiceSetupSingleton(this IServiceCollection services)
        {

            services.AddSingleton<IStandardWebService, StandardWebService>();
        }


    }
}
