using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Middleware.Abstraction;
using Nuuvify.CommonPack.Observability;
using Nuuvify.CommonPack.Observability.Abstraction;

namespace Nuuvify.CommonPack.Middleware.Setups;

/// <summary>
/// Popula o contexto neutro de operação para uma requisição HTTP.
/// </summary>
public sealed class OperationContextHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IOperationContextAccessor _accessor;
    private readonly ILogger<OperationContextHeadersMiddleware> _logger;

    /// <summary>
    /// Cria o adapter de contexto de operação para HTTP.
    /// </summary>
    /// <param name="next">Próximo componente do pipeline.</param>
    /// <param name="accessor">Accessor do contexto neutro.</param>
    /// <param name="logger">Logger do adapter.</param>
    public OperationContextHeadersMiddleware(
        RequestDelegate next,
        IOperationContextAccessor accessor,
        ILogger<OperationContextHeadersMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Cria o contexto, executa a requisição e restaura o contexto anterior.
    /// </summary>
    /// <param name="context">Contexto HTTP atual.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var correlationId = context.Request.Headers.TryGetValue(Constants.CorrelationHeader, out var headerValue)
            ? headerValue.FirstOrDefault() ?? Guid.NewGuid().ToString("N")
            : Guid.NewGuid().ToString("N");
        var traceId = Activity.Current?.TraceId.ToString() ?? string.Empty;
        var operationId = Activity.Current?.Id ?? Guid.NewGuid().ToString("N");
        using var scope = new OperationContextScope(
            _accessor,
            new OperationContext(correlationId, traceId, operationId));

        context.Response.Headers[Constants.CorrelationHeader] = correlationId;
        _logger.LogDebug("HTTP operation context initialized for correlation {CorrelationId}.", correlationId);

        await _next(context);
    }
}

/// <summary>
/// Contém extensões para habilitar o contexto neutro em HTTP.
/// </summary>
public static class OperationContextHeadersMiddlewareExtensions
{
    /// <summary>
    /// Registra o accessor e o adapter HTTP de contexto de operação.
    /// </summary>
    /// <param name="services">Coleção de serviços da aplicação.</param>
    /// <returns>A mesma coleção para composição de registros.</returns>
    public static IServiceCollection AddOperationContextHeaders(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _ = services.AddSingleton<IOperationContextAccessor, OperationContextAccessor>();
        return services;
    }

    /// <summary>
    /// Adiciona o adapter de contexto neutro ao pipeline HTTP.
    /// </summary>
    /// <param name="builder">Builder da aplicação.</param>
    /// <returns>O mesmo builder para composição do pipeline.</returns>
    public static IApplicationBuilder UseOperationContextHeaders(this IApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.UseMiddleware<OperationContextHeadersMiddleware>();
    }
}
