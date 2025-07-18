using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.Notificator;
using Nuuvify.CommonPack.Security.Abstraction;
using Nuuvify.CommonPack.StandardHttpClient.Results;


namespace Nuuvify.CommonPack.StandardHttpClient.Polly
{
    public class TokenService : ITokenService
    {

        private readonly ILogger<TokenService> _logger;
        private readonly IStandardHttpClient _standardHttpClient;
        private readonly IConfiguration _configuration;
        protected readonly IHttpContextAccessor _accessor;
        private CredentialToken _credentialToken;


        ///<inheritdoc/>
        public List<NotificationR> Notifications { get; set; }


        public TokenService(
            IOptions<CredentialToken> credentialToken,
            IStandardHttpClient standardHttpClient,
            IConfiguration configuration,
            ILogger<TokenService> logger,
            IHttpContextAccessor accessor)
        {
            _standardHttpClient = standardHttpClient;
            _configuration = configuration;
            _logger = logger;
            _accessor = accessor;

            _credentialToken = credentialToken.Value;

            Notifications = new List<NotificationR>();
        }


        private string GetHttpClientTokenName { get; set; }

        public string HttpClientTokenName(string httpClientName = "CredentialApi")
        {
            GetHttpClientTokenName = string.IsNullOrWhiteSpace(httpClientName)
                ? null
                : httpClientName;

            return GetHttpClientTokenName;
        }

        public string GetUsername()
        {
            return _accessor?.HttpContext?.User.GetLogin();
        }

        ///<inheritdoc/>
        public CredentialToken GetActualToken()
        {
            return _credentialToken;
        }


        public async Task<bool> GetNewToken(string urlToken, string login, string password, string userClaim = null)
        {
            var messageLog = $"{nameof(TokenService.GetNewToken)}";

            var messageBody = new
            {
                Login = login,
                Password = password
            };

            var userName = _accessor?.HttpContext?.User.GetLogin();
            userClaim ??= userName;

            _standardHttpClient.CreateClient(GetHttpClientTokenName ?? HttpClientTokenName());
            _standardHttpClient.ResetStandardHttpClient();

            if (!string.IsNullOrWhiteSpace(userClaim))
                _standardHttpClient.WithHeader(Constants.UserClaimHeader, userClaim);



            _logger.LogDebug("{messageLog} - User Claim: {userClaim}", messageLog, userClaim);


            var response = await _standardHttpClient.Post(urlRoute: urlToken, messageBody: messageBody);

            _credentialToken = ReturnClass<CredentialToken>(response);


            if (_credentialToken is null || string.IsNullOrWhiteSpace(_credentialToken?.Token))
            {
                _credentialToken = new CredentialToken();
                _credentialToken.Warnings.Add("ResponseMessage", response.ReturnMessage);

                _logger.LogWarning("{messageLog} - Response message invalid: {ReturnCode} {ReturnMessage}", messageLog, response?.ReturnCode, response?.ReturnMessage);

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

            Notifications.Clear();

            var messageLog = $"{nameof(TokenService.GetToken)}";
            _logger.LogDebug($"{messageLog} - Inicio");

            if (string.IsNullOrWhiteSpace(login))
            {
                login = _configuration.GetSection("ApisCredentials:Username")?.Value;
                login ??= _configuration.GetSection("AzureAdOpenID:cc:ClientId")?.Value;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                password = _configuration.GetSection("ApisCredentials:Password")?.Value;
                password ??= _configuration.GetSection("AzureAdOpenID:cc:ClientSecret")?.Value;
            }

            var urlLogin = _configuration.GetSection("AppConfig:AppURLs:UrlLoginApi")?.Value;
            var urlToken = $"{urlLogin}{_configuration.GetSection("AppConfig:AppURLs:UrlLoginApiToken")?.Value}";

            _logger.LogDebug("{messageLog} - urlToken: {urlToken}", messageLog, urlToken);




            if (string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(urlLogin) ||
                string.IsNullOrWhiteSpace(urlToken))
            {
                Notifications.Add(new NotificationR(nameof(GetToken), "Algum dos dados a seguir não deveria estar branco: login ou password ou urlLogin ou urlToken"));

                _logger.LogWarning("{messageLog} - Algum dos dados a seguir não deveria estar branco: login ou password ou urlLogin ou urlToken", messageLog);

                return null;
            }


            await GetNewToken(urlToken, login, password, userClaim);


            if (_credentialToken != null && !string.IsNullOrWhiteSpace(_credentialToken?.Token))
            {
                _logger.LogDebug("{messageLog} - Token obtido para o usuario: {LoginId} - Final", messageLog, _credentialToken.LoginId);
            }
            else
            {
                _logger.LogDebug("{messageLog} - Final", messageLog);
            }


            return _credentialToken;

        }


        ///<inheritdoc/>
        public string GetTokenAcessor()
        {
            var messageLog = $"{nameof(TokenService.GetTokenAcessor)}";
            _logger.LogDebug($"{messageLog} - Inicio");


            if (!IsAuthenticated(out string token))
            {
                _logger.LogWarning("{messageLog} - Não foi encontrado Authorization no HttpContextAccessor, usuario não esta logado com um token", messageLog);
                return string.Empty;
            }


            _logger.LogDebug("{messageLog} - Final", messageLog);

            return token;
        }

        private bool IsAuthenticated(out string token)
        {
            token = "";
            if (_accessor?.HttpContext == null) return false;
            var esquemaAutenticacao = _accessor?.HttpContext.Request.Headers
                .FirstOrDefault(x => x.Key.Equals("Authorization")).Value;

            foreach (var item in esquemaAutenticacao)
            {
                token = item?.Replace("bearer", "").Replace("Bearer", "").Trim();
            }

            if (_accessor?.HttpContext == null) return false;
            return _accessor.HttpContext.User.Identity.IsAuthenticated;
        }

        private T ReturnClass<T>(HttpStandardReturn returnResult) where T : class
        {
            Notifications.Clear();

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
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,

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