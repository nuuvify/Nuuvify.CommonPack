using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Nuuvify.CommonPack.AzureStorage.Abstraction;

namespace Nuuvify.CommonPack.AzureStorage
{

    public class StorageService : IStorageService
    {


        public StorageConfiguration StorageConfiguration
        {
            get
            {
                return _storageConfiguration;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value.ConnectionName) ||
                    string.IsNullOrWhiteSpace(value.BlobContainerName))
                {
                    throw new FieldAccessException($"{nameof(StorageConfiguration)} Cannot be null.");
                }
                else
                {
                    _storageConfiguration = value;
                }

            }
        }


        private StorageConfiguration _storageConfiguration;
        public StorageService(StorageConfiguration storageConfiguration)
        {

            _storageConfiguration = storageConfiguration;
            StorageConfiguration = _storageConfiguration;

        }


        private BlobClient BlobClientInstance(string Id)
        {
            return new BlobClient(StorageConfiguration.ConnectionName,
                StorageConfiguration.BlobContainerName,
                Id);
        } 

        ///<inheritdoc/>
        public async Task<string> AddOrUpdateBlob(IDictionary<string, byte[]> attachments, CancellationToken cancellationToken)
        {

            if (attachments == null || attachments.Count == 0) return null;

            BlobClient _blobClient;

            foreach (var fileBase64 in attachments)
            {
                _blobClient = BlobClientInstance(fileBase64.Key);

                using var ms = new MemoryStream(fileBase64.Value);
                
                await _blobClient.UploadAsync(ms, true, cancellationToken);

            }


            return "File(s) Added success.";

        }


        ///<inheritdoc/>
        public async Task<bool> DeleteBlob(string fileId)
        {
            var _blobClient = BlobClientInstance(fileId);

            return await _blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);

        }


        ///<inheritdoc/>
        public async Task<BlobStorageResult> GetBlobById(IEnumerable<string> attachments)
        {
            if (attachments == null || !attachments.Any()) return null;

            BlobClient _blobClient;
            BlobDownloadInfo blobDownloadInfo;
            byte[] fileBytes;
            var blobResult = new BlobStorageResult();
            bool blobExists;


            foreach (var item in attachments)
            {

                _blobClient = BlobClientInstance(item);

                blobExists = await _blobClient.ExistsAsync();
                if (blobExists)
                {
                    blobDownloadInfo = await _blobClient.DownloadAsync();

                    using var ms = new MemoryStream();

                    await blobDownloadInfo.Content.CopyToAsync(ms);
                    fileBytes = ms.ToArray();
                    ms.Close();

                    if (!blobResult.Blobs.TryGetValue(item, out byte[] value))
                    {
                        blobResult.Blobs.Add(item, fileBytes);
                    }


                }

            }


            return blobResult;

        }

    }


}