using Nuuvify.CommonPack.Email.Abstraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Nuuvify.CommonPack.Email
{
    public static class EmailSetup
    {

        /// <summary>
        /// Injeta AddScoped{IEmail, Email} e tambem uma instancia de EmailServerConfiguration
        /// conforme as configurações incluidas em seu appsettings.json "EmailConfig:EmailServerConfiguration"
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddEmailSetup(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IEmail, Email>();

            AddEmailServerConfiguration(services, configuration);
        }

        /// <summary>
        /// Injeta AddSingleton{IEmail, Email} e tambem uma instancia de EmailServerConfiguration
        /// conforme as configurações incluidas em seu appsettings.json "EmailConfig:EmailServerConfiguration"
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddEmailSetupSingleton(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IEmail, Email>();

            AddEmailServerConfiguration(services, configuration);

        }

        private static void AddEmailServerConfiguration(IServiceCollection services, IConfiguration configuration)
        {
            var emailServerConfiguration = new EmailServerConfiguration();

            new ConfigureFromConfigurationOptions<EmailServerConfiguration>(
                    configuration.GetSection("EmailConfig:EmailServerConfiguration"))
                        .Configure(emailServerConfiguration);

            services.AddSingleton(emailServerConfiguration);

        }
        
    }
}