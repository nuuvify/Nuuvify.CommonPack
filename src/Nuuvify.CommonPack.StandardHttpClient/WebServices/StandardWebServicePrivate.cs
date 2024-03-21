using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.StandardHttpClient.Results;

namespace Nuuvify.CommonPack.StandardHttpClient.WebServices
{

    public partial class StandardWebService
    {

        /// <summary>
        /// Exemplo: synRetDownloadXmlNFe
        /// </summary>
        /// <value>synRetDownloadXmlNFe</value>
        private string XmlResponseDocumentContains { get; set; }

        /// <summary>
        /// Exemplo: ns1:synDOCeDownloadXmlReturn
        /// </summary>
        /// <value>ns1:synDOCeDownloadXmlReturn</value>
        private string XmlGetElementsByTagName { get; set; }



        private HttpStandardReturn GetStreamReader(Stream data)
        {

            _returnMessage = new HttpStandardReturn();


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

            return _returnMessage;

        }

        // private void GetResponseStream(WebResponse webResponse)
        // {
        //     using Stream data = webResponse.GetResponseStream();
        //     GetStreamReader(data);

        // }


        // private async Task<HttpStandardReturn> HandleResponseMessage()
        // {
        //     _returnMessage = new HttpStandardReturn();

        //     if (_httpWebRequest is null) return _returnMessage;


        //     IAsyncResult asyncResult = _httpWebRequest.BeginGetResponse(null, null);



        //     using (WebResponse webResponse = _httpWebRequest.EndGetResponse(asyncResult))
        //     {

        //         GetResponseStream(webResponse);

        //     }


        //     return await Task.FromResult(_returnMessage);
        // }

        private async Task<HttpStandardReturn> StandardGetRequestStreamAsync(
            string url,
            XmlDocument soapEnvelopeXml)
        {
            _logger.LogDebug("Url and message before config {0} {1}",
                soapEnvelopeXml.InnerXml,
                _httpClient?.BaseAddress?.AbsoluteUri);

            HttpStandardReturn httpStandardReturn;


            if (!string.IsNullOrWhiteSpace(_httpClient?.BaseAddress?.AbsoluteUri) &&
                _httpClient.BaseAddress.IsAbsoluteUri)
            {
                var urlBase = new Uri(_httpClient?.BaseAddress?.AbsoluteUri, UriKind.Absolute);
                if (Uri.TryCreate(urlBase, url, out Uri result))
                {
                    FullUrl = result;
                }
                else
                {
                    _logger.LogWarning("Url base e relativa informada esta invalido Base: {0} Relativa: {1}",
                        _httpClient?.BaseAddress?.AbsoluteUri,
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



            using (var stream = await _httpClient.GetStreamAsync(FullUrl))
            {
                soapEnvelopeXml.Save(stream);
                httpStandardReturn = GetStreamReader(stream);
            }


            _logger.LogDebug("HttpStandardReturn return: {0}", httpStandardReturn.ReturnCode);


            return httpStandardReturn;

        }




    }
}
