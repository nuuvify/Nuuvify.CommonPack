using System.Text;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.StandardHttpClient.Results;

namespace Nuuvify.CommonPack.StandardHttpClient;

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

            _ = logString.AppendLine($"{item.Key} : {recurseValue}");

            if (item.Key.StartsWith("authorization", StringComparison.InvariantCultureIgnoreCase))
                AuthorizationLog = recurseValue;

        }

        _ = logString.AppendLine($"Content: {message.Content}")
            .AppendLine($"=======================================================================");

        _logger.LogInformation(logString.ToString());

    }

    private async Task<HttpStandardReturn> StandardSendAsync(
        string url,
        HttpRequestMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Url and message before config: {message} url: {url}", message, url);

        if (url.EndsWith("&") || url.EndsWith("?"))
            url = url[..^1];

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
                _logger.LogWarning("Url base e relativa informada esta invalido Base: {AbsoluteUri} Relativa: {Url}", _httpClient?.BaseAddress?.AbsoluteUri, url);
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
                _logger.LogWarning("Url informada é invalida: {Url}", url);
                return null;
            }
        }

        _logger.LogDebug("Url and message after config: {Message} client url: {RequestUri}", message, message?.RequestUri);

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

        if (LogRequest)
            LogRequestMessage(message);

        HttpStandardReturn httpStandardReturn = await HandleResponseMessage(response);

        _logger.LogDebug("HttpStandardReturn return: {ReturnCode}", httpStandardReturn.ReturnCode);

        return httpStandardReturn;
    }

    private async Task<HttpStandardStreamReturn> StandardStreamSendAsync(
        string url,
        HttpRequestMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Url and message before config: {Message} url: {Url}", message, url);

        if (url.EndsWith("&") || url.EndsWith("?"))
            url = url[..^1];

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
                _logger.LogWarning("Url base e relativa informada esta invalido Base: {AbsoluteUri} Relativa: {Url}", _httpClient?.BaseAddress?.AbsoluteUri, url);
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
                _logger.LogWarning("Url informada é invalida: {Url}", url);
                return null;
            }
        }

        _logger.LogDebug("Url and message after config: {Message} client url: {RequestUri}", message, message?.RequestUri);

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

        if (LogRequest)
            LogRequestMessage(message);

        HttpStandardStreamReturn httpStandardStreamReturn = await HandleResponseMessageStream(response);

        _logger.LogDebug("HttpStandardReturn return: {ReturnCode}", httpStandardStreamReturn?.ReturnCode);

        return httpStandardStreamReturn;

    }

    public async Task<HttpStandardReturn> HandleResponseMessage(HttpResponseMessage response)
    {
        var returnMessage = new HttpStandardReturn();

        if (response is null) return returnMessage;

        CustomHttpResponseMessage = response;

        var resultNumber = (int)response.StatusCode;
        returnMessage.ReturnCode = resultNumber.ToString(CultureInfo.InvariantCulture);
        returnMessage.Success = response.IsSuccessStatusCode;

        var content = await response.Content.ReadAsStringAsync();

        returnMessage.ReturnMessage = content;

        return returnMessage;
    }

    public async Task<HttpStandardStreamReturn> HandleResponseMessageStream(HttpResponseMessage response)
    {
        var returnMessage = new HttpStandardStreamReturn();

        if (response is null) return returnMessage;

        CustomHttpResponseMessage = response;

        var resultNumber = (int)response.StatusCode;

        returnMessage.ReturnCode = resultNumber.ToString(CultureInfo.InvariantCulture);
        returnMessage.Success = response.IsSuccessStatusCode;

        var content = await response.Content.ReadAsStreamAsync();

        returnMessage.ReturnMessage = content;

        return returnMessage;
    }

}

