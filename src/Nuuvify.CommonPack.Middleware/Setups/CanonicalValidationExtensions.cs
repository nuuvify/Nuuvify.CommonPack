using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Nuuvify.CommonPack.Middleware.Setups;

/// <summary>
/// Registra a resposta canônica de validação do ASP.NET Core.
/// </summary>
public static class CanonicalValidationExtensions
{
    /// <summary>
    /// Configura o pipeline MVC para responder a model state inválido com HTTP 400.
    /// </summary>
    /// <param name="services">Coleção de serviços da aplicação.</param>
    /// <returns>A mesma coleção para composição de registros.</returns>
    public static IServiceCollection AddCanonicalValidation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddControllers().ConfigureCanonicalValidation();
        return services;
    }

    /// <summary>
    /// Configura a resposta canônica sem registrar controllers novamente.
    /// </summary>
    /// <param name="services">Coleção de serviços da aplicação.</param>
    /// <returns>A mesma coleção para composição de registros.</returns>
    public static IServiceCollection ConfigureCanonicalValidation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.Configure<ApiBehaviorOptions>(ConfigureValidationOptions);
        return services;
    }

    /// <summary>
    /// Configura um builder MVC existente para responder a model state inválido com HTTP 400.
    /// </summary>
    /// <param name="mvcBuilder">Builder MVC já registrado pela aplicação.</param>
    /// <returns>O mesmo builder para composição de configurações.</returns>
    public static IMvcBuilder AddCanonicalValidation(this IMvcBuilder mvcBuilder)
    {
        ArgumentNullException.ThrowIfNull(mvcBuilder);

        _ = mvcBuilder.ConfigureApiBehaviorOptions(ConfigureValidationOptions);

        return mvcBuilder;
    }

    private static void ConfigureValidationOptions(ApiBehaviorOptions options)
    {
        options.SuppressModelStateInvalidFilter = false;
        options.InvalidModelStateResponseFactory = context =>
            new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            });
    }
}