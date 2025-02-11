using System.Diagnostics;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Middleware.Abstraction;

namespace Nuuvify.CommonPack.Middleware.Handle
{
    public class HandlingHeadersMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly ILogger<HandlingHeadersMiddleware> _logger;
        private readonly RequestConfiguration requestConfiguration;

        public HandlingHeadersMiddleware(
            RequestDelegate next,
            ILogger<HandlingHeadersMiddleware> logger,
            IOptions<RequestConfiguration> options)
        {
            _logger = logger;
            _next = next;

            requestConfiguration = options.Value;

        }

        public async Task Invoke(HttpContext context)
        {
            requestConfiguration.AppName = Assembly.GetEntryAssembly().GetName().Name;
            requestConfiguration.HostName ??= Dns.GetHostName();
            var guidStr = Guid.NewGuid().ToString();
            var guid12char = guidStr.Substring(guidStr.Length - 12);
            var correlationLocal = $"{requestConfiguration.AppName}_{requestConfiguration.HostName}_{guid12char}";


            if (context.Request.Headers.TryGetValue(Constants.CorrelationHeader, out StringValues value))
            {
                requestConfiguration.CorrelationId = value.FirstOrDefault() ?? correlationLocal;
            }
            else
            {
                requestConfiguration.CorrelationId = correlationLocal;
                context.Items[Constants.CorrelationHeader] = requestConfiguration.CorrelationId;
            }

            context.Items["X-AssemblyVersion"] = requestConfiguration.ApplicationVersion;
            context.Items["X-BuildNumber"] = requestConfiguration.BuildNumber;
            context.Response.Headers.Append("X-EnvironmentName", requestConfiguration.Environment);
            context.Response.Headers.Append("X-AssemblyVersion", requestConfiguration.ApplicationVersion);
            context.Response.Headers.Append("X-BuildNumber", requestConfiguration.BuildNumber);
            context.Response.Headers.Append(Constants.CorrelationHeader, requestConfiguration.CorrelationId);


            SetRequestConfiguration(context);


            _logger.LogInformation("### LOG DE ENTRADA DA REQUEST ###");
            await _next(context);
            _logger.LogInformation("### LOG DE SAIDA DA REQUEST ###");


        }

        /// <summary>
        /// Exemplo para leitura de body da request
        /// <example>
        /// <code>
        ///     context.Request.EnableRewind();
        ///     var buffer = new byte[Convert.ToInt32(context.Request.ContentLength)];
        ///     await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
        ///     var requestBody = Encoding.UTF8.GetString(buffer);
        ///     context.Request.Body.Seek(0, SeekOrigin.Begin);
        ///
        ///     if (!string.IsNullOrWhiteSpace(requestBody))
        ///         _logger.LogInformation("Corpo da request: {requestBody}", requestBody);
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        private void SetRequestConfiguration(HttpContext httpContext)
        {


            var executablePath = Environment.ProcessPath;
            var executable = Path.GetFileNameWithoutExtension(executablePath);
            string basePath;

            if ("dotnet".Equals(executable, StringComparison.InvariantCultureIgnoreCase))
            {
                basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }
            else
            {
                basePath = Path.GetDirectoryName(executablePath);
            }

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");


            requestConfiguration.SetAppVersion();
            requestConfiguration.Environment = environment ?? "Production";
            requestConfiguration.BasePath = basePath;

            httpContext.Request.Headers.TryGetValue(Constants.UserClaimHeader, out StringValues userClaimHeader);
            requestConfiguration.UserClaim = userClaimHeader.FirstOrDefault() ?? string.Empty;

            requestConfiguration.SetRequestData(
                httpContext?.Connection?.RemoteIpAddress?.MapToIPv4().ToString(),
                httpContext?.Connection?.RemotePort.ToString(),
                httpContext?.Connection?.LocalIpAddress?.MapToIPv4().ToString(),
                httpContext?.Connection?.LocalPort.ToString(),
                hostName: Dns.GetHostName());


        }

    }


    public static class HandlingHeadersMiddlewareExtensions
    {
        public static IApplicationBuilder UseHandlingHeadersMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HandlingHeadersMiddleware>();
        }
    }


}