using Microsoft.Extensions.DependencyInjection;
using Nuuvify.CommonPack.AzureStorage.Abstraction;

namespace Nuuvify.CommonPack.AzureStorage;

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
    public static void AddAzureStorageSetup(this IServiceCollection services)
    {

        _ = services.AddScoped<IStorageService, StorageService>();

    }

    ///<inheritdoc cref="AddAzureStorageSetup"/>
    public static void AddAzureStorageSetupSingleton(this IServiceCollection services)
    {

        _ = services.AddSingleton<IStorageService, StorageService>();

    }

    ///<inheritdoc cref="AddAzureStorageSetup"/>
    public static void AddAzureStorageSetupTransient(this IServiceCollection services)
    {

        _ = services.AddTransient<IStorageService, StorageService>();

    }

}
