using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Nuuvify.CommonPack.Extensions.Notificator;
using Nuuvify.CommonPack.Middleware.Abstraction.Results;

namespace Nuuvify.CommonPack.Middleware.Filters
{

    /// <summary>
    /// Se for utilizar ApiKey em sua Controller ou Action, informe esse atributo <br/>
    /// [ApiKey] acima do [Authorize] <br/>
    /// Se usar esse atributo, obrigatoriamente devera ter uma entrada no serviço de VAULT <br/>
    /// ApiKey = "xxxxxxxxxxxxx"
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAttribute : TypeFilterAttribute
    {

        public ApiKeyAttribute()
        : base(typeof(ApiKeyAttributeImpl))
        {

        }


    }



    public class ApiKeyAttributeImpl : IAsyncActionFilter
    {
        private const string Name = "ApiKey";
        private readonly IConfiguration _configuration;

        public ApiKeyAttributeImpl(IConfiguration configuration)
        {
            _configuration = configuration;
        }



        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {

            try
            {

                if (context.HttpContext.Request.Headers.TryGetValue(Name, out var headerApiKey))
                {
                    if (_configuration == null)
                    {
                        throw new ArgumentException($"{nameof(IConfiguration)} is null");
                    }


                    var apiKey = _configuration.GetSection(Name)?.Value;

                    if (apiKey != headerApiKey)
                    {

                        var notification = new NotificationR(nameof(Name), "ApiKey informed in the request header, is not valid, or does not exist in the Vault of this application.");
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
                }
            }
            catch (Exception)
            {
                throw;
            }

            await next();
        }
    }
}