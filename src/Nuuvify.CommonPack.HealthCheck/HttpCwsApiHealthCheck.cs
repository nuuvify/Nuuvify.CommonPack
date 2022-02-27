using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Nuuvify.CommonPack.HealthCheck.Helpers;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Nuuvify.CommonPack.HealthCheck
{
    public class HttpCwsApiHealthCheck : IHealthCheck
    {

        private const string ObjectCheckName = "CwsApi";
        private const string UrlHealthCheck = "hc-ui-api";
        private Uri UrlPrefix;

        private readonly JsonSerializerOptions jsonOptions;
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpCwsApiHealthCheck(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;

            jsonOptions = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                PropertyNameCaseInsensitive = true,
            };

        }


        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {

            try
            {

                HealthCheckResult checkResult;

                using (HttpClient client = _httpClientFactory.CreateClient(ObjectCheckName))
                {

                    UrlPrefix = client.BaseAddress.HasSegment("api/", UrlHealthCheck);

                    var response = await client.GetAsync(UrlPrefix.LocalPath);


                    if (response.IsSuccessStatusCode)
                    {
                        var resultHttp = await response.Content.ReadAsStringAsync();
                        if (resultHttp == null)
                        {
                            checkResult = HealthCheckResult.Degraded($"{ObjectCheckName} {nameof(HealthStatus.Degraded)}");
                        }
                        else
                        {

                            var jsonData = JsonSerializer.Deserialize<IEnumerable<HealthReportCustom>>(
                                resultHttp, jsonOptions);

                            var healthReport = jsonData.FirstOrDefault();


                            checkResult = healthReport.Status switch
                            {
                                nameof(HealthStatus.Healthy) => HealthCheckResult.Healthy($"{ObjectCheckName} {healthReport.Status}",
                                    data: healthReport.DataEntries()),
                                nameof(HealthStatus.Degraded) => HealthCheckResult.Degraded($"{ObjectCheckName} {healthReport.Status}",
                                    data: healthReport.DataEntries()),
                                _ => HealthCheckResult.Unhealthy($"{ObjectCheckName} {healthReport.Status}",
                                    data: healthReport.DataEntries()),
                            };
                        }
                    }
                    else
                    {
                        var requestResultFalse = new Dictionary<string, object>();
                        requestResultFalse.TryAdd($"Url responding with {response.StatusCode} {response.RequestMessage}", response.ToString());

                        checkResult = HealthCheckResult.Degraded($"{ObjectCheckName} {nameof(HealthStatus.Degraded)}",
                            data: requestResultFalse);

                    }

                }

                return await Task.FromResult(checkResult);

            }
            catch (Exception ex)
            {
                return await Task.FromResult(HealthCheckResult.Unhealthy($"{ObjectCheckName} Failed", ex));
            }

        }

    }
}