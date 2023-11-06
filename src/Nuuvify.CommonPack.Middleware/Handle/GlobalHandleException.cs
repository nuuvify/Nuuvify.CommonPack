using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

namespace Nuuvify.CommonPack.Middleware.Handle
{
    public class GlobalHandleException : IGlobalHandleException
    {
        private readonly ILogger<GlobalHandleException> _logger;
        public GlobalHandleException(
            ILogger<GlobalHandleException> logger)
        {
            _logger = logger;

        }




        public async Task HandleException(Exception ex, HttpContext context)
        {

            var mensagemRetorno = new ReturnStandardErrors
            {
                Success = false,
                Errors = new List<NotificationR> { new NotificationR { Message = " :( Ooops !! Houve uma exceção. There was an exception." } }
            };
            context.Response.ContentType = "application/json";


            await context.Response.WriteAsync(JsonSerializer.Serialize(mensagemRetorno));

            _logger.LogError(ex, " Global Exception");

        }
    }

}
