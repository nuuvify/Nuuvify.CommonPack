using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.StandardHttpClient.Results;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

namespace Nuuvify.CommonPack.StandardHttpClient
{

    ///<inheritdoc/>
    public class StandardHttpClient : IStandardHttpClient
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private HttpClient _httpClient;
        private HttpCompletionOption CompletionOption;
        private readonly ILogger<StandardHttpClient> _logger;
        private readonly Dictionary<string, object> _headerStandard;
        private readonly Dictionary<string, object> _headerAuthorization;
        private QueryBuilder _queryString;



        ///<inheritdoc/>
        public Uri FullUrl { get; private set; }
        ///<inheritdoc/>
        public string CorrelationId { get; private set; }




        public StandardHttpClient(
            IHttpClientFactory httpClientFactory,
            ILogger<StandardHttpClient> logger)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _queryString = new QueryBuilder();

            _headerAuthorization = new Dictionary<string, object>();
            _headerStandard = new Dictionary<string, object>();
        }


        ///<inheritdoc/>
        public void ResetStandardHttpClient()
        {
            _queryString = new QueryBuilder();
            _headerAuthorization.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = null;
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

        public void Configure(TimeSpan timeOut, long maxResponseContentBufferSize = default, HttpCompletionOption httpCompletionOption = HttpCompletionOption.Defult)
        {
            if (timeOut.TotalMilliseconds > 0)
                _httpClient.Timeout = timeOut;

            if (maxResponseContentBufferSize > 0)
                _httpClient.MaxResponseContentBufferSize = maxResponseContentBufferSize;

            CompletionOption = httpCompletionOption;
        }


        private async Task<HttpStandardReturn> HandleResponseMessage(HttpResponseMessage response)
        {
            var returnMessage = new HttpStandardReturn();

            if (response is null) return returnMessage;


            var resultNumber = (int)response.StatusCode;

            returnMessage.ReturnCode = resultNumber.ToString();
            returnMessage.Success = response.IsSuccessStatusCode;

            var content = await response.Content.ReadAsStringAsync();


            returnMessage.ReturnMessage = content;


            return returnMessage;
        }
        private async Task<HttpStandardStreamReturn> HandleResponseMessageStream(HttpResponseMessage response)
        {
            var returnMessage = new HttpStandardStreamReturn();

            if (response is null) return returnMessage;


            var resultNumber = (int)response.StatusCode;

            returnMessage.ReturnCode = resultNumber.ToString();
            returnMessage.Success = response.IsSuccessStatusCode;

            var content = await response.Content.ReadAsStreamAsync();


            returnMessage.ReturnMessage = content;


            return returnMessage;
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
        public IStandardHttpClient WithAuthorization(string schema = "bearer", string token = null, string userClaim = null)
        {

            if (!string.IsNullOrWhiteSpace(schema) &&
                !string.IsNullOrWhiteSpace(token))
            {

                if (_headerAuthorization.TryGetValue("Authorization", out object valueObject))
                {
                    _headerAuthorization.Remove("Authorization");
                }
                _headerAuthorization.Add("Authorization", $"{schema} {token}");

            }

            if (!string.IsNullOrWhiteSpace(userClaim))
            {
                WithHeader(Constants.UserClaimHeader, userClaim);
            }

            return this;
        }

        ///<inheritdoc/>
        public IStandardHttpClient WithCurrelationHeader(string correlationId)
        {
            if (string.IsNullOrWhiteSpace(correlationId)) return this;

            if (!_headerStandard.TryGetValue(Constants.CorrelationHeader, out object valueObject))
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



            if (!_headerStandard.TryGetValue(key, out object valueObject))
            {
                _headerStandard.Add(key, value);
            }
            else if (Constants.UserClaimHeader.Equals(key, StringComparison.InvariantCultureIgnoreCase))
            {
                _headerStandard.Remove(key);
                _headerStandard.Add(key, value);
            }


            return this;
        }

        public IStandardHttpClient WithHeader(KeyValuePair<string, string> header)
        {
            return WithHeader(header.Key, header.Value);
        }

        private async Task<HttpStandardReturn> StandardSendAsync(string url, HttpRequestMessage message)
        {
            _logger.LogDebug("Url and message before config {0} {1}", message, _httpClient.BaseAddress);



            if (!string.IsNullOrWhiteSpace(_httpClient?.BaseAddress?.AbsoluteUri) &&
                _httpClient.BaseAddress.IsAbsoluteUri)
            {
                var urlBase = new Uri(_httpClient.BaseAddress.AbsoluteUri, UriKind.Absolute);
                if (Uri.TryCreate(urlBase, url, out Uri result))
                {
                    FullUrl = result;
                }
                else
                {
                    _logger.LogWarning("Url base e relativa informada esta invalido Base: {0} Relativa: {1}", _httpClient?.BaseAddress?.AbsoluteUri, url);
                    return null;
                }
            }
            else
            {
                if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri result))
                {
                    FullUrl = result;
                }
                else
                {
                    _logger.LogWarning("Url informada é invalida {0}", url);
                    return null;
                }
            }

            _logger.LogDebug("Url and message after config {0}", message);


            HttpResponseMessage response;


            if (HttpCompletionOption.Defult != CompletionOption)
            {
                System.Net.Http.HttpCompletionOption HttpOption;
                HttpOption = (System.Net.Http.HttpCompletionOption)CompletionOption;

                response = await _httpClient.SendAsync(message, HttpOption);

            }
            else
            {
                response = await _httpClient.SendAsync(message);
            }


            HttpStandardReturn httpStandardReturn = await HandleResponseMessage(response);


            _logger.LogDebug("HttpStandardReturn return: {0}", httpStandardReturn.ReturnCode);


            return httpStandardReturn;
        }
        private async Task<HttpStandardStreamReturn> StandardStreamSendAsync(string url, HttpRequestMessage message)
        {
            _logger.LogDebug("Url and message before config {0} {1}", message, _httpClient.BaseAddress);


            if (!string.IsNullOrWhiteSpace(_httpClient?.BaseAddress?.AbsoluteUri) &&
                _httpClient.BaseAddress.IsAbsoluteUri)
            {
                var urlBase = new Uri(_httpClient.BaseAddress.AbsoluteUri, UriKind.Absolute);
                if (Uri.TryCreate(urlBase, url, out Uri result))
                {
                    FullUrl = result;
                }
                else
                {
                    _logger.LogWarning("Url base e relativa informada esta invalido Base: {0} Relativa: {1}", _httpClient?.BaseAddress?.AbsoluteUri, url);
                    return null;
                }
            }
            else
            {
                if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri result))
                {
                    FullUrl = result;
                }
                else
                {
                    _logger.LogWarning("Url informada é invalida {0}", url);
                    return null;
                }
            }

            _logger.LogDebug("Url and message after config {0}", message);


            HttpResponseMessage response;


            if (HttpCompletionOption.Defult != CompletionOption)
            {
                System.Net.Http.HttpCompletionOption HttpOption;
                HttpOption = (System.Net.Http.HttpCompletionOption)CompletionOption;

                response = await _httpClient.SendAsync(message, HttpOption);

            }
            else
            {
                response = await _httpClient.SendAsync(message);
            }


            HttpStandardStreamReturn httpStandardStreamReturn = await HandleResponseMessageStream(response);


            _logger.LogDebug("HttpStandardReturn return: {0}", httpStandardStreamReturn.ReturnCode);


            return httpStandardStreamReturn;

        }

        ///<inheritdoc/>
        public async Task<HttpStandardStreamReturn> GetStream(string urlRoute)
        {
            var url = $"{urlRoute}{_queryString.ToQueryString()}";



            var message = new HttpRequestMessage(HttpMethod.Get, url)
                .CustomRequestHeader(_headerStandard)
                .AddAuthorizationHeader(_headerAuthorization);

            return await StandardStreamSendAsync(url, message);
        }

        public async Task<HttpStandardReturn> Get(string urlRoute)
        {
            var url = $"{urlRoute}{_queryString.ToQueryString()}";



            var message = new HttpRequestMessage(HttpMethod.Get, url)
                .CustomRequestHeader(_headerStandard)
                .AddAuthorizationHeader(_headerAuthorization);

            return await StandardSendAsync(url, message);
        }

        public async Task<HttpStandardReturn> Post(string urlRoute, object messageBody)
        {
            var url = $"{urlRoute}{_queryString.ToQueryString()}";


            var message = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(messageBody),
                    Encoding.UTF8, "application/json"
                )
            }
            .CustomRequestHeader(_headerStandard)
            .AddAuthorizationHeader(_headerAuthorization);


            return await StandardSendAsync(url, message);

        }

        ///<inheritdoc/>
        public async Task<HttpStandardReturn> Post(string urlRoute, object messageBody, string mediaType)
        {
            var url = $"{urlRoute}{_queryString.ToQueryString()}";



            var message = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    messageBody.ToString(),
                    Encoding.UTF8,
                    mediaType
                )
            }
            .CustomRequestHeader(_headerStandard)
            .AddAuthorizationHeader(_headerAuthorization);


            return await StandardSendAsync(url, message);

        }

        ///<inheritdoc/>
        public async Task<HttpStandardReturn> Post(string urlRoute, MultipartFormDataContent messageBody, string mediaType = "multipart/form-data")
        {
            var url = $"{urlRoute}{_queryString.ToQueryString()}";


            WithHeader("Accept", new MediaTypeWithQualityHeaderValue(mediaType));

            var message = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = messageBody
            }
            .CustomRequestHeader(_headerStandard)
            .AddAuthorizationHeader(_headerAuthorization);


            return await StandardSendAsync(url, message);

        }

        public async Task<HttpStandardReturn> Put(string urlRoute, object messageBody)
        {

            var url = $"{urlRoute}{_queryString.ToQueryString()}";


            var message = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(messageBody),
                    Encoding.UTF8, "application/json"
                )
            }
            .CustomRequestHeader(_headerStandard)
            .AddAuthorizationHeader(_headerAuthorization);

            return await StandardSendAsync(url, message);

        }

        public async Task<HttpStandardReturn> Patch(string urlRoute, object messageBody)
        {

            var patch = await Put(urlRoute, messageBody);
            return patch;

        }

        public async Task<HttpStandardReturn> Delete(string urlRoute)
        {
            var url = $"{urlRoute}{_queryString.ToQueryString()}";


            var message = new HttpRequestMessage(HttpMethod.Delete, url)
                .CustomRequestHeader(_headerStandard)
                .AddAuthorizationHeader(_headerAuthorization);


            return await StandardSendAsync(url, message);

        }


    }
}
