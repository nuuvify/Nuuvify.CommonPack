using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Nuuvify.CommonPack.Security.Abstraction;

namespace Nuuvify.CommonPack.Security.JwtOpenId
{

    /// <summary>
    /// Essa classe customiza a autorização do Aspnet, para casos que o token não venha com <br/>
    /// com role, assim é possivel pesquisar no repositorio IUserAccountRepository, pelo login <br/>
    /// se o mesmo possui acesso as claims informadas em ControllerOpenIdAuthorizationRequirement
    /// </summary>
    public class ControllerOpenIdAuthorizationHandler : AuthorizationHandler<ControllerOpenIdAuthorizationRequirement>
    {

        private readonly IServiceProvider _serviceProvider;
        public ControllerOpenIdAuthorizationHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ControllerOpenIdAuthorizationRequirement requirement)
        {

            var isAuthenticated = context.HasSucceeded;

            if (context.HasSucceeded)
            {
                context.Succeed(requirement);
            }
            else
            {

                foreach (ControllerOpenIdAuthorizationRequirement item in context.PendingRequirements)
                {
                    //FIXME: Incluir "catloginid" como uma propriedade dessa classe onde possa ser informado dinamicamente
                    //no momento do setup dessa classe.

                    var catloginid = context.User.Claims
                        .FirstOrDefault(x => x.Type.EndsWith("catloginid", StringComparison.InvariantCultureIgnoreCase))?
                        .Value;

                    if (!string.IsNullOrWhiteSpace(catloginid))
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var userAccountRepository = scope.ServiceProvider.GetRequiredService<IUserAccountRepository>();

                            var isMemberOf = userAccountRepository.PersonIsMemberOf(catloginid, item.ClaimType).Result;
                            isAuthenticated = isMemberOf;
                            if (isAuthenticated) break;
                        }
                    }

                }


                if (isAuthenticated)
                {
                    foreach (ControllerOpenIdAuthorizationRequirement item in context.Requirements)
                    {
                        context.Succeed(item);
                    }
                }
                else
                {
                    context.Fail();
                }


            }


            return Task.CompletedTask;
        }
    }
}
