using System.Net;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;

namespace Nuuvify.CommonPack.StandardHttpClient.Polly
{
    public static class HttpCircuitBreakerFallBackPolicies
    {

        public static AsyncFallbackPolicy<HttpResponseMessage> GetHttpFallBackPolicy(HttpRequestMessage request, ILogger logger)
        {
            return HttpPolicyBuilders.GetBaseBuilder()
                .OrInner<BrokenCircuitException>()
                .FallbackAsync(
                    fallbackAction: (responseToFailedRequest, context, cancellationToken) =>
                    {
                        return FallbackAction(responseToFailedRequest, context, logger, request, cancellationToken);
                    },
                    onFallbackAsync: (responseToFailedRequest, context) =>
                    {
                        return OnFallbackAsync(responseToFailedRequest, context, logger, request);
                    });

        }

        private static Task OnFallbackAsync(DelegateResult<HttpResponseMessage> response, Context context, ILogger logger, HttpRequestMessage request)
        {
            context = request.GetPolicyExecutionContext();
            var serviceBreak = context.GetServiceName();

            logger.LogWarning("###### OnFallbackAsync was triggered, service: {serviceBreak} failed ######", serviceBreak);

            return Task.CompletedTask;
        }

        private static Task<HttpResponseMessage> FallbackAction(DelegateResult<HttpResponseMessage> responseToFailedRequest, Context context, ILogger logger, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            context = request.GetPolicyExecutionContext();
            var serviceBreak = context.GetServiceName();

            logger.LogWarning("###### FallbackAction was triggered, service: {serviceBreak} failed, customized warning message is being returned. ######", serviceBreak);

            HttpResponseMessage httpResponseMessage;


            if (responseToFailedRequest?.Result is null)
            {
                httpResponseMessage = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    Content = new StringContent($"###### The fallback executed, the service: {serviceBreak} is down, please wait and try again later. ######")
                };

            }
            else
            {
                httpResponseMessage = new HttpResponseMessage(responseToFailedRequest.Result.StatusCode)
                {
                    Content = new StringContent($"###### The fallback executed, the original error was: {responseToFailedRequest.Result.ReasonPhrase} ######")
                };

            }


            return Task.FromResult(httpResponseMessage);
        }

    }
}