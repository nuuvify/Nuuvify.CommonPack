using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace Nuuvify.CommonPack.Security.JwtOpenId
{
    public class ControllerOpenIdAuthorizationRequirement : IAuthorizationRequirement
    {
        public string ClaimType { get; protected set; }
        public IEnumerable<string> ClaimValues { get; protected set; }


        public ControllerOpenIdAuthorizationRequirement(string claimType, params string[] claimValues)
        {
            ClaimType = claimType;
            ClaimValues = claimValues;
        }

    }
}
