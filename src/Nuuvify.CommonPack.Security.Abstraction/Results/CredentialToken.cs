using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Nuuvify.CommonPack.StandardHttpClient.xTest")]
namespace Nuuvify.CommonPack.Security.Abstraction
{
    public class CredentialToken
    {


        public CredentialToken()
        {
            Warnings = new Dictionary<string, string>();
        }

        private string _loginId;


        /// <summary>
        /// Informar o Login da aplicação ou de um usuario
        /// </summary>
        public string LoginId
        {
            get
            {
                return _loginId;
            }
            set
            {
                if (_loginId?.ToLowerInvariant() != value?.ToLowerInvariant() ||
                    !IsValidToken())
                {
                    Token = null;
                    Expires = DateTimeOffset.MinValue;
                    Created = DateTimeOffset.MinValue;
                    Warnings = new Dictionary<string, string>();

                    _loginId = value;
                }
                
            }
        }
        public string Password { get; set; }
        public string Cookie { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }

        public DateTimeOffset Expires { get; set; }
        public DateTimeOffset Created { get; set; }
        public IDictionary<string, string> Warnings { get; set; }

        private long _expiresIn;

        /// <summary>
        /// Valor deve ser diferente de 0 para que Expires seja calculado
        /// </summary>
        public long ExpiresIn
        {
            get
            {
                return _expiresIn;
            }
            set
            {
                _expiresIn = value;
                if (_expiresIn != 0)
                    Expires = DateTimeOffset.Now.AddSeconds(_expiresIn);
            }
        }

        /// <summary>
        /// Verifica se o token ira expirar em até "n" minutos, informe o parametro negativo
        /// </summary>
        /// <returns></returns>
        public bool IsValidToken(int minutes = -5)
        {

            return !DateTimeOffset.MinValue.Equals(Expires) &&
                    Expires.AddMinutes(minutes) >= DateTimeOffset.Now;

        }

    }
}
