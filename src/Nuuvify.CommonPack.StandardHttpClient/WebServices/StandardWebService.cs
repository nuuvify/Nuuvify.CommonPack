using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
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
        private HttpStandardReturn _returnMessage;

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
            _returnMessage = new HttpStandardReturn();

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



        public async Task<HttpStandardReturn> RequestSoap(
            string urlRoute,
            StandardHttpMethods method,
            XmlDocument messageBody,
            string mediaType = "application/xml")
        {

            return await Task.FromResult(new HttpStandardReturn()
            {
                Success = false,
                ReturnCode = "400",
                ReturnMessage = "Esse metodo foi descontinuado, use o novo metodo RequestSoap"
            });
        }


        ///<inheritdoc/>
        public async Task<HttpStandardReturn> RequestSoap(
            string urlRoute,
            XmlDocument messageBody,
            string xmlDocumentmlResponseDocumentContains,
            string xmlGetElementsByTagName,
            string mediaType = "application/xml")
        {
            var url = $"{urlRoute}{_queryString}";


            if (string.IsNullOrWhiteSpace(xmlDocumentmlResponseDocumentContains) ||
                string.IsNullOrWhiteSpace(xmlGetElementsByTagName))
            {
                throw new ArgumentException($"Os parametros {nameof(xmlDocumentmlResponseDocumentContains)} e {nameof(xmlGetElementsByTagName)} são obrigatorios");
            }


            WithHeader("Accept", new MediaTypeWithQualityHeaderValue("text/xml"));
            WithHeader("Content-Type", mediaType);


            _httpClient.CustomRequestHeader(_headerStandard);


            return await StandardGetRequestStreamAsync(url, messageBody);

        }


    }
}
