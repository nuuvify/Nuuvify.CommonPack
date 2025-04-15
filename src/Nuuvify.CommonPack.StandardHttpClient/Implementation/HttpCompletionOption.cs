namespace Nuuvify.CommonPack.StandardHttpClient;

public enum HttpCompletionOption
{
    /// <summary>
    /// A operação deve ser concluída após a leitura de toda a resposta, incluindo o conteúdo.
    /// </summary>
    ResponseContentRead,
    /// <summary>
    /// A operação deve ser concluída assim que uma resposta estiver disponível e os cabeçalhos forem lidos. O conteúdo ainda não foi lido.
    /// </summary>
    ResponseHeadersRead,
    /// <summary>
    /// Não utiliza o parametro na configuração
    /// </summary>
    Defult
}
