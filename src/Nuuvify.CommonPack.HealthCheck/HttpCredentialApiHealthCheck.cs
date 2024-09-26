using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nuuvify.CommonPack.HealthCheck.Helpers;

namespace Nuuvify.CommonPack.HealthCheck;

public class HttpCredentialApiHealthCheck : IHealthCheck
{

    public string SegmentSearch = "api/";

    public string ObjectCheckName = "CredentialApi";
    public string UrlHealthCheck = "hc-ui-api";
    private Uri UrlPrefix;

    private readonly JsonSerializerOptions jsonOptions;
    private readonly IHttpClientFactory _httpClientFactory;

    public HttpCredentialApiHealthCheck(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;

        jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
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

                UrlPrefix = client.BaseAddress.HasSegment(SegmentSearch, UrlHealthCheck);

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
            return await Task.FromResult(HealthCheckResult.Unhealthy($"{ObjectCheckName} Failed Exception", ex));
        }

    }

}
