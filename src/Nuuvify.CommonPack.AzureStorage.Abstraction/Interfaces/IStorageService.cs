using System;
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
        public string BlobConnectionName { get; set; }
        public string BlobContainerName { get; set; }


        /// <summary>
        /// Obtem um lista com todos os arquivos de um determinado blob container <br/>
        /// Atualize as proprieades StorageConfiguration.BlobConnectionName e StorageConfiguration.BlobContainerName
        /// </summary>
        /// <returns></returns>
        Task<BlobStorageResult> GetAllBlobs();

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
        /// Esse metodo esta obsoleto, considere usar os metodos:
        ///     • DownloadContentAsync – as a prefered way of downloading small blobs that can fit into memory
        ///     • DownloadStreamingAsync – as a replacement to this API. Use it to access network stream directly for any advanced scenario.
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        [Obsolete("Esse metodo esta obsoleto e sera removido em futura versao.")]
        Task<BlobStorageResult> GetBlobById(IEnumerable<string> attachments);


        /// <summary>
        /// Retorna uma lista de arquivos no formato byte[], esse é o metodo preferencial para baixar arquivos do blob
        /// </summary>
        /// <param name="attachments"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<BlobStorageResult> DownloadStreamingAsync(IEnumerable<string> attachments, CancellationToken cancellationToken = default);

        /// <summary>
        /// Baixa a lista de ID's em formato string (Use esse metodo apenas para pequenos arquivos)
        /// <para>Esse metodo popula a propriedade BlobStorageResult.StringBlobs</para>
        /// </summary>
        /// <param name="attachments">Lista de ID's para baixar</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<BlobStorageResult> DownloadContentAsync(IEnumerable<string> attachments, CancellationToken cancellationToken = default);

        /// <summary>
        /// Id do arquivo gravado no storage, geralmente "prefixo_NomeArquivo"
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        Task<bool> DeleteBlob(string fileId);

        /// <summary>
        /// Converte um byte[] para string, normalmente usado para converter o conteudo de BlobStorageResult.Blobs
        /// <para>ATENÇÃO: Use esse metodo com responsabilidade, pois converter dados grandes pode causar exceções e problemas de performance</para>
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        string ContentToString(byte[] bytes);

    }


}