using Microsoft.AspNetCore.Mvc;

namespace Nuuvify.CommonPack.Middleware.Filters;

[Obsolete("Use [ApiController] e ValidationProblemDetails. Consulte README.md#validação.", error: false)]
public sealed partial class ValidateModelStateCustomAttribute : TypeFilterAttribute
{
    public ValidateModelStateCustomAttribute()
        : base(typeof(ValidateModelAttributeCustom))
    {
    }
}
