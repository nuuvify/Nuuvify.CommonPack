using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.Notificator;
using Nuuvify.CommonPack.Middleware.Abstraction.Results;

namespace Nuuvify.CommonPack.Middleware.Filters;

/// <summary>
/// [ApiKey(KeyName = new string[] {"MyKeyName")] deve ser utilizado em sua Controller ou Action, se utiliza-lo, não utilize [Authorize] <br/>
/// O parametro "KeyName" deve corresponder, obrigatoriamente a uma entrada no serviço de VAULT <br/>
/// [ApiKey(KeyName = new string[] {"MyKeyName")] ou <br/>
/// [ApiKey(KeyName = new string[] {"MyKeyName", "OtherKeyName")] <br/>
/// </summary>
[AttributeUsage(validOn: AttributeTargets.Class |
    AttributeTargets.Method, AllowMultiple = true)]
public class ApiKeyAttribute : Attribute, IFilterFactory
{
    public string[] KeyName { get; set; }

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {

        var logger = (ILogger)serviceProvider.GetService(typeof(ILogger));
        var configuration = (IConfiguration)serviceProvider.GetRequiredService(typeof(IConfiguration));

        var apiKeyFilter = new ApiKeyFilter(logger, configuration, KeyName);
        return apiKeyFilter;
    }

    public bool IsReusable => false;

}

public static class ApiKeyFilterConstants
{
    public const string ApiKeyInfo = "ApiKeyInfo";

}

public class ApiKeyFilter : IResourceFilter, IActionFilter, IExceptionFilter
{
    private readonly string[] _keyName;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public ApiKeyFilter(
        ILogger logger,
        IConfiguration configuration,
        string[] keyName)
    {
        _logger = logger;
        _configuration = configuration;
        _keyName = keyName;

    }

    private static ClaimsPrincipal AddNewClaim(ClaimsPrincipal principal, string keyName)
    {
        var clone = principal.Clone();
        var newIdentity = (ClaimsIdentity)clone.Identity;

        var claim = new Claim(ApiKeyFilterConstants.ApiKeyInfo, keyName);
        newIdentity.AddClaim(claim);
        return clone;
    }

    private bool HasClaimApiKey(ClaimsPrincipal principal)
    {
        return principal.HasClaim(x => x.Type == ApiKeyFilterConstants.ApiKeyInfo);
    }

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var logMessage = "Run: [Before] IResourceFilter.OnResourceExecuting.";

        if (_logger == null)
            Debug.WriteLine($"Console: {logMessage}");
        else
            _logger.LogDebug($"ILogger: {logMessage}");

    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        var logMessage = "Run: [After] IResourceFilter.OnResourceExecuting.";

        if (_logger == null)
            Debug.WriteLine($"Console: {logMessage}");
        else
            _logger.LogDebug($"ILogger: {logMessage}");

    }

    private (bool hasHeader, string apiKeyValue) HasKeyHeader(ActionExecutingContext context, string itemKeyName)
    {

        if (context.HttpContext.Request.Headers.TryGetValue(itemKeyName, out var headerApiKey))
        {
            var apiKeyVaultValue = _configuration.GetSection(itemKeyName)?.Value;
            var matches = apiKeyVaultValue is not null && CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(apiKeyVaultValue),
                Encoding.UTF8.GetBytes(headerApiKey.ToString()));
            return (hasHeader: matches, apiKeyValue: apiKeyVaultValue);
        }

        return (false, string.Empty);

    }

    public void OnActionExecuting(ActionExecutingContext context)
    {

        try
        {

            if (_configuration == null)
            {
                throw new ArgumentException($"{nameof(IConfiguration)} is null");
            }
            if (!_keyName.NotNullOrZero())
            {
                throw new ArgumentException($"{nameof(_keyName)} is null");
            }

            if (HasClaimApiKey(context.HttpContext.User)) return;

            foreach (var item in _keyName)
            {
                (bool hasHeader, string apiKeyValue) hasKeyHeader = HasKeyHeader(context, item);
                if (hasKeyHeader.hasHeader)
                {
                    context.HttpContext.User = AddNewClaim(context.HttpContext.User, item);
                    return;
                }

            }

            var notification = new NotificationR(nameof(_keyName), "ApiKey informed in the request header, is not valid, or does not exist in the Vault of this application.");
            var jsonResult = JsonSerializer.Serialize(new ReturnStandardErrors<NotificationR>
            {
                Success = false,
                Errors = new List<NotificationR> { notification }
            });

            context.Result = new ContentResult
            {
                StatusCode = 401,
                Content = jsonResult
            };

            return;

        }
        catch (Exception)
        {
            throw;
        }

    }

    public void OnActionExecuted(ActionExecutedContext context)
    {

        var logMessage = "Run: [After] IActionFilter.OnActionExecuting.";

        if (_logger == null)
            Debug.WriteLine($"Console: {logMessage}");
        else
            _logger.LogDebug($"ILogger: {logMessage}");

    }

    public void OnException(ExceptionContext context)
    {
        var notification = new NotificationR(nameof(_keyName), context.Exception.Message);
        var jsonResult = JsonSerializer.Serialize(new ReturnStandardErrors<NotificationR>
        {
            Success = false,
            Errors = new List<NotificationR> { notification }
        });

        context.Result = new ContentResult
        {
            StatusCode = 401,
            Content = jsonResult
        };

        context.ExceptionHandled = true;

    }

}

