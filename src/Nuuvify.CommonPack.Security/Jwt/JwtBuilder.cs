using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Nuuvify.CommonPack.Security.Abstraction;

namespace Nuuvify.CommonPack.Security.Jwt
{


    /// <summary>
    /// Para gerar um token, após validar as credenciais do seu usuario, execute essa classe da seguinte forma:
    /// <example>
    /// <code>
    ///     private readonly IOptions{JwtTokenOptions} jwtTokenOptions;
    /// 
    ///     var token = new JwtBuilder()
    ///         .WithJwtOptions(jwtTokenOptions.Value)
    ///         .WithJwtUserClaims(usuarioGrupo)
    ///         .GetUserToken();
    /// </code>
    /// </example>
    /// </summary>
    public class JwtBuilder : IJwtBuilder
    {

        protected JwtTokenOptions _jwtTokenOptions;
        protected ICollection<Claim> _jwtClaims;
        protected ClaimsIdentity _identityClaims;



        private void ThrowIfInvalidOptions(JwtTokenOptions jwtTokenOptions)
        {

            if (jwtTokenOptions is null)
                throw new ArgumentNullException(nameof(jwtTokenOptions), "Objeto não pode ser null");


            jwtTokenOptions.NewInstance();

            if (jwtTokenOptions.ValidFor.TotalMinutes <= 0)
                throw new NotSupportedException($"O período deve ser maior que zero {nameof(jwtTokenOptions.ValidFor)}");


            if (jwtTokenOptions.JtiGenerator is null)
                throw new ArgumentNullException(nameof(jwtTokenOptions), "JtiGenerator - Propriedade não pode ser nulo");


            _jwtTokenOptions = jwtTokenOptions;

        }

        public virtual long ToUnixEpochDate(DateTime date)
            => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
                .TotalSeconds);



        /// <summary>
        /// Use esse metodo primeiro, após receber IOptions{JwtTokenOptions} em seu metodo de chamada
        /// </summary>
        /// <param name="jwtTokenOptions">Tag encontrado no appsettings.json</param>
        /// <returns></returns>
        public virtual IJwtBuilder WithJwtOptions(JwtTokenOptions jwtTokenOptions)
        {

            ThrowIfInvalidOptions(jwtTokenOptions);

            return this;
        }

        /// <summary>
        /// Use esse metodo após o metodo WithJwtUserClaims(), caso queira inserir Informações padrões do Token
        /// como JwtRegisteredClaimNames.Nbf , JwtRegisteredClaimNames.Iat (essas informações já são passadas pela
        /// classe  WithJwtUserClaims(), isso ira gerar outra claim com essas informações )
        /// </summary>
        /// <returns></returns>
        public virtual IJwtBuilder WithJwtClaims()
        {
            if (_jwtClaims is null)
            {
                _jwtClaims = new List<Claim>();
            }

            _jwtClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            _jwtClaims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.Now).ToString()));
            _jwtClaims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.Now.AddHours(_jwtTokenOptions.ValidityInHours)).ToString(), ClaimValueTypes.Integer64));

            _identityClaims.AddClaims(_jwtClaims);

            return this;
        }

        /// <summary>
        /// Esse metodo é utilizado após o WithJwtOptions()
        /// É obrigatorio que o PersonWithRolesQueryResult.Login esteja preenchido
        /// </summary>
        /// <param name="personGroups">Classe obtida pelo seu metodo que autenticou o usuario/senha</param>
        /// <returns></returns>
        public virtual IJwtBuilder WithJwtUserClaims<T>(PersonWithRolesQueryResult personGroups)
            where T : IPersonBuilder
        {

            if (string.IsNullOrWhiteSpace(personGroups?.Login))
            {
                throw new ArgumentNullException(nameof(personGroups), "Login do usuario não pode ser nulo");
            }

            _jwtClaims = new List<Claim>();
            _identityClaims = new ClaimsIdentity();

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, personGroups?.Name ?? ""),
                new Claim(ClaimTypes.NameIdentifier, personGroups.Login)
            };
            _identityClaims.AddClaims(userClaims);


            var grupos = personGroups?.Groups.ToList();
            for (int i = 0; i < grupos.Count; i++)
            {
                _identityClaims.AddClaim(new Claim(grupos[i].Group, i.ToString()));
            }


            return this;
        }

        public virtual ClaimsIdentity GetClaimsIdentity()
        {
            return _identityClaims;
        }

        /// <summary>
        /// Esse metodo gera um JwtToken, e retorno o mesmo como string
        /// </summary>
        /// <returns>Retorna um token como string</returns>
        public virtual string BuildToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _jwtTokenOptions.Issuer,
                Audience = _jwtTokenOptions.Audience,
                Subject = _identityClaims,
                NotBefore = _jwtTokenOptions.NotBefore.DateTime,
                Expires = _jwtTokenOptions.Expiration.DateTime,
                SigningCredentials = _jwtTokenOptions.SigningCredentials()
            });

            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Esse metodo obtem o token, e inclui em uma classe padrão de retorno, com informações
        /// de criação e expiração do token
        /// </summary>
        /// <returns></returns>
        public virtual CredentialToken GetUserToken()
        {
            var result = new CredentialToken
            {
                Token = BuildToken(),
                Expires = _jwtTokenOptions.Expiration,
                Created = _jwtTokenOptions.NotBefore

            };

            return result;
        }

        /// <summary>
        /// Caso necessite, valida se um token é valido, segundo os parametros de JwtTokenOptions
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual bool CheckTokenIsValid(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new SecurityTokenException($"{nameof(CheckTokenIsValid)} Token não foi informado no corpo da request");
            }

            var validationParameters = new TokenValidationParameters
            {
                ValidIssuer = _jwtTokenOptions.Issuer,
                ValidAudience = _jwtTokenOptions.Audience,
                IssuerSigningKey = _jwtTokenOptions.SigningCredentials().Key,
                RequireExpirationTime = true
            };


            try
            {
                var _tokenHandler = new JwtSecurityTokenHandler();
                var isValid = _tokenHandler.ValidateToken(token, validationParameters,
                                out SecurityToken securityToken);

                return isValid != null;
            }
            catch (Exception)
            {
                return false;
            }

        }


    }


}