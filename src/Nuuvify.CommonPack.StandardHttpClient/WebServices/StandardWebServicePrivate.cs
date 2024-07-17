using System.Xml;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.StandardHttpClient.Results;

namespace Nuuvify.CommonPack.StandardHttpClient.WebServices
{

    public partial class StandardWebService
    {



        private HttpStandardXmlReturn GetStreamReader(Stream data)
        {

            _returnMessage = new HttpStandardXmlReturn();
            var xmlResponseDocument = new XmlDocument();


            using (var reader = new StreamReader(data))
            {

                try
                {
                    xmlResponseDocument.LoadXml(reader.ReadToEnd());

                    _returnMessage.Success = true;
                    _returnMessage.ReturnCode = "200";
                    _returnMessage.ReturnMessage = xmlResponseDocument;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao tentar converter o xml para XmlDocument, veja o log da aplicação.");
                    _returnMessage.Success = false;
                    _returnMessage.ReturnCode = "422";
                    _returnMessage.ReturnMessage = null;
                }

            }

            return _returnMessage;

        }


        private async Task<HttpStandardXmlReturn> StandardGetRequestStreamAsync(
            string url,
            XmlDocument soapEnvelopeXml)
        {
            _logger.LogDebug("Url and message before config {InnerXml} {AbsoluteUri}",
                soapEnvelopeXml.InnerXml,
                _httpClient?.BaseAddress?.AbsoluteUri);

            HttpStandardXmlReturn httpStandardReturn;


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
                    _logger.LogWarning("Url base e relativa informada esta invalido Base: {AbsoluteUri} Relativa: {url}",
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
                    _logger.LogWarning("Url informada é invalida {url}", url);
                    return null;
                }
            }

            _logger.LogDebug("Url and message after config {FullUrl}", FullUrl.ToString());



            using (var stream = await _httpClient.GetStreamAsync(FullUrl))
            {
                soapEnvelopeXml.Save(stream);
                httpStandardReturn = GetStreamReader(stream);
            }


            _logger.LogDebug("HttpStandardReturn return: {ReturnCode}", httpStandardReturn.ReturnCode);


            return httpStandardReturn;

        }




    }
}
