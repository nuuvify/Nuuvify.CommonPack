using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Nuuvify.CommonPack.Security.Abstraction;
using Nuuvify.CommonPack.Security.JwtCredentials.Interfaces;

namespace Nuuvify.CommonPack.Security.JwtCredentials.Jwt;

/// <summary>
/// Para gerar um token, após validar as credenciais do seu usuario, execute essa classe da seguinte forma:
/// <example>
/// <code>
///     private readonly IOptions{JwksOptions} jwksOptions;
/// 
///     var token = new JwtService()
///        .WithJwtOptions(jwksOptions.Value)
///        .WithJwtUserClaims(usuarioGrupo)
///        .GetUserToken();
/// </code>
/// </example>
/// </summary>
public class JwtService
{

    private JwksOptions _jwksOptions;
    private ICollection<Claim> _jwtClaims;
    private ClaimsIdentity _identityClaims;
    private readonly IJwkSetService _jwksService;

    public JwtService(IJwkSetService jwksService)
    {
        _jwksService = jwksService;
    }

    private static long ToUnixEpochDate(DateTime date)
        => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
            .TotalSeconds);

    private void ThrowIfInvalidOptions(JwksOptions jwksOptions)
    {

        if (jwksOptions is null)
            throw new ArgumentNullException(nameof(jwksOptions), "Objeto não pode ser null");

        if (jwksOptions.ValidFor.TotalMinutes <= 0)
            throw new NotSupportedException($"O período deve ser maior que zero {nameof(jwksOptions.ValidFor)}");

        if (jwksOptions.JtiGenerator is null)
            throw new ArgumentNullException(nameof(jwksOptions), "JtiGenerator - Propriedade não pode ser nulo");

        _jwksOptions = jwksOptions;

    }

    /// <summary>
    /// Use esse metodo primeiro, após receber IOptions{JwksOptions} em seu metodo de chamada
    /// </summary>
    /// <param name="jwksOptions"></param>
    /// <returns></returns>
    public JwtService WithJwtOptions(JwksOptions jwksOptions)
    {
        ThrowIfInvalidOptions(jwksOptions);

        return this;
    }

    /// <summary>
    /// Use esse metodo após o metodo WithJwtUserClaims(), caso queira inserir Informações padrões do Token
    /// como JwtRegisteredClaimNames.Nbf , JwtRegisteredClaimNames.Iat (essas informações já são passadas pela
    /// classe  WithJwtUserClaims(), isso ira gerar outra claim com essas informações )
    /// </summary>
    /// <returns></returns>
    public JwtService WithJwtClaims()
    {
        _jwtClaims ??= new List<Claim>();

        _jwtClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        _jwtClaims.Add(new Claim(JwtRegisteredClaimNames.Nbf,
            ToUnixEpochDate(DateTime.Now).ToString()));
        _jwtClaims.Add(new Claim(JwtRegisteredClaimNames.Iat,
            ToUnixEpochDate(DateTime.Now).ToString(), ClaimValueTypes.Integer64));

        _identityClaims.AddClaims(_jwtClaims);

        return this;
    }

    /// <summary>
    /// Esse metodo é utilizado após o WithJwtOptions()
    /// É obrigatorio que o PersonWithRolesQueryResult.Login esteja preenchido
    /// </summary>
    /// <param name="personRoles">Classe obtida pelo seu metodo que autenticou o usuario/senha</param>
    /// <returns></returns>
    public JwtService WithJwtUserClaims(PersonWithRolesQueryResult personRoles)
    {

        if (string.IsNullOrWhiteSpace(personRoles?.Login))
        {
            throw new ArgumentNullException(nameof(personRoles), "Login do usuario não pode ser nulo");
        }

        _jwtClaims = new List<Claim>();
        _identityClaims = new ClaimsIdentity();

        var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, personRoles?.Name ?? ""),
                new Claim(ClaimTypes.NameIdentifier, personRoles.Login)
            };
        _identityClaims.AddClaims(userClaims);

        var grupos = personRoles?.Groups.ToList();
        for (int i = 0; i < grupos.Count; i++)
        {
            _identityClaims.AddClaim(new Claim(grupos[i].Group, i.ToString()));
        }

        return this;
    }

    public ClaimsIdentity GetClaimsIdentity()
    {
        return _identityClaims;
    }

    /// <summary>
    /// Esse metodo gera um JwtToken, e retorno o mesmo como string
    /// </summary>
    /// <returns>Retorna um token como string</returns>
    public string BuildToken(out DateTimeOffset created, out DateTimeOffset expires)
    {
        created = _jwksOptions.NotBefore;
        expires = _jwksOptions.Expiration;

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _jwksOptions.Issuer,
            Audience = _jwksOptions.Audience,
            Subject = _identityClaims,
            NotBefore = created.DateTime,
            Expires = expires.DateTime,
            SigningCredentials = _jwksService.GetCurrent()
        });

        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Esse metodo obtem o token, e inclui em uma classe padrão de retorno, com informações
    /// de criação e expiração do token
    /// </summary>
    /// <returns></returns>
    public CredentialToken GetUserToken()
    {

        var result = new CredentialToken
        {
            Token = BuildToken(out DateTimeOffset created, out DateTimeOffset expires),
            Expires = expires,
            Created = created

        };

        return result;
    }

    /// <summary>
    /// Caso necessite, valida se um token é valido, segundo os parametros de JwksOptions
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public bool CheckTokenIsValid(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new SecurityTokenException($"{nameof(CheckTokenIsValid)} Token não foi informado no corpo da request");
        }

        var validationParameters = new TokenValidationParameters
        {
            ValidIssuer = _jwksOptions.Issuer,
            ValidAudience = _jwksOptions.Audience,
            IssuerSigningKey = _jwksService.GetCurrent().Key,
            RequireExpirationTime = true
        };

        try
        {
            var _tokenHandler = new JwtSecurityTokenHandler();
            var isValid = _tokenHandler.ValidateToken(
                token,
                validationParameters,
                out SecurityToken securityToken);

            return isValid != null;
        }
        catch (Exception)
        {
            return false;
        }

    }

}

