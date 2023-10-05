using Microsoft.IdentityModel.Tokens;

namespace Nuuvify.CommonPack.Security.JwtCredentials.Interfaces
{
    public interface IJwkService
    {
        JsonWebKey Generate(Algorithm algorithm);
    }
}
