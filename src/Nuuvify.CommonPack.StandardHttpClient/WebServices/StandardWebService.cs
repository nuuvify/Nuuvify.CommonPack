using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.StandardHttpClient.Results;

namespace Nuuvify.CommonPack.StandardHttpClient.WebServices
{

    public partial class StandardWebService : IStandardWebService
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private HttpClient _httpClient;

        private readonly ILogger<StandardWebService> _logger;
        private readonly Dictionary<string, object> _headerStandard;
        private string _queryString;
        private HttpStandardXmlReturn _returnMessage;

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
            _returnMessage = new HttpStandardXmlReturn();

        }

        ///<inheritdoc/>
        public void CreateHttp(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("Url deve ser informada", url);
            }
            var options = new UriCreationOptions();

            if (Uri.TryCreate(url, options, out Uri uri))
            {
                _httpClient = _httpClientFactory.CreateClient();
                _httpClient.BaseAddress = uri;
            }
            else
            {
                throw new ArgumentException("Parametro informado não é uma url valida", url);
            }


        }

        ///<inheritdoc/>
        public void CreateHttp(HttpWebRequest httpWebRequest)
        {
            var httpClientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = httpWebRequest.AllowAutoRedirect,
                AutomaticDecompression = httpWebRequest.AutomaticDecompression,
                Credentials = httpWebRequest.Credentials,
                Proxy = httpWebRequest.Proxy,
                MaxAutomaticRedirections = httpWebRequest.MaximumAutomaticRedirections,
                MaxResponseHeadersLength = httpWebRequest.MaximumResponseHeadersLength,
                UseDefaultCredentials = httpWebRequest.UseDefaultCredentials
            };


            _httpClient = new HttpClient(httpClientHandler, true)
            {
                BaseAddress = httpWebRequest.Address
            };

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
            builder.Append(_queryString);


            if (!builder.ToString().Equals("?"))
                builder.Append("&");

            builder.Append($"{key}={value}");


            _queryString = builder.ToString();

            return this;
        }


        ///<inheritdoc/>
        public IStandardWebService WithCurrelationHeader(string correlationId)
        {
            if (string.IsNullOrWhiteSpace(correlationId)) return this;

            if (!_headerStandard.TryGetValue(Constants.CorrelationHeader, out object valueObject))
            {
                _headerStandard.Add(Constants.CorrelationHeader, correlationId);
            }

            return this;
        }

        ///<inheritdoc/>
        public IStandardWebService WithHeader(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key)) return this;

            if (!_headerStandard.TryGetValue(key, out object valueObject))
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


            WithHeader("Accept", new MediaTypeWithQualityHeaderValue(accept));
            WithHeader("Content-Type", contentType);


            _httpClient.CustomRequestHeader(_headerStandard);


            return await StandardGetRequestStreamAsync(url, soapEnvelopeXml);

        }


    }
}
