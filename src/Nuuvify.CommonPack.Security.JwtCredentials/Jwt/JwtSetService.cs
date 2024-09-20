using Nuuvify.CommonPack.Security.Abstraction;
using Nuuvify.CommonPack.Security.JwtCredentials.Interfaces;

namespace Nuuvify.CommonPack.Security.JwtCredentials.Jwt
{
    public class JwtSetService : IJwtSetService
    {

        private readonly IJwtStore _store;
        public JwtSetService(IJwtStore store)
        {
            _store = store;
        }




        public async Task Clear(string username, CancellationToken cancellationToken = default)
        {
            await _store.Clear(username, cancellationToken);
        }
        public async Task ClearAll(CancellationToken cancellationToken = default)
        {
            await _store.ClearAll(cancellationToken);
        }

        public async Task<CredentialToken> Get(string username, CancellationToken cancellationToken = default)
        {
            return await _store.Get(username, cancellationToken);
        }


        public void Set(string username, CredentialToken tokenResult)
        {
            _store.Set(username, tokenResult);
        }



    }


}