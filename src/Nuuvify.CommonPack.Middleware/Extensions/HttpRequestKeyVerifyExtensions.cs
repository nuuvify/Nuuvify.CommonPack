using Microsoft.AspNetCore.Http;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Legacy middleware for HTTP header API key verification.
/// This middleware is maintained for backward compatibility during migration.
/// Use ApiKeyAuthenticationDefaults.AuthenticationScheme with AddApiKeyAuthentication() instead.
/// </summary>
[Obsolete(
    "Use AddApiKeyAuthentication() from Nuuvify.CommonPack.Security instead. " +
    "This middleware will be removed in a future major version. " +
    "See README.md in Nuuvify.CommonPack.Security for migration guide.",
    error: false)]
public class UseHttpRequestKeyVerifyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _headerKey;
    private readonly int _failureStatusCode;
    private List<NotificationR> _notificationsMiddleware;

    public UseHttpRequestKeyVerifyMiddleware(
        RequestDelegate next,
        string headerKey,
        int failureStatusCode)
    {
        _next = next;
        _headerKey = headerKey;
        _failureStatusCode = failureStatusCode;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(_headerKey, out var userFront))
        {
            context.Response.StatusCode = _failureStatusCode;
            _notificationsMiddleware =
            [
                new NotificationR("Chave não informada no Header", $"Informe a chave '{_headerKey}' no header da request"),
            ];
            await context.Response.WriteAsJsonAsync(_notificationsMiddleware);
            return;
        }

        await _next(context);
    }
}

/// <summary>
/// Extension methods for legacy HTTP request key verification middleware.
/// This class is maintained for backward compatibility during migration.
/// Use AddApiKeyAuthentication() from Nuuvify.CommonPack.Security instead.
/// </summary>
[Obsolete(
    "Use AddApiKeyAuthentication() from Nuuvify.CommonPack.Security instead. " +
    "This extension class will be removed in a future major version. " +
    "See README.md in Nuuvify.CommonPack.Security for migration guide.",
    error: false)]
public static class UseHttpRequestKeyVerifyExtensions
{

    /// <summary>
    /// Esse middleware verifica se foi enviado uma determinada chave no header da request,
    /// caso não tenha sido enviado, ele retorna um status code de erro e uma mensagem de erro.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="headerKey"></param>
    /// <param name="failureStatusCode"></param>
    /// <returns></returns>
    [Obsolete(
        "Use AddApiKeyAuthentication() from Nuuvify.CommonPack.Security instead. " +
        "See README.md in Nuuvify.CommonPack.Security for migration guide.",
        error: false)]
    public static IApplicationBuilder UseHttpRequestKeyVerifyMiddleware(
        this IApplicationBuilder builder,
        string headerKey = "x-user-claim",
        int failureStatusCode = StatusCodes.Status400BadRequest)
    {

        return builder.UseMiddleware<UseHttpRequestKeyVerifyMiddleware>(headerKey, failureStatusCode);
    }
}

