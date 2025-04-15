using System.Xml;
using Nuuvify.CommonPack.StandardHttpClient.Results;

namespace Nuuvify.CommonPack.StandardHttpClient;


public interface IStandardWebService
{
    /// <summary>
    /// Essa propriedade retorna a url completa "absoluta" da sua chamada http,
    /// depois que a mesma ocorrer  
    /// </summary>
    /// <value></value>
    Uri FullUrl { get; }

    /// <summary>
    /// Cria uma nova instancia do HttpClient ou, se informado o nome de um client já registrado
    /// retonara sua instancia
    /// </summary>
    /// <param name="namedClient"></param>
    void CreateClient(string namedClient = null);

    /// <summary>
    /// Cria uma nova instancia do HttpClient
    /// </summary>
    /// <param name="url">A url é obrigatorio</param>
    void CreateHttp(Uri url);

    /// <summary>
    /// Recebe uma instancia de HttpClientHandler onde você pode incluir outras configurações
    /// como Proxy e delegate para verificação de certificados.
    /// </summary>
    /// <param name="httpClientHandler"></param>
    /// <param name="uri"></param>
    void CreateHttp(HttpClientHandler httpClientHandler, Uri uri);

    /// <summary>
    /// Executa uma consulta SOAP
    /// </summary>
    /// <param name="urlRoute">Esse caminho sera concatenado a url principal caso ja tenha sido informado antes, deixe NULL caso não possuir</param>
    /// <param name="soapEnvelopeXml">XML contendo soapenv:Envelope</param>
    /// <param name="accept">"text/xml"</param>
    /// <param name="contentType">"text/xml;charset=\"utf-8\""</param>
    /// <returns></returns>
    Task<HttpStandardXmlReturn> RequestSoap(
        string urlRoute,
        XmlDocument soapEnvelopeXml,
        string accept = "text/xml",
        string contentType = "text/xml;charset=\"utf-8\"");

    /// <summary>
    /// Limpa definições de Headers, QueryString e autorization
    /// </summary>
    void ResetStandardWebService();

    /// <summary>
    /// Utilize isso para rastreabilidade da sua chamada http, isso ira gerar um header
    /// com o parametro CorrelationId e o valor que for informado no parametro
    /// </summary>
    /// <param name="correlationId">Qualquer valor informado nesse campo sera passado na 
    /// request e devolvido no response, sendo incluido em todos os logs gerado
    /// </param>
    /// <returns></returns>
    IStandardWebService WithCurrelationHeader(string correlationId);

    /// <summary>
    /// Incluir Headers para a instancia do HttpClient
    /// </summary>
    /// <param name="key">Nome do cabecalho</param>
    /// <param name="value">Valor do cabecalho</param>
    /// <returns></returns>
    IStandardWebService WithHeader(string key, object value);

    /// <summary>
    /// Exemplo de uso:
    /// </summary>
    /// <example>
    /// <code>
    ///     _standardWebService.CreateHttp(url);
    ///     var resultHttp = await _standardWebService
    ///         .WithQueryString("procurarTexto", loginId)
    ///         .WithQueryString("campoProcura", "catloginid")
    ///         .WithQueryString("hrStatus", "Any")
    ///         .WithQueryString("empresaCodigo", empresa)
    ///         .RequestSoap(url);
    /// </code>
    /// </example>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    IStandardWebService WithQueryString(string key, object value);

    void Configure(int timeOut);

}
