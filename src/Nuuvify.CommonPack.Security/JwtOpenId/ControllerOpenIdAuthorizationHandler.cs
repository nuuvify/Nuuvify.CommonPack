using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Nuuvify.CommonPack.Security.Abstraction;

namespace Nuuvify.CommonPack.Security.JwtOpenId
{
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
                    var catloginid = context.User.Claims
                        .FirstOrDefault(x => x.Type.EndsWith("catloginid", StringComparison.InvariantCultureIgnoreCase))?
                        .Value;

                    if (!string.IsNullOrWhiteSpace(catloginid))
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var cwsRepository = scope.ServiceProvider.GetRequiredService<IUserAccountRepository>();

                            var isMemberOf = cwsRepository.PersonIsMemberOf(catloginid, item.ClaimType).Result;
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
