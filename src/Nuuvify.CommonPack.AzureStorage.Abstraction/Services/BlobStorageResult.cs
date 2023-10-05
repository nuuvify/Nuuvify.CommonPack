using System.Collections.Generic;

namespace Nuuvify.CommonPack.AzureStorage.Abstraction
{

    public class BlobStorageResult
    {


        public BlobStorageResult()
        {
            Blobs = new Dictionary<string, byte[]>();
        }




        /// <summary>
        /// Key: Nome do arquivo, Value: conteudo do arquivo em base64
        /// </summary>
        /// <value></value>
        public virtual IDictionary<string, byte[]> Blobs { get; set; }
        
    }


}