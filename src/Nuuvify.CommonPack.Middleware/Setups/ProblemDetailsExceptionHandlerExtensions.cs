using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Nuuvify.CommonPack.Middleware.Setups;

/// <summary>
/// Contém extensões para registrar o tratamento moderno de exceções.
/// </summary>
public static class ProblemDetailsExceptionHandlerExtensions
{
    /// <summary>
    /// Registra Problem Details e o handler global de exceções.
    /// </summary>
    /// <param name="services">Coleção de serviços da aplicação.</param>
    /// <returns>A mesma coleção para composição de registros.</returns>
    public static IServiceCollection AddProblemDetailsExceptionHandler(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddProblemDetails();
        _ = services.AddExceptionHandler<ProblemDetailsExceptionHandler>();
        return services;
    }
}
