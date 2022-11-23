using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using Nuuvify.CommonPack.AzureStorage.Abstraction;
using Nuuvify.CommonPack.Extensions;
using Xunit;

namespace Nuuvify.CommonPack.AzureStorage.xTest
{
    public class StorageTests
    {
        private Mock<IConfiguration> mockIConfigurationCustom;


        [Fact(Skip = "Executar apenas localmente")]
        // [Fact]
        public async Task DeveGravar_e_RecuperarArquivosNoBlobStorage()
        {
            var storageConfiguration = new StorageConfiguration
            {
                BlobContainerName = "sgkqas",
                BlobConnectionName = "StorageKey"
            };


            const string cnnBlob = "MinhaApp_4e1f0c64-f02e-435a-baa7-c78923ad371a";



            mockIConfigurationCustom = new Mock<IConfiguration>();
            mockIConfigurationCustom.Setup(x => x.GetConnectionString(storageConfiguration.BlobConnectionName))
                .Returns(cnnBlob);


            var storage = new StorageService(mockIConfigurationCustom.Object, storageConfiguration);

            var byteFiles = new Dictionary<string, byte[]>();
            var fileData = new FileData();
            var guid = "2d029467-d75e-4680-8dc3-9c86069967f7";
            Console.WriteLine(guid);

            fileData.FileToByteArray("./dotnet.png");
            byteFiles.Add($"{guid}:teste1", fileData.Content);
            fileData.FileToByteArray("./MeuArquivo de teste.txt");
            byteFiles.Add($"{guid}:teste2", fileData.Content);


            var addResult = await storage.AddOrUpdateBlob(byteFiles, default);


            var chaves = byteFiles.Keys.AsEnumerable();
            var blobResult = await storage.GetBlobById(chaves);


            Assert.Equal(expected: 2, actual: blobResult.Blobs.Count());
            Assert.Equal(expected: "File(s) Added success.", actual: addResult);


        }
    }
}