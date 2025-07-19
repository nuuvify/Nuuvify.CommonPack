using Nuuvify.CommonPack.Security.Abstraction;
using Nuuvify.CommonPack.Security.JwtCredentials.Interfaces;

namespace Nuuvify.CommonPack.Security.JwtCredentials.Jwt;

public class JwtSetService : IJwtSetService
{

    private readonly IJwtStore _store;
    public JwtSetService(IJwtStore store)
    {
        _store = store;
    }





    public async Task Clear(string username, string cacheType, CancellationToken cancellationToken = default)
    {

        await _store.Clear(username, cacheType, cancellationToken);
    }

    public async Task ClearAll(string cacheType, CancellationToken cancellationToken = default)
    {

        await _store.ClearAll(cacheType, cancellationToken);
    }

    public async Task<CredentialToken> Get(string username, string cacheType, CancellationToken cancellationToken = default)
    {

        return await _store.Get(username, cacheType, cancellationToken);
    }

    public void Set(string username, CredentialToken tokenResult, string cacheType)
    {

        _store.Set(username, tokenResult, cacheType);
    }



}

