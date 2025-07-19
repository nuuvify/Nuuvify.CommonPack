using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Nuuvify.CommonPack.AzureStorage.Abstraction;

namespace Nuuvify.CommonPack.AzureStorage
{

    public class StorageService : IStorageService
    {


        /// <summary>
        /// No Azure Storage, essa propriedade esta localizada em "Access Key"
        /// </summary>
        /// <value></value>
        public string BlobConnectionName
        {
            get
            {
                return _blobConnectionName;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new FieldAccessException($"{nameof(BlobConnectionName)} Cannot be null.");
                }
                else
                {
                    _blobConnectionName = value;
                }

            }
        }

        public string BlobContainerName
        {
            get
            {
                return _blobContainerName;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new FieldAccessException($"{nameof(BlobContainerName)} Cannot be null.");
                }
                else
                {
                    _blobContainerName = value;
                }

            }
        }



        private string _blobConnectionName;
        private string _blobContainerName;
        private readonly IConfiguration Configuration;
        public StorageService(IConfiguration configuration)
        {
            Configuration = configuration;

        }


        private BlobClient BlobClientInstance(string Id)
        {
            var blobCnn = Configuration.GetConnectionString(BlobConnectionName);

            return new BlobClient(
                blobCnn,
                BlobContainerName,
                Id);
        }


        public virtual async Task<BlobStorageResult> GetAllBlobs()
        {

            var blobCnn = Configuration.GetConnectionString(BlobConnectionName);


            var blobServiceClient = new BlobServiceClient(blobCnn);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(BlobContainerName);

            var blobs = blobContainerClient.GetBlobs(BlobTraits.All, BlobStates.Version);


            var blobFiles = new List<string>();
            foreach (var item in blobs)
            {
                blobFiles.Add(item.Name);
            }


            var files = await GetBlobById(blobFiles);
            return files;
        }

        ///<inheritdoc/>
        public virtual async Task<string> AddOrUpdateBlob(IDictionary<string, byte[]> attachments, CancellationToken cancellationToken)
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
        public virtual async Task<bool> DeleteBlob(string fileId)
        {
            var _blobClient = BlobClientInstance(fileId);

            return await _blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);

        }


        ///<inheritdoc/>
        public virtual async Task<BlobStorageResult> GetBlobById(IEnumerable<string> attachments)
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


        ///<inheritdoc/>
        public virtual async Task<BlobStorageResult> DownloadStreamingAsync(IEnumerable<string> attachments, CancellationToken cancellationToken = default)
        {
            if (attachments == null || !attachments.Any()) return null;

            BlobClient _blobClient;
            BlobDownloadStreamingResult blobDownloadStreamingResult;
            byte[] fileBytes;
            var blobResult = new BlobStorageResult();
            bool blobExists;


            foreach (var item in attachments)
            {

                _blobClient = BlobClientInstance(item);

                blobExists = await _blobClient.ExistsAsync();
                if (blobExists)
                {
                    blobDownloadStreamingResult = await _blobClient.DownloadStreamingAsync(null, cancellationToken);

                    using var ms = new MemoryStream();
                    await blobDownloadStreamingResult.Content.CopyToAsync(ms);
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

        ///<inheritdoc/>
        public virtual async Task<BlobStorageResult> DownloadContentAsync(IEnumerable<string> attachments, CancellationToken cancellationToken = default)
        {

            if (attachments == null || !attachments.Any()) return null;

            BlobClient _blobClient;
            BlobDownloadResult blobDownloadResult;
            var blobResult = new BlobStorageResult();
            bool blobExists;


            foreach (var item in attachments)
            {

                _blobClient = BlobClientInstance(item);

                blobExists = await _blobClient.ExistsAsync();
                if (blobExists)
                {
                    blobDownloadResult = await _blobClient.DownloadContentAsync(cancellationToken);

                    if (!blobResult.StringBlobs.TryGetValue(item, out string value))
                    {
                        blobResult.StringBlobs.Add(item, blobDownloadResult.Content.ToString());
                    }

                }

            }


            return blobResult;


        }

        ///<inheritdoc/>
        public virtual string ContentToString(byte[] bytes)
        {

            var contentString = Encoding.UTF8.GetString(bytes);
            return contentString;

        }
    }


}