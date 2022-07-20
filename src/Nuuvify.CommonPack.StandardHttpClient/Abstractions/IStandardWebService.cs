using System;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using Nuuvify.CommonPack.StandardHttpClient.Results;

namespace Nuuvify.CommonPack.StandardHttpClient
{

    public interface IStandardWebService
    {
        /// <summary>
        /// Essa propriedade retorna a url completa "absoluta" da sua chamada http,
        /// depois que a mesma ocorrer  
        /// </summary>
        /// <value></value>
        Uri FullUrl { get; }

        /// <summary>
        /// Cria uma nova instancia do HttpWebRequest
        /// </summary>
        /// <param name="url">A url é obrigatorio</param>
        void CreateHttp(string url);


        /// <summary>
        /// Recebe uma instancia de WebRequest.CreateHttp(url) onde você pode incluir outras configurações
        /// como Proxy e delegate para verificação de certificados.
        /// </summary>
        /// <param name="httpWebRequest"></param>
        void CreateHttp(HttpWebRequest httpWebRequest);

        /// <summary>
        /// Executa o WebService, retornando o conteudo de forma padronizada
        /// </summary>
        /// <param name="urlRoute">Opcional: Complemento da url base</param>
        /// <param name="method">Obrigatorio: POST, GET</param>
        /// <param name="messageBody">Obrigatorio: XML contendo o SoapEnvelope</param>
        /// <param name="mediaType">Opcional: Tipo do conteudo da mensagem EX: "application/xml"</param>
        /// <returns></returns>
        Task<HttpStandardReturn> RequestSoap(string urlRoute, StandardHttpMethods method, XmlDocument messageBody, string mediaType = "application/xml");


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
        ///         .Get(url);
        /// </code>
        /// </example>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IStandardWebService WithQueryString(string key, object value);


        void Configure(int timeOut);


    }

}
