using System.Collections.Generic;
using System.Threading.Tasks;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.Notificator;
using Nuuvify.CommonPack.Security.Abstraction;

namespace Nuuvify.CommonPack.StandardHttpClient
{
    public interface ITokenService
    {

        /// <summary>
        /// Obtem o usuario logado. Esse metodo deve ser utilizado no front-end
        /// </summary>
        /// <returns></returns>
        string GetUsername();

        /// <summary>
        /// Obtem o conteudo da classe CredentialToken
        /// Esse metodo não ira obter um novo token, apenas retorna a instancia existente da classe
        /// </summary>
        /// <returns></returns>
        CredentialToken GetActualToken();

        /// <summary>
        /// Obtem um novo token para as credenciais informadas, se não informar as credenciais, 
        /// será usado a chave do appsettings ApisCredentials:Username e Password <br/>
        /// Esse metodo sempre ira obter um novo token a partir de CredentialApi ou <br/>
        /// informe um valor para HttpClientTokenName caso queira outro nome
        /// </summary>
        /// <remarks>
        /// É necessario possuir as seguintes entradas no appsettings: AppConfig:AppURLs:UrlLoginApi e AppConfig:AppURLs:UrlCredentialToken  <br/>
        /// Nunca use esse metodo depois de ResetStandardHttpClient
        /// </remarks>
        /// <param name="login">Login, usuario ou applicação</param>
        /// <param name="password">Senha do usuario ou aplicação</param>
        /// <param name="userClaim">Usuario que esta logado na aplicação, ele sera incluido no Header para a CredentialApi apenas como informativo <see cref="Constants.UserClaimHeader"/> <br/>
        /// Null = Sera utilizado o usuario "Anonymous", se passar string.Empty, não será utilizado nenhum usuario.
        /// </param>
        /// <returns></returns>
        Task<CredentialToken> GetToken(string login = null, string password = null, string userClaim = null);

        /// <summary>
        /// Obtem o token do usuario no contexto atual.
        /// Não utilize esse metodo se sua aplicação for do tipo Worker, Console ou
        /// qualquer outra que não utilize Aspnet Authentication, pois não existira usuario
        /// logado na aplicação.
        /// </summary>
        /// <returns></returns>
        string GetTokenAcessor();

        /// <summary>
        /// Obtem as mensagens de inconsistencias ocorridas dentro da classe
        /// </summary>
        List<NotificationR> Notifications { get; set; }

        /// <summary>
        /// Retorna o nome para uma instancia HttpClient, informe null, caso não queira nomear
        /// </summary>
        /// <param name="httpClientName">Informe um nome para ser utilizado no <br />
        /// TokenService para instancia HttpClient ou Null para instancia sem nome</param>
        /// <returns></returns>
        string HttpClientTokenName(string httpClientName = "CredentialApi");

    }
}