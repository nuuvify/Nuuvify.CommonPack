using Microsoft.AspNetCore.Authorization;
using Nuuvify.CommonPack.Extensions.Implementation;

namespace Nuuvify.CommonPack.Security.Jwt;

public class ControllerCustomAuthorizationHandler : AuthorizationHandler<ControllerCustomAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   ControllerCustomAuthorizationRequirement requirement)
    {

        var isAuthenticated = false;

        if (context.HasSucceeded)
        {
            isAuthenticated = true;
        }
        else
        {

            if (requirement.ClaimType.Equals(Constants.UserClaimHeader, System.StringComparison.OrdinalIgnoreCase))
            {
                isAuthenticated = false;
            }
            else
            {
                var claims = context.User.Claims
                    .Where(x => x.Type.Equals(requirement.ClaimType, StringComparison.Ordinal))?
                    .ToList();

                isAuthenticated = claims?.Count > 0;
            }

        }

        if (isAuthenticated)
            context.Succeed(requirement);
        else
            context.Fail();

        return Task.CompletedTask;
    }
}
