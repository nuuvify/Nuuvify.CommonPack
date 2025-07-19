using System.Collections.Generic;
using System.Security.Claims;

namespace Nuuvify.CommonPack.Security.Abstraction
{
    public interface IUserAuthenticated
    {
        /// <summary>
        /// Obtem o login do usuario logado
        /// </summary>
        /// <returns></returns>
        string Username();

        /// <summary>
        /// Informa se existe um usuario logado
        /// </summary>
        /// <returns></returns>
        bool IsAuthenticated();
        /// <summary>
        /// Retorna se o usuario da aplicação esta logado, e obtem o token caso esteja disponivel
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        bool IsAuthenticated(out string token);
        /// <summary>
        /// Verifica nas Claims do usuario autenticado (Principal.Identity), se possui algum dos grupos informado no parametro 
        /// </summary>
        /// <param name="groups">Grupo(s) que deeja verificar se esta na claim do usuario</param>
        /// <returns></returns>
        bool IsAuthorized(params string[] groups);

        /// <summary>
        /// Obtem o conteudo de uma claim, caso exista, caso contrario retorna vazio
        /// </summary>
        /// <param name="claimName">Nome de uma claim que foi gerada com o token</param>
        /// <returns></returns>
        string GetClaimValue(string claimName);

        /// <summary>
        /// Retorna a lista de clains que foi enviado pelo servidor de autenticação da corporação
        /// </summary>
        /// <returns></returns>
        IEnumerable<Claim> GetClaims();

        /// <summary>
        ///  Returns a value that indicates whether the entity (user) represented by this
        ///     claims principal is in the specified role.
        /// </summary>
        /// <param name="role">The role for which to check</param>
        /// <returns>true if claims principal is in the specified role; otherwise, false</returns>
        bool IsInRole(string role);


    }
}
