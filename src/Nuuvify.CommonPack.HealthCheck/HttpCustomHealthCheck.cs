
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Nuuvify.CommonPack.HealthCheck;

/// <summary>
/// Faz uma chamada http para um serviço e retorna o status da chamada.
/// <p>Forceça os seguintes parâmetros: </p>
///   <p>Uri baseUri = exemplo: new Uri(builder.Configuration.GetSection("AppConfig:AppURLs:SynchroApi")?.Value), </p>
///   <p>string hcUrl = exemplo: "hc", </p>
///   <p>bool youWantReturnEndpointContent = exemplo: false, </p>
///   <p>HealthStatus failureStatus = exemplo: HealthStatus.Degraded, </p>
///   <p>HttpClientHandler httpClientHandler = exemplo: new MyHttpClientHandler(WebRequest.DefaultWebProxy).MyClientHandler </p>
/// </summary>
public class HttpCustomHealthCheck : IHealthCheck
{

    private readonly string _hcUrl;
    private readonly HealthStatus _failureStatus;
    private readonly bool _youWantReturnEndpointContent;

    private readonly HttpClient _httpClient;
    private readonly Func<HttpClient> _httpClientFactory;

    public HttpCustomHealthCheck(
        Uri baseUri,
        string hcUrl,
        bool youWantReturnEndpointContent,
        HealthStatus failureStatus,
        HttpClientHandler httpClientHandler)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hcUrl);
        _youWantReturnEndpointContent = youWantReturnEndpointContent;
        _hcUrl = hcUrl;
        _failureStatus = failureStatus;

        _httpClientFactory = () => new HttpClient(httpClientHandler)
        {
            BaseAddress = baseUri
        };

        _httpClient = _httpClientFactory.Invoke();

    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {

        var resultData = new Dictionary<string, object>();

        try
        {

            HealthCheckResult checkResult;
            var httpReturn = await _httpClient.GetAsync(_hcUrl, cancellationToken);
            var contentReturn = httpReturn.Content.ReadAsStringAsync(cancellationToken).Result;
            _ = int.TryParse(httpReturn.StatusCode.ToString(), out int returnCode);

            resultData = new Dictionary<string, object>
            {
                ["Uri: "] = _httpClient.BaseAddress,
                [$"Return Code: {returnCode} / {httpReturn.ReasonPhrase}"] =
                    _youWantReturnEndpointContent ? contentReturn : "Content received",
            };

            if (httpReturn.IsSuccessStatusCode)
            {

                if (httpReturn.ReasonPhrase != "OK" ||
                    returnCode >= 400)
                {

                    checkResult = _failureStatus.Equals(HealthStatus.Unhealthy) ?
                        HealthCheckResult.Unhealthy(
                            description: $"{_hcUrl} {nameof(HealthStatus.Unhealthy)}",
                            data: resultData) :
                        HealthCheckResult.Degraded(
                            description: $"{_hcUrl} {nameof(HealthStatus.Degraded)}",
                            data: resultData);

                }
                else
                {
                    checkResult = HealthCheckResult.Healthy(
                        description: $"{_hcUrl} {nameof(HealthStatus.Healthy)}",
                        data: resultData);
                }

            }
            else
            {

                checkResult = _failureStatus.Equals(HealthStatus.Unhealthy) ?
                    HealthCheckResult.Unhealthy(
                        description: $"{_hcUrl} {nameof(HealthStatus.Unhealthy)}",
                        data: resultData) :
                    HealthCheckResult.Degraded(
                        description: $"{_hcUrl} {nameof(HealthStatus.Degraded)}",
                        data: resultData);

            }

            return await Task.FromResult(checkResult);

        }
        catch (Exception ex)
        {
            return await Task.FromResult(
                HealthCheckResult.Unhealthy(
                    description: $"{_hcUrl} Failed Exception",
                    exception: ex,
                    data: resultData));
        }

    }

}
