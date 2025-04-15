using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.Extensions.Implementation;

namespace Nuuvify.CommonPack.StandardHttpClient;

///<inheritdoc/>
public partial class StandardHttpClient : IStandardHttpClient, IDisposable
{

    private readonly IHttpClientFactory _httpClientFactory;
    private HttpClient _httpClient;
    private HttpCompletionOption CompletionOption;
    private readonly ILogger<StandardHttpClient> _logger;
    private readonly Dictionary<string, string> _formParameter;
    private readonly Dictionary<string, object> _headerStandard;
    private readonly Dictionary<string, object> _headerAuthorization;
    private IDictionary<String, String> _queryString;

    public bool LogRequest { get; set; }
    public string AuthorizationLog { get; private set; }

    ///<inheritdoc/>
    public Uri FullUrl { get; private set; }
    ///<inheritdoc/>
    public string CorrelationId { get; private set; }

    public HttpResponseMessage CustomHttpResponseMessage { get; private set; }

    public StandardHttpClient(
        IHttpClientFactory httpClientFactory,
        ILogger<StandardHttpClient> logger)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _queryString = new Dictionary<String, String>();

        _headerAuthorization = new Dictionary<string, object>();
        _headerStandard = new Dictionary<string, object>();
        _formParameter = new Dictionary<string, string>();

    }

    ///<inheritdoc/>
    public void ResetStandardHttpClient()
    {
        _queryString = new Dictionary<String, String>();
        _headerAuthorization.Clear();
        _httpClient.DefaultRequestHeaders.Authorization = null;
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        CustomHttpResponseMessage = null;

        CorrelationId = string.Empty;
        FullUrl = null;
        _headerStandard.Clear();
    }

    ///<inheritdoc/>
    public void CreateClient(string namedClient = null)
    {
        if (string.IsNullOrWhiteSpace(namedClient))
            _httpClient = _httpClientFactory.CreateClient();
        else
            _httpClient = _httpClientFactory.CreateClient(namedClient);

    }
    public void CreateClient(HttpClientHandler httpClientHandler)
    {

        ArgumentNullException.ThrowIfNull(httpClientHandler);

        _httpClient = new HttpClient(httpClientHandler);

    }

    public void Configure(
        TimeSpan timeOut,
        long maxResponseContentBufferSize = default,
        HttpCompletionOption httpCompletionOption = HttpCompletionOption.Defult)
    {
        if (timeOut.TotalMilliseconds > 0)
            _httpClient.Timeout = timeOut;

        if (maxResponseContentBufferSize > 0)
            _httpClient.MaxResponseContentBufferSize = maxResponseContentBufferSize;

        CompletionOption = httpCompletionOption;
    }

    ///<inheritdoc/>
    public IStandardHttpClient WithQueryString(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            return this;

        _queryString.Add(key, value.ToString());

        return this;
    }

    ///<inheritdoc/>
    public IStandardHttpClient WithAuthorization(
        string schema = null,
        string token = null,
        string userClaim = null)
    {

        if (!string.IsNullOrWhiteSpace(schema) &&
            !string.IsNullOrWhiteSpace(token))
        {
            if (_headerAuthorization.TryGetValue(schema, out _))
            {
                _ = _headerAuthorization.Remove(schema);
            }
            _headerAuthorization.Add(schema, token);

        }

        if (!string.IsNullOrWhiteSpace(userClaim))
        {
            _ = WithHeader(Constants.UserClaimHeader, userClaim);
        }

        return this;
    }

    ///<inheritdoc/>
    public IStandardHttpClient WithCurrelationHeader(string correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId)) return this;
        if (!_headerStandard.TryGetValue(Constants.CorrelationHeader, out _))
        {
            _headerStandard.Add(Constants.CorrelationHeader, correlationId);
            CorrelationId = correlationId;
        }

        return this;
    }

    ///<inheritdoc/>
    public IStandardHttpClient WithHeader(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key)) return this;
        if (!_headerStandard.TryGetValue(key, out _))
        {
            _headerStandard.Add(key, value);
        }
        else if (Constants.UserClaimHeader.Equals(key, StringComparison.OrdinalIgnoreCase))
        {
            _ = _headerStandard.Remove(key);
            _headerStandard.Add(key, value);
        }

        return this;
    }

    public IStandardHttpClient WithFormParameter(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key)) return this;
        if (!_formParameter.TryGetValue(key, out _))
        {
            _formParameter.Add(key, value);
        }
        else if (Constants.UserClaimHeader.Equals(key, StringComparison.OrdinalIgnoreCase))
        {
            _ = _formParameter.Remove(key);
            _formParameter.Add(key, value);
        }

        return this;
    }

    public IStandardHttpClient WithHeader(KeyValuePair<string, string> header)
    {
        return WithHeader(header.Key, header.Value);
    }

    private bool _disposed = false;

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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~StandardHttpClient()
    {
        Dispose(false);
    }
}

