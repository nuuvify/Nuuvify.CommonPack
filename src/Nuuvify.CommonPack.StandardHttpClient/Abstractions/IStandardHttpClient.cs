using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.StandardHttpClient.Results;

namespace Nuuvify.CommonPack.StandardHttpClient
{

    /// <summary>
    /// Voce deve registrar seus endpoints pelo services.AddHttpClient no projeto de Infra.IoC ou
    /// o projeto onde esta sua lib Http, utilize essa interface no seu projeto de Infra.Data ou 
    /// Infra.ExternalServices, para executar suas comunicação http
    /// </summary>
    public interface IStandardHttpClient
    {


        /// <summary>
        /// Essa propriedade retorna a url completa "absoluta" da sua chamada http,
        /// depois que a mesma ocorrer  
        /// </summary>
        /// <value></value>
        Uri FullUrl { get; }

        /// <summary>
        /// Aqui é possivel obter um codigo ou uma string, que o desenvolvedor usou para rastrear
        /// sua request pelo log, essa propriedade recebe valor pelo metodo WithCurrelationHeader
        /// </summary>
        /// <value></value>
        public string CorrelationId { get; }


        /// <summary>
        /// Se true, um log detalhado sera enviado para o console e seu sistema de logs, <br/>
        /// esse parametro pode ser alterado durante o uso da classe.
        /// </summary>
        /// <value></value>
        public bool LogRequest { get; set; }
        /// <summary>
        /// Retorna authorization incluido no header da request
        /// </summary>
        /// <value></value>
        public string AuthorizationLog { get; }

        /// <summary>
        /// Cria uma nova instancia do HttpClient ou, se informado o nome de um lient já registrado
        /// retonara sua instancia
        /// </summary>
        /// <param name="namedClient"></param>
        void CreateClient(string namedClient = null);


        /// <summary>
        /// Recebe arquivos (stream)
        /// </summary>
        /// <param name="urlRoute"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<HttpStandardStreamReturn> GetStream(
            string urlRoute,
            CancellationToken cancellationToken = default);
        Task<HttpStandardReturn> Get(
            string urlRoute,
            CancellationToken cancellationToken = default);
        Task<HttpStandardReturn> Post(
            string urlRoute,
            object messageBody,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Informe parametro adicional informando o tipo do messageBody
        /// </summary>
        /// <param name="urlRoute"></param>
        /// <param name="messageBody"></param>
        /// <param name="mediaType">exemplo: "application/xml"</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<HttpStandardReturn> Post(
            string urlRoute,
            object messageBody,
            string mediaType,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Parametros para envio de arquivos
        /// </summary>
        /// <param name="urlRoute"></param>
        /// <param name="messageBody"></param>
        /// <param name="mediaType"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<HttpStandardReturn> Post(
            string urlRoute,
            MultipartFormDataContent messageBody,
            string mediaType = "multipart/form-data",
            CancellationToken cancellationToken = default);

        Task<HttpStandardReturn> Put(
            string urlRoute,
            object messageBody,
            CancellationToken cancellationToken = default);
        Task<HttpStandardReturn> Patch(
            string urlRoute,
            object messageBody,
            CancellationToken cancellationToken = default);
        Task<HttpStandardReturn> Delete(
            string urlRoute,
            CancellationToken cancellationToken = default);


        /// <summary>
        /// Limpa definições de Headers, QueryString e autorization
        /// </summary>
        void ResetStandardHttpClient();

        /// <summary>
        /// Utilize isso para rastreabilidade da sua chamada http, isso ira gerar um header
        /// com o parametro CorrelationId e o valor que for informado no parametro
        /// </summary>
        /// <param name="correlationId">Qualquer valor informado nesse campo sera passado na 
        /// request e devolvido no response, sendo incluido em todos os logs gerado
        /// </param>
        /// <returns></returns>
        IStandardHttpClient WithCurrelationHeader(string correlationId);

        /// <summary>
        /// Informe o schema de autenticação, senha, PAT ou token <br/>
        /// Para basic, o metodo ira transformar o parametro "token" (que sera usuario:senha) em base64
        /// <example>
        /// <code>
        ///     
        /// _standardHttpClient.WithAuthorization("Bearer", _tokenService.GetTokenAcessor());
        ///     ou
        /// _standardHttpClient.WithAuthorization("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...");
        ///     Basic
        /// _standardHttpClient.WithAuthorization("Basic", $"{Username}:{UserPassword}");
        ///
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="schema">bearer ou basic</param>
        /// <param name="token">PAT ou "usuario:senha" se for basic. Token para bearer</param>
        /// <param name="userClaim">Inclui um Header na requet <see cref="Constants.UserClaimHeader"/> com o usuario informado, <br/>
        /// isso é utilizado para comunicação com o backend, pois ele usara esse usuario <br/>
        /// durante o savechanges
        /// </param>
        /// <returns></returns>
        IStandardHttpClient WithAuthorization(
            string schema = "Bearer",
            string token = null,
            string userClaim = null);


        /// <summary>
        /// Incluir Headers para a instancia do HttpClient
        /// </summary>
        /// <param name="key">Nome do cabecalho</param>
        /// <param name="value">Valor do cabecalho</param>
        /// <returns></returns>
        IStandardHttpClient WithHeader(string key, object value);

        /// <summary>
        /// Incluir Headers para a instancia do HttpClient
        /// </summary>
        IStandardHttpClient WithHeader(KeyValuePair<string, string> header);

        /// <summary>
        /// Exemplo de uso:
        /// <example>
        /// <code>
        ///     _standardHttpClient.CreateClient("CredentialApi");
        ///     var resultHttp = await _standardHttpClient
        ///         .WithQueryString("procurarTexto", loginId)
        ///         .WithQueryString("campoProcura", "catloginid")
        ///         .WithQueryString("hrStatus", "Any")
        ///         .WithQueryString("empresaCodigo", empresa)
        ///         .Get(url);
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IStandardHttpClient WithQueryString(string key, object value);

        /// <summary>
        /// Altera os parametros do HttpClient
        /// </summary>
        /// <param name="timeOut">Padrão é 00:01:40</param>
        /// <param name="maxResponseContentBufferSize">Padrão é 2 GB</param>
        /// <param name="httpCompletionOption">Como a resposta deve se comportar <see cref="HttpCompletionOption"/> </param>
        void Configure(
            TimeSpan timeOut,
            long maxResponseContentBufferSize = default,
            HttpCompletionOption httpCompletionOption = HttpCompletionOption.Defult);

        /// <summary>
        /// Faz um Dto do HttpResponseMessage do Aspnet para a classe customizada HttpStandardReturn
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        Task<HttpStandardReturn> HandleResponseMessage(HttpResponseMessage response);

        /// <summary>
        /// Faz um Dto do HttpResponseMessage do Aspnet para a classe customizada HttpStandardStreamReturn para Stream
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        Task<HttpStandardStreamReturn> HandleResponseMessageStream(HttpResponseMessage response);
    }

}
