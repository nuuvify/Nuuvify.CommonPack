using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.StandardHttpClient.Results;

namespace Nuuvify.CommonPack.StandardHttpClient.WebServices
{

    public class StandardWebService : IStandardWebService
    {

        private HttpWebRequest _httpWebRequest;
        private readonly ILogger<StandardWebService> _logger;
        private readonly Dictionary<string, object> _headerStandard;
        private string _queryString;
        private HttpStandardReturn _returnMessage;

        ///<inheritdoc/>
        public Uri FullUrl { get; private set; }


        public StandardWebService(
            ILogger<StandardWebService> logger)
        {
            _logger = logger;
            _queryString = string.Empty;

            _headerStandard = new Dictionary<string, object>();
        }


        ///<inheritdoc/>
        public void ResetStandardWebService()
        {
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

            _httpWebRequest = WebRequest.CreateHttp(url);

        }
        ///<inheritdoc/>
        public void CreateHttp(HttpWebRequest httpWebRequest)
        {
            _httpWebRequest = httpWebRequest;
        }

        public void Configure(int timeOut)
        {
            _httpWebRequest.Timeout = timeOut;
        }


        private void GetStreamReader(Stream data)
        {

            using (var reader = new StreamReader(data))
            {

                var xmlResponseDocument = new XmlDocument();

                xmlResponseDocument.LoadXml(reader.ReadToEnd());

                if (!xmlResponseDocument.InnerText.Contains("synRetDownloadXmlNFe"))
                {
                    _returnMessage.Success = false;
                    _returnMessage.ReturnCode = "422";
                    _returnMessage.ReturnMessage = xmlResponseDocument.InnerText;

                }
                else
                {
                    _returnMessage.Success = true;
                    _returnMessage.ReturnCode = "200";
                    _returnMessage.ReturnMessage = xmlResponseDocument
                        .GetElementsByTagName("ns1:synDOCeDownloadXmlReturn")[0].InnerText;
                }

            }
        }

        private void GetResponseStream(WebResponse webResponse)
        {
            using Stream data = webResponse.GetResponseStream();
            GetStreamReader(data);

        }


        private async Task<HttpStandardReturn> HandleResponseMessage()
        {
            _returnMessage = new HttpStandardReturn();

            if (_httpWebRequest is null) return _returnMessage;


            IAsyncResult asyncResult = _httpWebRequest.BeginGetResponse(null, null);



            using (WebResponse webResponse = _httpWebRequest.EndGetResponse(asyncResult))
            {

                GetResponseStream(webResponse);

            }


            return await Task.FromResult(_returnMessage);
        }

        private async Task<HttpStandardReturn> StandardGetRequestStreamAsync(string url, XmlDocument soapEnvelopeXml)
        {
            _logger.LogDebug("Url and message before config {0} {1}",
                soapEnvelopeXml.InnerXml,
                _httpWebRequest.RequestUri);

            HttpStandardReturn HttpStandardReturn;



            if (!string.IsNullOrWhiteSpace(_httpWebRequest?.RequestUri?.AbsoluteUri) &&
                _httpWebRequest.RequestUri.IsAbsoluteUri)
            {
                var urlBase = new Uri(_httpWebRequest.RequestUri.AbsoluteUri, UriKind.Absolute);
                if (Uri.TryCreate(urlBase, url, out Uri result))
                {
                    FullUrl = result;
                }
                else
                {
                    _logger.LogWarning("Url base e relativa informada esta invalido Base: {0} Relativa: {1}",
                        _httpWebRequest?.RequestUri?.AbsoluteUri,
                        url);

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

            _logger.LogDebug("Url and message after config {0}", FullUrl.ToString());



            using (var stream = await _httpWebRequest.GetRequestStreamAsync())
            {
                soapEnvelopeXml.Save(stream);
            }


            HttpStandardReturn = await HandleResponseMessage();


            _logger.LogDebug("HttpStandardReturn return: {0}", HttpStandardReturn.ReturnCode);


            return HttpStandardReturn;
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
        public async Task<HttpStandardReturn> RequestSoap(string urlRoute,
            StandardHttpMethods method,
            XmlDocument messageBody,
            string mediaType = "application/xml")
        {
            var url = $"{urlRoute}{_queryString}";


            _httpWebRequest.MediaType = mediaType;
            _httpWebRequest.Accept = "text/xml";
            _httpWebRequest.Method = method.ToString();
            _httpWebRequest.CustomRequestHeader(_headerStandard);


            return await StandardGetRequestStreamAsync(url, messageBody);

        }


    }
}
