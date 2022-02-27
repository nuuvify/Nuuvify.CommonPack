using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Security.Abstraction;
using Nuuvify.CommonPack.Security.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;

namespace Nuuvify.CommonPack.Security.JwtOpenId
{

    /// <summary>
    /// Inclui a claim <see cref="Constants.UserIsValidToApplication"/> nas claims do usuario autenticado pelo servidor
    /// <para>
    /// Sendo possivel identificar se o usuario autenticado tem ou não permissão para acessar a aplicação <br/>
    /// Esse metodo é chamado automaticamente pelo Aspnet, no momento da autenticação <br/>
    /// </para>
    /// <see href="https://gunnarpeipman.com/aspnet-core-adding-claims-to-existing-identity" />
    /// </summary>
    public class AddRolesClaimsTransformation : IClaimsTransformation
    {
        
        private readonly IUserAccountRepository _cwsRepository;
        private dynamic policyGroups;

        public AddRolesClaimsTransformation(IUserAccountRepository cwsRepository, IConfiguration configuration)
        {
            _cwsRepository = cwsRepository;
            policyGroups = new PolicyGroupsApplication(configuration);
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {

            if (principal.Identity.IsAuthenticated && !principal.IsInRole(Constants.UserIsValidToApplication))
            {

                var clone = principal.Clone();
                var newIdentity = (ClaimsIdentity)clone.Identity;
                var loginId = principal.GetLogin();


                var gruposDoUsuario = await _cwsRepository.GetUserRoles(loginId);
                var isValidAuthenticated = false;



                foreach (KeyValuePair<string, string> item in policyGroups.PolicyGroups)
                {

                    isValidAuthenticated = gruposDoUsuario.Count(x =>
                        x.Group.Equals(item.Value, StringComparison.InvariantCultureIgnoreCase)) > 0;

                    if (isValidAuthenticated) break;

                }

                if (isValidAuthenticated)
                {
                    var claim = new Claim(newIdentity.RoleClaimType, Constants.UserIsValidToApplication);
                    newIdentity.AddClaim(claim);
                }

                return clone;
            }


            return principal;

        }


    }
}