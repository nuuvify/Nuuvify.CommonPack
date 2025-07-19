using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Nuuvify.CommonPack.Security.Abstraction;

namespace Nuuvify.CommonPack.Security.Helpers
{

    public class UserAuthenticated : IUserAuthenticated
    {

        protected readonly IHttpContextAccessor _accessor;


        public UserAuthenticated(IHttpContextAccessor accessor)
        {
            _accessor = accessor;

        }


        public virtual string Username()
        {
            var username = string.Empty;

            if (IsAuthenticated())
            {
                username = _accessor.HttpContext.User.GetLogin();
            }

            username = string.IsNullOrWhiteSpace(username) ? "Anonymous" : username;
            return username;

        }

        public virtual bool IsAuthenticated()
        {
            if (_accessor.HttpContext == null) return false;
            return _accessor.HttpContext.User.Identity.IsAuthenticated;
        }
        public virtual bool IsAuthenticated(out string token)
        {
            token = "";
            if (_accessor.HttpContext == null) return false;
            var esquemaAutenticacao = _accessor.HttpContext.Request.Headers
                .FirstOrDefault(x => x.Key.Equals("Authorization")).Value;

            foreach (var item in esquemaAutenticacao)
            {
                token = item?.Replace("bearer", "").Replace("Bearer", "").Trim();
            }

            return IsAuthenticated();
        }

        public virtual bool IsAuthorized(params string[] groups)
        {
            var authorized = false;

            if (_accessor.HttpContext == null) return false;
            var currentUser = _accessor.HttpContext.User.GetLogin();

            if (groups?.Count() == 0 || string.IsNullOrWhiteSpace(currentUser))
            {
                return authorized;
            }

            string item = null;
            Claim claim;

            for (int i = 0; i < groups?.Length; i++)
            {
                item = groups[i];

                claim = _accessor.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type.Equals(item, System.StringComparison.InvariantCultureIgnoreCase));

                authorized = claim != null;

                if (authorized) break;
            }

            return authorized;
        }


        public virtual string GetClaimValue(string claimName)
        {
            if (string.IsNullOrWhiteSpace(claimName)) return string.Empty;

            if (_accessor.HttpContext == null) return string.Empty;
            var claim = _accessor.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type.Equals(claimName, System.StringComparison.InvariantCultureIgnoreCase));

            return claim?.Value;
        }

        public virtual IEnumerable<Claim> GetClaims()
        {
            if (_accessor.HttpContext == null) return null;
            return _accessor.HttpContext.User.Claims;
        }

        public virtual bool IsInRole(string role)
        {
            if (_accessor.HttpContext == null) return false;
            return _accessor.HttpContext.User.IsInRole(role);
        }



    }
}
