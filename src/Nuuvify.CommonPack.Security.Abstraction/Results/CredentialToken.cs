using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        /// Informar o Login da aplicação ou de um usuario, ao informas essa propriedade, <br/>
        /// as demais serão alteradas para seus valores padrão.
        /// </summary>
        /// <example>ZOCATEL<example>
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

        /// <summary>
        /// Senha do usuario ou aplicação
        /// </summary>
        /// <example>Xyz.9</example>
        public string Password { get; set; }
        public string Cookie { get; set; }

        /// <summary>
        /// JWT gerado pelo mecanismo de geração de token
        /// </summary>
        /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.</example>
        public string Token { get; set; }

        /// <summary>
        /// JWT gerado pelo mecanismo de geração de token, para obter um novo token quando o mesmo expirar <br/>
        /// sem necessidade de nova autenticação
        /// </summary>
        /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.</example>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Data com timezone que o token ira expirar
        /// </summary>
        /// <example>31/12/2021 23:43:51 -03:00</example>
        [DataType(DataType.DateTime)]
        public DateTimeOffset Expires { get; set; }

        /// <summary>
        /// Data com timezone que o token foi gerado
        /// </summary>
        /// <example>31/12/2021 22:43:51 -03:00</example>
        [DataType(DataType.DateTime)]
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// Lista com notificações, caso possua
        /// </summary>
        public IDictionary<string, string> Warnings { get; set; }

        private long _expiresIn;

        /// <summary>
        /// Valor em SEGUNDOS, deve ser diferente de 0 para que Expires seja calculado
        /// </summary>
        /// <example>3600</example>
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
