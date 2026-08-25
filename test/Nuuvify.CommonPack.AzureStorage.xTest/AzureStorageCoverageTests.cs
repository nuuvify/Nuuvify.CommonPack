using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Nuuvify.CommonPack.AzureStorage.Abstraction;
using Xunit;

namespace Nuuvify.CommonPack.AzureStorage.xTest;

[Trait("Category", "Unit")]
public class AzureStorageCoverageTests
{
    [Fact]
    public void AzureStorageSetup_AddAzureStorageSetup_WithConfiguration_RegistersStorageServiceAsScoped()
    {
        var services = new ServiceCollection();
        var mockConfig = new Mock<IConfiguration>();
        services.AddSingleton(mockConfig.Object);

        services.AddAzureStorageSetup();

        using var provider = services.BuildServiceProvider();
        var storageService = provider.GetRequiredService<IStorageService>();

        Assert.NotNull(storageService);
        Assert.IsType<StorageService>(storageService);
    }

    [Fact]
    public void AzureStorageSetup_AddAzureStorageSetupSingleton_WithConfiguration_RegistersStorageServiceAsSingleton()
    {
        var services = new ServiceCollection();
        var mockConfig = new Mock<IConfiguration>();
        services.AddSingleton(mockConfig.Object);

        services.AddAzureStorageSetupSingleton();

        using var provider = services.BuildServiceProvider();
        var storageService1 = provider.GetRequiredService<IStorageService>();
        var storageService2 = provider.GetRequiredService<IStorageService>();

        Assert.NotNull(storageService1);
        Assert.NotNull(storageService2);
        Assert.Same(storageService1, storageService2);
    }

    [Fact]
    public void AzureStorageSetup_AddAzureStorageSetupTransient_WithConfiguration_RegistersStorageServiceAsTransient()
    {
        var services = new ServiceCollection();
        var mockConfig = new Mock<IConfiguration>();
        services.AddSingleton(mockConfig.Object);

        services.AddAzureStorageSetupTransient();

        using var provider = services.BuildServiceProvider();
        var storageService1 = provider.GetRequiredService<IStorageService>();
        var storageService2 = provider.GetRequiredService<IStorageService>();

        Assert.NotNull(storageService1);
        Assert.NotNull(storageService2);
        Assert.NotSame(storageService1, storageService2);
    }

    [Fact]
    public void StorageService_BlobConnectionName_SetValid_StoresValue()
    {
        var mockConfig = new Mock<IConfiguration>();
        var storage = new StorageService(mockConfig.Object);

        storage.BlobConnectionName = "MyConnectionString";

        Assert.Equal("MyConnectionString", storage.BlobConnectionName);
    }

    [Fact]
    public void StorageService_BlobConnectionName_SetNull_ThrowsFieldAccessException()
    {
        var mockConfig = new Mock<IConfiguration>();
        var storage = new StorageService(mockConfig.Object);

        var exception = Assert.Throws<FieldAccessException>(() =>
        {
            storage.BlobConnectionName = null;
        });

        Assert.Contains("BlobConnectionName", exception.Message);
        Assert.Contains("Cannot be null", exception.Message);
    }

    [Fact]
    public void StorageService_BlobConnectionName_SetEmpty_ThrowsFieldAccessException()
    {
        var mockConfig = new Mock<IConfiguration>();
        var storage = new StorageService(mockConfig.Object);

        var exception = Assert.Throws<FieldAccessException>(() =>
        {
            storage.BlobConnectionName = "";
        });

        Assert.Contains("BlobConnectionName", exception.Message);
        Assert.Contains("Cannot be null", exception.Message);
    }

    [Fact]
    public void StorageService_BlobConnectionName_SetWhitespace_ThrowsFieldAccessException()
    {
        var mockConfig = new Mock<IConfiguration>();
        var storage = new StorageService(mockConfig.Object);

        var exception = Assert.Throws<FieldAccessException>(() =>
        {
            storage.BlobConnectionName = "   ";
        });

        Assert.Contains("BlobConnectionName", exception.Message);
    }

