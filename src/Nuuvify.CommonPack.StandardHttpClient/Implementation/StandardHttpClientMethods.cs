using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.StandardHttpClient.Results;

namespace Nuuvify.CommonPack.StandardHttpClient
{


    public partial class StandardHttpClient
    {


        private void LogRequestMessage(HttpRequestMessage message)
        {

            var logString = new StringBuilder("==== REQUEST ==========================================================")
                .AppendLine()
                .AppendLine($"Verb: {message.Method.Method}")
                .AppendLine($"Uri:  {message.RequestUri.AbsoluteUri}");

            string recurseValue;
            foreach (var item in message.Headers)
            {
                recurseValue = "";
                foreach (var itemValue in item.Value)
                {
                    recurseValue += $",{itemValue}";
                }
                recurseValue = recurseValue.SubstringNotNull(1, recurseValue.Length - 1);

                logString.AppendLine($"{item.Key} : {recurseValue}");

                if (item.Key.StartsWith("authorization", StringComparison.InvariantCultureIgnoreCase))
                    AuthorizationLog = recurseValue;

            }

            logString.AppendLine($"Content: {message.Content}")
                .AppendLine($"=======================================================================");


            _logger.LogInformation(logString.ToString());

        }


        private async Task<HttpStandardReturn> StandardSendAsync(string url, HttpRequestMessage message, CancellationToken cancellationToken = default)
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

                response = await _httpClient.SendAsync(message, HttpOption, cancellationToken);

            }
            else
            {
                response = await _httpClient.SendAsync(message, cancellationToken);
            }


            LogRequestMessage(message);


            HttpStandardReturn httpStandardReturn = await HandleResponseMessage(response);


            _logger.LogDebug("HttpStandardReturn return: {0}", httpStandardReturn.ReturnCode);


            return httpStandardReturn;
        }

        private async Task<HttpStandardStreamReturn> StandardStreamSendAsync(string url, HttpRequestMessage message, CancellationToken cancellationToken = default)
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

                response = await _httpClient.SendAsync(message, HttpOption, cancellationToken);

            }
            else
            {
                response = await _httpClient.SendAsync(message, cancellationToken);
            }


            LogRequestMessage(message);


            HttpStandardStreamReturn httpStandardStreamReturn = await HandleResponseMessageStream(response);


            _logger.LogDebug("HttpStandardReturn return: {0}", httpStandardStreamReturn.ReturnCode);


            return httpStandardStreamReturn;

        }

        public async Task<HttpStandardReturn> HandleResponseMessage(HttpResponseMessage response)
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

        public async Task<HttpStandardStreamReturn> HandleResponseMessageStream(HttpResponseMessage response)
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

    }
}
