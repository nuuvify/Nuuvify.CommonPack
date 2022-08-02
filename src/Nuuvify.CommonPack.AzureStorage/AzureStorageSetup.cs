using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuuvify.CommonPack.AzureStorage.Abstraction;

namespace Nuuvify.CommonPack.AzureStorage
{
    public static class AzureStorageSetup
    {


        /// <summary>
        /// É necessario ter as seguintes tags em seu arquivo appsettings.json
        /// <code>
        ///   "ConnectionStrings": {
        ///     "BLABLA": "DefaultEndpointsProtocol=https;AccountName=MyAccount;AccountKey=kwkwkwkwkwkwkwkwkwkwkwkwkwkwkwkwkw/kokokokokokokoko==;EndpointSuffix=core.windows.net",
        ///   },
        ///   "AppConfig": {
        ///     "BlobContainerName": "dafweb-qa"
        ///   }
        /// </code>
        /// </summary>
        public static void AddAzureStorageSetup(this IServiceCollection services,
            IConfiguration configuration, string storageConnectionName, string storageContainerName)
        {

            ConfigurationIsValid(services, configuration, storageConnectionName, storageContainerName);



            services.AddScoped<IStorageService, StorageService>();
            AddStorageConfiguration(services, StorageConnectionNameSection, StorageContainerNameSection);

        }


        ///<inheritdoc cref="AddAzureStorageSetup"/>
        public static void AddAzureStorageSetupSingleton(this IServiceCollection services,
            IConfiguration configuration, string storageConnectionName, string storageContainerName)
        {

            ConfigurationIsValid(services, configuration, storageConnectionName, storageContainerName);


            services.AddSingleton<IStorageService, StorageService>();
            AddStorageConfiguration(services, StorageConnectionNameSection, StorageContainerNameSection);

        }


        private static string StorageConnectionNameSection { get; set; }
        private static IConfigurationSection StorageContainerNameSection { get; set; }


        private static void ConfigurationIsValid(IServiceCollection services,
            IConfiguration configuration, string storageConnectionName, string storageContainerName)
        {

            if (services is null)
                throw new ArgumentNullException(nameof(services), "Your Dependency Injection container is empty.");

            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration), "Your appsettings.* files not found.");

            if (string.IsNullOrWhiteSpace(storageConnectionName))
                throw new ArgumentNullException(nameof(storageConnectionName), "Cannot be null.");

            if (string.IsNullOrWhiteSpace(storageContainerName))
                throw new ArgumentNullException(nameof(storageContainerName), "Cannot be null.");


            StorageConnectionNameSection = configuration.GetConnectionString(storageConnectionName);
            if (string.IsNullOrWhiteSpace(StorageConnectionNameSection))
            {
                throw new ArgumentNullException(nameof(storageConnectionName), "ConnectionString not found in your appsetings file.*");
            }

            StorageContainerNameSection = configuration.GetSection(storageContainerName);
            if (StorageContainerNameSection?.Key is null)
            {
                throw new ArgumentNullException(nameof(storageContainerName), "Key/Value not found in your appsetings file.*");
            }

        }


        private static void AddStorageConfiguration(IServiceCollection services,
            string storageConnectionName,
            IConfigurationSection storageContainerName)
        {
            var storageConfiguration = new StorageConfiguration
            {
                ConnectionName = storageConnectionName,
                BlobContainerName = storageContainerName.Value
            };

            services.AddSingleton(storageConfiguration);

        }

    }

}