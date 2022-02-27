using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace Nuuvify.CommonPack.Security.Jwt
{
    public class ControllerCustomAuthorizationRequirement : IAuthorizationRequirement
    {
        public string ClaimType { get; protected set; }
        public IEnumerable<string> ClaimValues { get; protected set; }


        public ControllerCustomAuthorizationRequirement(string claimType, params string[] claimValues)
        {
            ClaimType = claimType;
            ClaimValues = claimValues;
        }

    }
}
