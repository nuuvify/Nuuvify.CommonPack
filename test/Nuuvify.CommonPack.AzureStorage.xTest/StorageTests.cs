using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nuuvify.CommonPack.AzureStorage;
using Nuuvify.CommonPack.AzureStorage.Abstraction;
using Xunit;

namespace Nuuvify.CommonPack.AzureStorage.xTest
{
    public class StorageTests
    {
        [Fact]
        public async Task TestName()
        {
            var configuration = new StorageConfiguration
            {
                BlobContainerName = "sgkqas",
                ConnectionName = "DefaultEndpointsProtocol=https;AccountName=sgkqas;AccountKey=REDACTED_AZURE_STORAGE_ACCOUNT_KEY;EndpointSuffix=core.windows.net"
            };

            var storage = new StorageService(configuration);


            var byteFiles = new Dictionary<string, byte[]>();
            byteFiles.Add("teste1", );

            var addResult = await storage.AddOrUpdateBlob(byteFiles, default);




            var files = new string[] { "", "" };
            var blobResult = await storage.GetBlobById(files);


        }
    }
}