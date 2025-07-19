using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Nuuvify.CommonPack.StandardHttpClient.Polly
{
    public static class HttpRetryPolicies
    {
        public static AsyncRetryPolicy<HttpResponseMessage> GetHttpResponseRetryPolicy(HttpRequestMessage request, ILogger logger, IRetryPolicyConfig retryPolicyConfig)
        {
            int retryNum = 0;

            var httpResponseMessage = HttpPolicyBuilders.GetBaseBuilder()
                .WaitAndRetryAsync(
                    retryCount: retryPolicyConfig.RetryCount,
                    sleepDurationProvider: attemp => PollyHelpers.ComputeDuration(attemp),
                    onRetryAsync: async (message, retrySleep, context) =>
                    {
                        retryNum++;
                        await OnHttpRetry(message, request, retrySleep, retryNum, retryPolicyConfig.RetryCount, context, logger);

                        if (retryNum > retryPolicyConfig.RetryCount) retryNum = 0;
                    });

            if (request.Headers.Authorization?.Scheme == "bearer" &&
                string.IsNullOrWhiteSpace(request.Headers.Authorization.Parameter))
                request.Headers.Authorization = null;

            return httpResponseMessage;
        }

        private static async Task OnHttpRetry(DelegateResult<HttpResponseMessage> message, HttpRequestMessage request, TimeSpan retrySleep, int retryNum, int retryTotal, Context context, ILogger logger)
        {

            var messageLog = $"{nameof(GetHttpResponseRetryPolicy)} Request failed with StatusCode: {message?.Result?.StatusCode}. Waiting retrySleep: {retrySleep} before next retry. Retry attempt {retryNum}/{retryTotal} Context CorrelationId: {context.CorrelationId} Request: {request?.RequestUri}";

            var serviceBreak = request?.RequestUri.ToString();
            context.SetServiceName(serviceBreak);
            request.SetPolicyExecutionContext(context);


            if (message?.Exception?.Message != null)
            {

                logger.LogWarning("{messageLog} - Request failed because network failure: {messageEx}", messageLog, message?.Exception?.Message);
            }
            else
            {
                logger.LogWarning("{messageLog}", messageLog);
            }

            await Task.CompletedTask;
        }



    }
}