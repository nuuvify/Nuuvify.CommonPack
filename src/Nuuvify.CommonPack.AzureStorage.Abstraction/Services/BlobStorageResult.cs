namespace Nuuvify.CommonPack.AzureStorage.Abstraction;


public class BlobStorageResult
{

    public BlobStorageResult()
    {
        Blobs = new Dictionary<string, byte[]>();
        StringBlobs = new Dictionary<string, string>();
    }

    /// <summary>
    /// Key: Nome do arquivo, Value: conteudo do arquivo em base64
    /// </summary>
    /// <value></value>
    public virtual IDictionary<string, byte[]> Blobs { get; set; }
    /// <summary>
    /// Key: Nome do arquivo, Value: conteudo do arquivo em texto (Use isso apenas para pequenos arquivos)
    /// </summary>
    /// <value></value>
    public virtual IDictionary<string, string> StringBlobs { get; set; }

}
