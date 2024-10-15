using Microsoft.AspNetCore.Http;


namespace Microsoft.AspNetCore.Builder;


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
                new NotificationR("Chave não informada no Header", $"Informa a chave '{_headerKey}' no header da request"),
            ];
            await context.Response.WriteAsJsonAsync(_notificationsMiddleware);
            return;
        }

        await _next(context);
    }
}

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
    public static IApplicationBuilder UseHttpRequestKeyVerifyMiddleware(
        this IApplicationBuilder builder,
        string headerKey = "x-user-claim",
        int failureStatusCode = StatusCodes.Status400BadRequest)
    {

        return builder.UseMiddleware<UseHttpRequestKeyVerifyMiddleware>(headerKey, failureStatusCode);
    }
}

public class NotificationR
{
    public string Property { get; set; }
    public string Message { get; set; }

    public NotificationR(string property, string message)
    {
        Property = property;
        Message = message;
    }
}
