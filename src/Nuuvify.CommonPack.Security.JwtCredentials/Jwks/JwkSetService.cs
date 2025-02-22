using Nuuvify.CommonPack.Security.JwtCredentials.Interfaces;
using Nuuvify.CommonPack.Security.JwtCredentials.Model;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;


namespace Nuuvify.CommonPack.Security.JwtCredentials.Jwks;

/// <summary>
/// Util class to allow restoring RSA/ECDsa parameters from JSON as the normal
/// parameters class won't restore private key info.
/// </summary>
public class JwkSetService : IJwkSetService
{
    private readonly IJwkStore _store;
    private readonly IJwkService _jwkService;
    private readonly IOptions<JwksOptions> _options;

    public JwkSetService(IJwkStore store,
        IJwkService jwkService,
        IOptions<JwksOptions> options)
    {
        _store = store;
        _jwkService = jwkService;
        _options = options;
    }

    public SigningCredentials Generate(JwksOptions options = null)
    {
        if (options == null)
            options = _options.Value;

        var key = _jwkService.Generate(options.Algorithm);
        var t = new SecurityKeyWithPrivate();

        t.SetParameters(key, options.Algorithm);
        _store.Save(t);

        return new SigningCredentials(key, options.Algorithm);
    }

    /// <summary>
    /// If current doesn't exist will generate new one
    /// </summary>
    public SigningCredentials GetCurrent(JwksOptions options = null)
    {
        if (_store.NeedsUpdate())
        {
            RemovePrivateKeys();
            return Generate(options);
        }

        var currentKey = _store.GetCurrentKey();

        if (!CheckCompatibility(currentKey, options))
            currentKey = _store.GetCurrentKey();

        return currentKey.GetSigningCredentials();
    }


    /// <summary>
    /// According NIST - https://nvlpubs.nist.gov/nistpubs/SpecialPublications/NIST.SP.800-57pt1r4.pdf - Private key should be removed when no longer needs
    /// </summary>
    private void RemovePrivateKeys()
    {
        foreach (var securityKeyWithPrivate in _store.Get(_options.Value.AlgorithmsToKeep))
        {
            securityKeyWithPrivate.SetParameters();
            _store.Update(securityKeyWithPrivate);
        }
    }

    /// <summary>
    /// options has change. Change current key
    /// </summary>
    /// <param name="currentKey"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    private bool CheckCompatibility(SecurityKeyWithPrivate currentKey, JwksOptions options)
    {
        if (options == null)
            options = _options.Value;

        if (currentKey.Algorithm == options.Algorithm) return true;

        Generate(options);
        return false;

    }

    public IReadOnlyCollection<JsonWebKey> GetLastKeysCredentials(int qty)
    {
        var store = _store.Get(qty);
        if (!store.Any())
        {
            GetCurrent();
            return _store.Get(qty).OrderByDescending(o => o.CreationDate).Select(s => s.GetSecurityKey()).ToList().AsReadOnly();
        }

        return store.OrderByDescending(o => o.CreationDate).Select(s => s.GetSecurityKey()).ToList().AsReadOnly();
    }

}
