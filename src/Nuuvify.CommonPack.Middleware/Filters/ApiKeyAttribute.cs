using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.Extensions.Notificator;
using Nuuvify.CommonPack.Middleware.Abstraction.Results;

namespace Nuuvify.CommonPack.Middleware.Filters
{

    /// <summary>
    /// Se for utilizar ApiKey junto com Authorize, em sua Controller ou Action, informe esse atributo <br/>
    /// [ApiKey()] acima do [Authorize] <br/>
    /// [ApiKey()] também pode ser utilizado sem [Authorize] <br/>
    /// O parametro "KeyName" deve corresponder, obrigatoriamente a uma entrada no serviço de VAULT <br/>
    /// [ApiKey("KeyName")] = "xxxxxxxxxxxxx"
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Class |
        AttributeTargets.Method, AllowMultiple = true)]
    public class ApiKeyAttribute : Attribute, IFilterFactory
    {
        public string KeyName { get; set; }

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var logger = (ILogger)serviceProvider.GetService(typeof(ILogger));
            var configuration = (IConfiguration)serviceProvider.GetService(typeof(IConfiguration));

            var apiKeyFilter = new ApiKeyFilter(logger, configuration, KeyName);
            return apiKeyFilter;
        }

        public bool IsReusable => false;

    }


    public class ApiKeyFilter : IResourceFilter, IActionFilter, IExceptionFilter
    {
        private readonly string _keyName;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public ApiKeyFilter(
            ILogger logger,
            IConfiguration configuration,
            string keyName)
        {
            _logger = logger;
            _configuration = configuration;
            _keyName = keyName;

        }


        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            _logger.LogDebug("Run: [Before] IResourceFilter.OnResourceExecuting.");

        }


        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            _logger.LogDebug("Run: [After] IResourceFilter.OnResourceExecuting.");

        }


        public void OnActionExecuting(ActionExecutingContext context)
        {

            try
            {

                if (_configuration == null)
                {
                    throw new ArgumentException($"{nameof(IConfiguration)} is null");
                }
                if (string.IsNullOrWhiteSpace(_keyName))
                {
                    throw new ArgumentException($"{nameof(_keyName)} is null");
                }


                string apiKey = string.Empty;

                if (context.HttpContext.Request.Headers.TryGetValue(_keyName, out var headerApiKey))
                {
                    apiKey = _configuration.GetSection(_keyName)?.Value;
                }

                if (apiKey != headerApiKey)
                {

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

                }


            }
            catch (Exception)
            {
                throw;
            }


        }


        public void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.LogDebug("Run: [After] IActionFilter.OnActionExecuting.");

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
}

