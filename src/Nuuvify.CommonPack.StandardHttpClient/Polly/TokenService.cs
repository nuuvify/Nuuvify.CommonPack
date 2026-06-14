using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Runtime.CompilerServices;
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

namespace Nuuvify.CommonPack.StandardHttpClient.Polly;

public class TokenService : ITokenService
{

    private readonly ILogger<TokenService> _logger;
    private readonly IStandardHttpClient _standardHttpClient;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _accessor;
    private readonly List<NotificationR> _notifications;

    private CredentialToken _credentialToken;
    protected IHttpContextAccessor Accessor => _accessor;

    ///<inheritdoc/>
    public ReadOnlyCollection<NotificationR> Notifications => new ReadOnlyCollection<NotificationR>(_notifications);

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

        _notifications = new List<NotificationR>();
    }

    private string GetHttpClientTokenName { get; set; }

    private static string Origin([CallerMemberName] string method = "")
    {
        return $"{nameof(TokenService)}.{method}";
    }

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

    public async Task<bool> GetNewToken(
        string urlToken,
        string login,
        string password,
        string userClaim = null,
        CancellationToken cancellationToken = default)
    {
        var messageLog = Origin();

        try
        {
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
                _ = _standardHttpClient.WithHeader(Constants.UserClaimHeader, userClaim);

            _logger.LogDebug("{MessageLog} - User Claim: {UserClaim}", messageLog, userClaim);

            var response = await _standardHttpClient.Post(
                urlRoute: urlToken,
                messageBody: messageBody,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            _credentialToken = ReturnClass<CredentialToken>(response);

            if (_credentialToken is null || string.IsNullOrWhiteSpace(_credentialToken.Token))
            {
                _credentialToken = new CredentialToken();
                _credentialToken.Warnings["ResponseMessage"] = response?.ReturnMessage ?? "Token nao retornado pela API de credenciais.";

                _notifications.Add(new NotificationR(Origin(), _credentialToken.Warnings["ResponseMessage"]));
                _logger.LogWarning("{MessageLog} - Response message invalid: {ReturnCode} {ReturnMessage}", messageLog, response?.ReturnCode, response?.ReturnMessage);

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
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _notifications.Add(new NotificationR(Origin(), $"Timeout ao obter token na API de credenciais: {ex.Message}"));
            _logger.LogError(ex, "{MessageLog} - Timeout ao obter token.", messageLog);
            return false;
        }
        catch (TimeoutException ex)
        {
            _notifications.Add(new NotificationR(Origin(), $"Timeout ao obter token na API de credenciais: {ex.Message}"));
            _logger.LogError(ex, "{MessageLog} - Timeout ao obter token.", messageLog);
            return false;
        }
        catch (HttpRequestException ex)
        {
            _notifications.Add(new NotificationR(Origin(), $"Falha HTTP ao obter token: {ex.Message}"));
            _logger.LogError(ex, "{MessageLog} - Falha HTTP ao obter token.", messageLog);
            return false;
        }
        catch (SocketException ex)
        {
            _notifications.Add(new NotificationR(Origin(), $"Falha de rede ao obter token: {ex.Message}"));
            _logger.LogError(ex, "{MessageLog} - Falha de rede ao obter token.", messageLog);
            return false;
        }
        catch (AuthenticationException ex)
        {
            _notifications.Add(new NotificationR(Origin(), $"Falha de autenticacao TLS ao obter token: {ex.Message}"));
            _logger.LogError(ex, "{MessageLog} - Falha de autenticacao TLS ao obter token.", messageLog);
            return false;
        }
        catch (JsonException ex)
        {
            _notifications.Add(new NotificationR(Origin(), $"Falha ao processar resposta de token: {ex.Message}"));
            _logger.LogError(ex, "{MessageLog} - Falha ao processar resposta de token.", messageLog);
            return false;
        }
    }

    ///<inheritdoc/>
    public async Task<CredentialToken> GetToken(
        string login = null,
        string password = null,
        string userClaim = null,
        CancellationToken cancellationToken = default)
    {
        _notifications.Clear();

        var messageLog = Origin();
        _logger.LogDebug("{MessageLog} - Inicio", messageLog);

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

        _logger.LogDebug("{MessageLog} - urlToken: {UrlToken}", messageLog, urlToken);

        if (string.IsNullOrWhiteSpace(login) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(urlLogin) ||
            string.IsNullOrWhiteSpace(urlToken))
        {
            _notifications.Add(new NotificationR(Origin(), "Algum dos dados a seguir não deveria estar branco: login ou password ou urlLogin ou urlToken"));

            _logger.LogWarning("{MessageLog} - Algum dos dados a seguir não deveria estar branco: login ou password ou urlLogin ou urlToken", messageLog);

            return null;
        }

        var tokenGenerated = await GetNewToken(urlToken, login, password, userClaim, cancellationToken).ConfigureAwait(false);

        if (!tokenGenerated)
        {
            var warningMessage = _credentialToken?.Warnings.TryGetValue("ResponseMessage", out var responseMessage) == true
                ? responseMessage
                : "Falha ao obter token na API de credenciais.";

            if (_notifications.All(n => !string.Equals(n.Property, Origin(), StringComparison.Ordinal)))
            {
                _notifications.Add(new NotificationR(Origin(), warningMessage));
            }

            _logger.LogWarning("{MessageLog} - Falha ao obter token: {WarningMessage}", messageLog, warningMessage);
            return null;
        }

        if (_credentialToken != null && !string.IsNullOrWhiteSpace(_credentialToken.Token))
        {
            _logger.LogDebug("{MessageLog} - Token obtido para o usuario: {LoginId} - Final", messageLog, _credentialToken.LoginId);
        }
        else
        {
            _logger.LogDebug("{MessageLog} - Final", messageLog);
        }

        return _credentialToken;

    }

    ///<inheritdoc/>
    public string GetTokenAcessor()
    {
        var messageLog = $"{nameof(TokenService.GetTokenAcessor)}";
        _logger.LogDebug("{MessageLog} - Inicio", messageLog);

        if (!IsAuthenticated(out string token))
        {
            _logger.LogWarning("{MessageLog} - Não foi encontrado Authorization no HttpContextAccessor, usuario não esta logado com um token", messageLog);
            return string.Empty;
        }

        _logger.LogDebug("{MessageLog} - Final", messageLog);

        return token;
    }

    private bool IsAuthenticated(out string token)
    {
        token = "";
        if (_accessor?.HttpContext == null) return false;
        var esquemaAutenticacao = _accessor?.HttpContext.Request.Headers
            .FirstOrDefault(x => x.Key.Equals("Authorization", StringComparison.Ordinal)).Value;

        foreach (var item in esquemaAutenticacao)
        {
            token = item?.Replace("bearer", "").Replace("Bearer", "").Trim();
        }

        return _accessor?.HttpContext?.User?.Identity?.IsAuthenticated == true;
    }

    private T ReturnClass<T>(HttpStandardReturn returnResult) where T : class
    {
        if (returnResult == null)
        {
            _notifications.Add(new NotificationR(Origin(), "Retorno HTTP nulo ao obter token."));
            return (T)Convert.ChangeType(null, typeof(T), CultureInfo.InvariantCulture);
        }

        _notifications.Clear();

        if (returnResult.Success)
        {
            if (!string.IsNullOrWhiteSpace(returnResult.ReturnMessage))
            {
                return DeserealizeObject<T>(returnResult.ReturnMessage);

            }
            _notifications.Add(new NotificationR(typeof(T).Name, "Não foi possivel deserializar essa classe"));
            _notifications.Add(new NotificationR(typeof(T).Name, $"{returnResult.ReturnMessage}"));
            return (T)Convert.ChangeType(null, typeof(T), CultureInfo.InvariantCulture);
        }
        _notifications.Add(new NotificationR(typeof(T).Name, $"Não houve sucesso no retorno da request para a classe {nameof(HttpStandardReturn)}"));
        _notifications.Add(new NotificationR(typeof(T).Name, $"{returnResult.ReturnMessage}"));

        return (T)Convert.ChangeType(null, typeof(T), CultureInfo.InvariantCulture);
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

        return (T)Convert.ChangeType(null, typeof(T), CultureInfo.InvariantCulture);
    }
}
