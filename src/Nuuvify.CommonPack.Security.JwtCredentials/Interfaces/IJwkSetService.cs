using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;

namespace Nuuvify.CommonPack.Security.JwtCredentials.Interfaces
{
    public interface IJwkSetService
    {
        SigningCredentials Generate(JwksOptions options = null);

        SigningCredentials GetCurrent(JwksOptions options = null);
        IReadOnlyCollection<JsonWebKey> GetLastKeysCredentials(int qty);

    }
}