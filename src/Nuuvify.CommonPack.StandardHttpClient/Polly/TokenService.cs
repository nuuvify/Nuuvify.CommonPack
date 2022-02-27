using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.Notificator;
using Nuuvify.CommonPack.Security.Abstraction;
using Nuuvify.CommonPack.StandardHttpClient.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Nuuvify.CommonPack.StandardHttpClient.Polly
{
    public class TokenService : ITokenService
    {

        private readonly ILogger<TokenService> _logger;
        private readonly IStandardHttpClient _standardHttpClient;
        private readonly IConfiguration _configuration;
        private readonly IUserAuthenticated _userAuthenticated;
        private CredentialToken _credentialToken;


        ///<inheritdoc/>
        public List<NotificationR> Notifications { get; set; }

        public TokenService(
            IOptions<CredentialToken> credentialToken,
            IStandardHttpClient standardHttpClient,
            IConfiguration configuration,
            ILogger<TokenService> logger, IUserAuthenticated userAuthenticated)
        {
            _standardHttpClient = standardHttpClient;
            _configuration = configuration;
            _userAuthenticated = userAuthenticated;
            _logger = logger;

            _credentialToken = credentialToken.Value;

            Notifications = new List<NotificationR>();
        }



        public string GetUsername()
        {
            return _userAuthenticated.Username();
        }



        ///<inheritdoc/>
        public CredentialToken GetActualToken()
        {
            return _credentialToken;
        }


        public async Task<bool> GetNewToken(string urlToken, string login, string password, string userClaim = null)
        {
            var messageLog = $"{nameof(GetNewToken)}";

            var messageBody = new
            {
                Login = login,
                Password = password
            };

            var userName = _userAuthenticated.Username();
            if (userClaim == null) 
                userClaim = userName;

            _standardHttpClient.CreateClient();
            if (!string.IsNullOrWhiteSpace(userClaim))
                _standardHttpClient.WithHeader(Constants.UserClaimHeader, userClaim);

            _logger.LogDebug(messageLog + " - _standardHttpClient.GetNewToken()");


            var response = await _standardHttpClient.Post(urlToken, messageBody: messageBody);

            _credentialToken = ReturnClass<CredentialToken>(response);


            if (string.IsNullOrWhiteSpace(_credentialToken?.Token))
            {
                if (_credentialToken is null) _credentialToken = new CredentialToken();
                _credentialToken.Warnings.Add("ResponseMessage", response.ReturnMessage);

                _logger.LogWarning(messageLog + " - Response message invalid: {response}", response?.ReturnMessage);

                return false;
            }

            var tempToken = _credentialToken.Token;
            var tempCreated = _credentialToken.Created;
            var tempExpire = _credentialToken.Expires;

            _credentialToken.LoginId ??= login;
            _credentialToken.Password = password;
            _credentialToken.Expires = tempExpire;
            _credentialToken.Created = tempCreated;
            _credentialToken.Token = tempToken;

            return true;
        }


        ///<inheritdoc/>
        public async Task<CredentialToken> GetToken(string login = null, string password = null, string userClaim = null)
        {

            var messageLog = $"{nameof(GetToken)}";
            _logger.LogDebug($"{messageLog} - Inicio");

            if (string.IsNullOrWhiteSpace(login))
            {
                login = _configuration.GetSection("ApisCredentials:Username")?.Value;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                password = _configuration.GetSection("ApisCredentials:Password")?.Value;
            }

            var urlLogin = _configuration.GetSection("AppConfig:AppURLs:UrlLoginApi")?.Value;
            var urlToken = $"{urlLogin}{_configuration.GetSection("AppConfig:AppURLs:UrlLoginApiToken")?.Value}";

            _logger.LogDebug(messageLog + " - urlToken: {url}", urlToken);




            if (string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(urlLogin) ||
                string.IsNullOrWhiteSpace(urlToken))
            {
                Notifications.Add(new NotificationR(nameof(GetToken), "Algum dos dados a seguir não deveria estar branco: login ou password ou urlLogin ou urlToken"));

                _logger.LogWarning(messageLog + " - Algum dos dados a seguir não deveria estar branco: login ou password ou urlLogin ou urlToken");

                return null;
            }


            await GetNewToken(urlToken, login, password, userClaim);


            if (!(_credentialToken is null) && !string.IsNullOrWhiteSpace(_credentialToken?.Token))
            {
                _logger.LogDebug($"{messageLog} - Token obtido para o usuario: {_credentialToken.LoginId} - Final");
            }
            else
            {
                _logger.LogDebug(messageLog + " - Final");
            }


            return _credentialToken;

        }


        ///<inheritdoc/>
        public string GetTokenAcessor()
        {
            var messageLog = $"{nameof(GetTokenAcessor)}";
            _logger.LogDebug(messageLog + " - Inicio");


            if (!_userAuthenticated.IsAuthenticated(out string token))
            {
                _logger.LogWarning(messageLog + " - Não foi encontrado Authorization no HttpContextAccessor, usuario não esta logado com um token");
                return string.Empty;
            }


            _logger.LogDebug(messageLog + " - Final");

            return token;
        }

        private T ReturnClass<T>(HttpStandardReturn returnResult) where T : class
        {
            if (returnResult.Success)
            {
                if (!string.IsNullOrWhiteSpace(returnResult.ReturnMessage))
                {
                    return DeserealizeObject<T>(returnResult.ReturnMessage);

                }
                Notifications.Add(new NotificationR(typeof(T).Name, "Não foi possivel deserializar essa classe"));
                Notifications.Add(new NotificationR(typeof(T).Name, $"{returnResult.ReturnMessage}"));
                return (T)Convert.ChangeType(null, typeof(T));
            }
            Notifications.Add(new NotificationR(typeof(T).Name, $"Não houve sucesso no retorno da request para a classe {nameof(HttpStandardReturn)}"));
            Notifications.Add(new NotificationR(typeof(T).Name, $"{returnResult.ReturnMessage}"));

            return (T)Convert.ChangeType(null, typeof(T));
        }

        private T DeserealizeObject<T>(string message) where T : class
        {
            var JsonSettings = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                IgnoreNullValues = true
            };


            var jsonData = JsonSerializer.Deserialize<DeserializeObjectSuccess<T>>(message, JsonSettings);
            if (!(jsonData is null) && jsonData.Data != null)
            {
                var returnData = jsonData.Data;
                return returnData;
            }


            return (T)Convert.ChangeType(null, typeof(T));
        }
    }
}