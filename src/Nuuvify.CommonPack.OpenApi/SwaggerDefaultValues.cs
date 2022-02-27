using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;
using System.Reflection;

namespace Nuuvify.CommonPack.OpenApi
{

    public class SwaggerDefaultValues : IOperationFilter
    {

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Deprecated)
            {
                var obsoleteActions = context.MethodInfo.GetCustomAttributes<ObsoleteAttribute>();

                operation.Description = obsoleteActions.FirstOrDefault()?.Message;
            }
        }


    }

}
