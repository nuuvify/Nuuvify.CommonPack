namespace Nuuvify.CommonPack.AzureStorage.Abstraction
{
    public class StorageConfiguration
    {

        /// <summary>
        /// No Azure Storage, essa propriedade esta localizada em "Access Key"
        /// </summary>
        /// <value></value>
        public string ConnectionName { get; set; }
        public string BlobContainerName { get; set; }


    }


}