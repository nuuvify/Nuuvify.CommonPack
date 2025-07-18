
namespace Nuuvify.CommonPack.Security.JwtCredentials.Model;

public class JwtCacheToken
{

    public string Id { get; set; }
    public byte[] Value { get; set; }
    public DateTimeOffset ExpiresAtTime { get; set; }
    public long? SlidingExpirationInSeconds { get; set; }
    public DateTimeOffset AbsoluteExpiration { get; set; }


}
