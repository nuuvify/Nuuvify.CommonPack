using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nuuvify.CommonPack.AzureStorage.Abstraction
{

    public interface IStorageService
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
        StorageConfiguration StorageConfiguration { get; set; }



        /// <summary>
        /// <para>Key: Deve conter o Id do arquivo que sera gravado no storage, geralmente "prefixo_NomeArquivo"</para>
        /// <para>Value: Arquivo em base64</para>
        /// <para>Use "FormFileCollectionExtensions.GetFilesBase64" para obter arquivos enviados na Controller</para>
        /// <para>Caso o arquivo já exista, sera substituido</para>
        /// </summary>
        /// <param name="attachments"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> AddOrUpdateBlob(IDictionary<string, byte[]> attachments, CancellationToken cancellationToken = default);

        /// <summary>
        /// Informe uma lista com Id do arquivo gravado no storage, geralmente "prefixo_NomeArquivo"
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        Task<BlobStorageResult> GetBlobById(IEnumerable<string> attachments);

        /// <summary>
        /// Id do arquivo gravado no storage, geralmente "prefixo_NomeArquivo"
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        Task<bool> DeleteBlob(string fileId);

    }


}