    [Fact]
    public void StorageService_BlobContainerName_SetValid_StoresValue()
    {
        var mockConfig = new Mock<IConfiguration>();
        var storage = new StorageService(mockConfig.Object);

        storage.BlobContainerName = "MyContainer";

        Assert.Equal("MyContainer", storage.BlobContainerName);
    }

    [Fact]
    public void StorageService_BlobContainerName_SetNull_ThrowsFieldAccessException()
    {
        var mockConfig = new Mock<IConfiguration>();
        var storage = new StorageService(mockConfig.Object);

        var exception = Assert.Throws<FieldAccessException>(() =>
        {
            storage.BlobContainerName = null;
        });

        Assert.Contains("BlobContainerName", exception.Message);
        Assert.Contains("Cannot be null", exception.Message);
    }

    [Fact]
    public void StorageService_BlobContainerName_SetEmpty_ThrowsFieldAccessException()
    {
        var mockConfig = new Mock<IConfiguration>();
        var storage = new StorageService(mockConfig.Object);

        var exception = Assert.Throws<FieldAccessException>(() =>
        {
            storage.BlobContainerName = "";
        });

        Assert.Contains("BlobContainerName", exception.Message);
    }

    [Fact]
    public void StorageService_BlobContainerName_SetWhitespace_ThrowsFieldAccessException()
    {
        var mockConfig = new Mock<IConfiguration>();
        var storage = new StorageService(mockConfig.Object);

        var exception = Assert.Throws<FieldAccessException>(() =>
        {
            storage.BlobContainerName = "   ";
        });

        Assert.Contains("BlobContainerName", exception.Message);
    }

    [Fact]
    public void StorageService_ContentToString_ValidBytes_ReturnsDecodedString()
    {
        var mockConfig = new Mock<IConfiguration>();
        var storage = new StorageService(mockConfig.Object);

        var content = "Hello Azure Storage";
        var bytes = Encoding.UTF8.GetBytes(content);

        var result = storage.ContentToString(bytes);

        Assert.Equal(content, result);
    }

    [Fact]
    public void StorageService_ContentToString_EmptyBytes_ReturnsEmptyString()
    {
        var mockConfig = new Mock<IConfiguration>();
        var storage = new StorageService(mockConfig.Object);

        var result = storage.ContentToString(Array.Empty<byte>());

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void StorageService_ContentToString_SpecialCharacters_ReturnsCorrectString()
    {
        var mockConfig = new Mock<IConfiguration>();
        var storage = new StorageService(mockConfig.Object);

        var content = "São Paulo - Brasil - Ação teste!";
        var bytes = Encoding.UTF8.GetBytes(content);

        var result = storage.ContentToString(bytes);

        Assert.Equal(content, result);
    }

    [Fact]
    public void StorageService_Constructor_AcceptsConfiguration()
    {
        var mockConfig = new Mock<IConfiguration>();

        var storage = new StorageService(mockConfig.Object);

        Assert.NotNull(storage);
    }

    [Fact]
    public void StorageService_BlobStorageResult_InitializesWithEmptyCollections()
    {
        var result = new BlobStorageResult();

        Assert.NotNull(result.Blobs);
        Assert.NotNull(result.StringBlobs);
        Assert.Empty(result.Blobs);
        Assert.Empty(result.StringBlobs);
    }

    [Fact]
    public void StorageService_BlobStorageResult_StoresBlobs()
    {
        var result = new BlobStorageResult();
        var testData = new byte[] { 1, 2, 3, 4, 5 };

        result.Blobs.Add("test-blob", testData);

        Assert.Single(result.Blobs);
        Assert.Equal(testData, result.Blobs["test-blob"]);
    }

    [Fact]
    public void StorageService_BlobStorageResult_StoresStringBlobs()
    {
        var result = new BlobStorageResult();
        var testData = "Test blob content";

        result.StringBlobs.Add("test-string-blob", testData);

        Assert.Single(result.StringBlobs);
        Assert.Equal(testData, result.StringBlobs["test-string-blob"]);
    }
}
