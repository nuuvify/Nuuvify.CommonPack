using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Nuuvify.CommonPack.Security.Jwt;

public class JwtTokenOptions
{



    public string Subject { get; set; }
    public string Issuer { get; set; }
    public IList<string> Issuers { get; set; }
    public string Audience { get; set; }
    public IList<string> Audiences { get; set; }

    /// <summary>
    /// Data da Criacao do token.
    /// Não expirar antes de 
    /// </summary>
    public DateTimeOffset NotBefore { get; set; } = DateTimeOffset.Now;


    private TimeSpan _validFor;

    /// <summary>
    /// Valido por n horas
    /// </summary>
    public TimeSpan ValidFor
    {
        get
        {
            if (_validFor.TotalSeconds <= 0)
            {
                _validFor = TimeSpan.FromHours(8);
            }
            return _validFor;
        }
        set
        {
            if (value.TotalSeconds <= 0)
            {
                value = TimeSpan.FromHours(8);
            }
            _validFor = value;
        }
    }


    /// <summary>
    /// Data/Hora de expiracao, NotBefore + ValidFor
    /// </summary>
    public DateTimeOffset Expiration
    {
        get
        {
            var expiration = NotBefore.Add(ValidFor);
            return expiration;
        }

    }

    public string JtiGenerator { get; set; } = Guid.NewGuid().ToString();
    public string SecretKey { get; set; }



    public SigningCredentials SigningCredentials()
    {
        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(KeyEncoding()),
            SecurityAlgorithms.HmacSha256);


        if (signingCredentials.Key.KeySize < 288)
            throw new SecurityTokenInvalidSigningKeyException($"Saltkey deve ter pelo meno 32 digitos = {nameof(SigningCredentials)}");


        return signingCredentials;
    }

    public byte[] KeyEncoding()
    {
        if (string.IsNullOrWhiteSpace(SecretKey))
        {
            throw new ArgumentNullException(SecretKey);
        }


        return Encoding.ASCII.GetBytes(SecretKey);
    }

    public SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        if (string.IsNullOrWhiteSpace(SecretKey))
        {
            throw new ArgumentNullException(SecretKey);
        }

        var signingKey =
            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));

        return signingKey;
    }

    public void NewInstance()
    {
        ValidFor = TimeSpan.Zero;
        NotBefore = DateTimeOffset.Now;
    }

}

