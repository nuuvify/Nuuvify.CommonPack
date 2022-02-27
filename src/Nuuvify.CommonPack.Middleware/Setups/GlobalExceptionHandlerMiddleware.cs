using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Nuuvify.CommonPack.Middleware.Handle
{
    public class GlobalExceptionHandlerMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly IGlobalHandleException _globalHandleException;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, IGlobalHandleException globalHandleException)
        {
            _next = next;

            _globalHandleException = globalHandleException;
        }



        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);

            }
            catch (Exception ex)
            {

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                await _globalHandleException.HandleException(ex, context);
            }
        }

    }


    public static class GlobalExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandlerMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        }
    }


}
