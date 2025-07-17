using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.StandardHttpClient.Results;

namespace Nuuvify.CommonPack.StandardHttpClient.WebServices;

public partial class StandardWebService : IStandardWebService, IDisposable
{

    private readonly IHttpClientFactory _httpClientFactory;
    private HttpClient _httpClient;

    private readonly ILogger<StandardWebService> _logger;
    private readonly Dictionary<string, object> _headerStandard;
    private string _queryString;
    private HttpStandardXmlReturn _returnMessage;
    private bool _disposed = false;

    ///<inheritdoc/>
    public Uri FullUrl { get; private set; }

    public StandardWebService(
        IHttpClientFactory httpClientFactory,
        ILogger<StandardWebService> logger)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _queryString = string.Empty;

        _headerStandard = new Dictionary<string, object>();
    }

    ///<inheritdoc/>
    public void ResetStandardWebService()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
        FullUrl = null;

        _queryString = string.Empty;
        _headerStandard.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Clear();

        _returnMessage = new HttpStandardXmlReturn();

    }

    public void CreateClient(string namedClient = null)
    {
        if (string.IsNullOrWhiteSpace(namedClient))
            _httpClient = _httpClientFactory.CreateClient();
        else
            _httpClient = _httpClientFactory.CreateClient(namedClient);

    }

    ///<inheritdoc/>
    public void CreateHttp(Uri url)
    {
        _httpClient = _httpClientFactory.CreateClient();
        _httpClient.BaseAddress = url ?? throw new ArgumentException("Url deve ser informada", nameof(url));

    }

    public void CreateHttp(HttpClientHandler httpClientHandler, Uri uri)
    {
        _httpClient = new HttpClient(httpClientHandler, true)
        {
            BaseAddress = uri
        };
    }

    public void Configure(int timeOut)
    {
        _httpClient.Timeout = new TimeSpan(0, 0, timeOut);
    }

    ///<inheritdoc/>
    public IStandardWebService WithQueryString(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            return this;

        if (string.IsNullOrWhiteSpace(_queryString))
            _queryString = "?";

        var builder = new StringBuilder();
        _ = builder.Append(_queryString);

        if (!builder.ToString().Equals("?", StringComparison.Ordinal))
            _ = builder.Append("&");

        _ = builder.Append(System.Globalization.CultureInfo.InvariantCulture, $"{key}={value}");
        _queryString = builder.ToString();

        return this;
    }

    ///<inheritdoc/>
    public IStandardWebService WithCurrelationHeader(string correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId)) return this;
        if (!_headerStandard.TryGetValue(Constants.CorrelationHeader, out _))
        {
            _headerStandard.Add(Constants.CorrelationHeader, correlationId);
        }

        return this;
    }

    ///<inheritdoc/>
    public IStandardWebService WithHeader(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key)) return this;
        if (!_headerStandard.TryGetValue(key, out _))
        {
            _headerStandard.Add(key, value);
        }

        return this;
    }

    ///<inheritdoc/>
    public async Task<HttpStandardXmlReturn> RequestSoap(
        string urlRoute,
        XmlDocument soapEnvelopeXml,
        string accept = "text/xml",
        string contentType = "text/xml;charset=\"utf-8\"")
    {
        var url = $"{urlRoute}{_queryString}";

        if (soapEnvelopeXml == null || soapEnvelopeXml.InnerXml.Length == 0)
        {
            throw new ArgumentException("O xml do envelope soap não pode ser vazio", nameof(soapEnvelopeXml));
        }

        _ = WithHeader("Content-Type", contentType);

        _ = _httpClient.CustomRequestHeader(_headerStandard);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));
        return await StandardGetRequestStreamAsync(url, soapEnvelopeXml, accept);

    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _httpClient?.Dispose();
            }
            _disposed = true;
        }
    }

}

