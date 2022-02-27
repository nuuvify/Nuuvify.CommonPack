using System;
using Nuuvify.CommonPack.AzureStorage.Abstraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Nuuvify.CommonPack.AzureStorage
{
    public static class AzureStorageSetup
    {


        ///<inheritdoc cref="IStorageService.StorageConfiguration"/>
        public static void AddAzureStorageSetup(this IServiceCollection services,
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


            var storageConnectionNameSection = configuration.GetConnectionString(storageConnectionName);
            if (string.IsNullOrWhiteSpace(storageConnectionNameSection))
            {
                throw new ArgumentNullException(nameof(storageConnectionName), "ConnectionString not found in your appsetings file.*");
            }

            var storageContainerNameSection = configuration.GetSection(storageContainerName);
            if (storageContainerNameSection?.Key is null)
            {
                throw new ArgumentNullException(nameof(storageContainerName), "Key/Value not found in your appsetings file.*");
            }


            services.AddScoped<IStorageService, StorageService>();
            AddStorageConfiguration(services, storageConnectionNameSection, storageContainerNameSection);

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