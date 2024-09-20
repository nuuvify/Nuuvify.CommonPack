using System.Net;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Nuuvify.CommonPack.StandardHttpClient.Polly
{
    public static class HttpRetryWithTokenPolicies
    {
        public static AsyncRetryPolicy<HttpResponseMessage> GetHttpResponseRetryPolicyWithToken(HttpRequestMessage request, ILogger logger, ITokenService tokenService, IRetryPolicyConfig retryPolicyConfig)
        {
            int retryNum = 0;

            var httpReponseMessage = HttpPolicyBuilders.GetBaseBuilder()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.Unauthorized)
                .WaitAndRetryAsync(
                    retryCount: retryPolicyConfig.RetryCount,
                    sleepDurationProvider: attemp => PollyHelpers.ComputeDuration(attemp),
                    onRetryAsync: async (message, retrySleep, context) =>
                    {
                        retryNum++;
                        await OnHttpRetryWithToken(message, request, tokenService, retrySleep, retryNum, retryPolicyConfig.RetryCount, context, logger);

                        if (retryNum > retryPolicyConfig.RetryCount) retryNum = 0;
                    });

            if (request.Headers.Authorization?.Scheme == "bearer" &&
                string.IsNullOrWhiteSpace(request.Headers.Authorization.Parameter))
                request.Headers.Authorization = null;

            return httpReponseMessage;
        }

        private static async Task OnHttpRetryWithToken(
            DelegateResult<HttpResponseMessage> message,
            HttpRequestMessage request,
            ITokenService tokenService,
            TimeSpan retrySleep,
            int retryNum,
            int retryTotal,
            Context context,
            ILogger logger)
        {

            var messageLog = $"{nameof(GetHttpResponseRetryPolicyWithToken)} Request with token failed with StatusCode: {message?.Result?.StatusCode}. Waiting retrySleep: {retrySleep} before next retry. Retry attempt {retryNum}/{retryTotal} Context CorrelationId: {context.CorrelationId} Request: {request?.RequestUri}";

            var serviceBreak = request?.RequestUri.ToString();
            context.SetServiceName(serviceBreak);
            request.SetPolicyExecutionContext(context);


            if (message?.Result?.StatusCode == HttpStatusCode.Unauthorized)
            {
                logger.LogWarning("{messageLog} - Before ITokenService.GetToken", messageLog);

                if (retryNum == 1)
                {
                    tokenService.GetTokenAcessor();
                }

                if (string.IsNullOrWhiteSpace(tokenService.GetActualToken()?.Token) || retryNum != 1)
                {
                    await tokenService.GetToken();
                }

                request.AddAuthorizationHeader("bearer", tokenService.GetActualToken()?.Token);

                logger.LogWarning("{messageLog} - After ITokenService.GetToken", messageLog);
            }
            else if (message?.Exception?.Message != null)
            {

                logger.LogWarning("{messageLog} - Request with token failed because network failure: {messageEx}", messageLog, message?.Exception?.Message);
            }
            else
            {
                logger.LogInformation("{messageLog}", messageLog);
            }

            await Task.CompletedTask;
        }


    }
}