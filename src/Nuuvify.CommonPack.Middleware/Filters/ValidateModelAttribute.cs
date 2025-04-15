using Microsoft.AspNetCore.Mvc;

namespace Nuuvify.CommonPack.Middleware.Filters;

public sealed partial class ValidateModelStateCustomAttribute : TypeFilterAttribute
{
    public ValidateModelStateCustomAttribute()
        : base(typeof(ValidateModelAttributeCustom))
    {
    }
}
