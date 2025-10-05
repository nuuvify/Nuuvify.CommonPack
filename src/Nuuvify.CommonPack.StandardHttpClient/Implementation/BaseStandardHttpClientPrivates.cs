using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nuuvify.CommonPack.Extensions.Notificator;
using Nuuvify.CommonPack.StandardHttpClient.Helpers;
using Nuuvify.CommonPack.StandardHttpClient.Results;

namespace Nuuvify.CommonPack.StandardHttpClient;

public abstract partial class BaseStandardHttpClient
{

    private IList<T> DeserealizeList<T>(string message, int jsonDataDepth = 0) where T : class
    {
        try
        {
            if (jsonDataDepth > 0)
            {
                // Navigate through nested "Data" properties based on depth
                var jsonData = NavigateToDataProperty(message, jsonDataDepth);
                if (!string.IsNullOrEmpty(jsonData))
                {
                    var nestedList = JsonSerializer.Deserialize<List<T>>(jsonData, JsonSettings);
                    if (nestedList != null)
                    {
                        return nestedList;
                    }
                }
            }

            // Fallback: Try to deserialize as direct list
            var directList = JsonSerializer.Deserialize<List<T>>(message, JsonSettings);
            if (directList != null)
            {
                return directList;
            }
        }
        catch (JsonException ex)
        {
            // Log the exception if needed
            AddNotification(new NotificationR(MethodBase.GetCurrentMethod().Name, $"Failed to deserialize JSON message: {message} jsonDataDepth: {jsonDataDepth} Exception: {ex.Message}"));
        }

        return new List<T>();
    }


    private T DeserealizeObject<T>(string message, int jsonDataDepth = 0) where T : class
    {
        try
        {
            if (jsonDataDepth > 0)
            {
                // Navigate through nested "Data" properties based on depth
                var nestedJsonData = NavigateToDataProperty(message, jsonDataDepth);
                if (!string.IsNullOrEmpty(nestedJsonData))
                {
                    var nestedObject = JsonSerializer.Deserialize<T>(nestedJsonData, JsonSettings);
                    if (nestedObject != null)
                    {
                        return nestedObject;
                    }
                }
            }

            // Fallback: Try to deserialize using the existing DeserializeObjectSuccess approach
            var jsonData = JsonSerializer.Deserialize<DeserializeObjectSuccess<T>>(message, JsonSettings);
            if (!(jsonData is null) && jsonData.Data != null)
            {
                var returnData = jsonData.Data;
                return returnData;
            }
        }
        catch (JsonException ex)
        {
            // Log the exception if needed
            AddNotification(new NotificationR(MethodBase.GetCurrentMethod().Name, $"Failed to deserialize JSON message: {message} jsonDataDepth: {jsonDataDepth} Exception: {ex.Message}"));
        }

        return (T)Convert.ChangeType(null, typeof(T), CultureInfo.InvariantCulture);
    }
    private string NavigateToDataProperty(string jsonMessage, int depth)
    {
        try
        {
            using var document = JsonDocument.Parse(jsonMessage);
            var currentElement = document.RootElement;

            for (int i = 0; i < depth + 1; i++)
            {
                // Check if current element is an object before trying to get properties
                if (currentElement.ValueKind != JsonValueKind.Object)
                {
                    return null; // Cannot navigate deeper, not an object
                }

                if (currentElement.TryGetProperty("Data", out var dataProperty) ||
                    currentElement.TryGetProperty("data", out dataProperty))
                {
                    currentElement = dataProperty;
                }
                else
                {
                    return null; // Property not found
                }
            }

            // Convert JsonElement to string before the document is disposed
            return currentElement.GetRawText();
        }
        catch (JsonException ex)
        {
            AddNotification(new NotificationR(MethodBase.GetCurrentMethod().Name, $"Failed to navigate JSON structure at depth {depth} Exception: {ex.Message}"));
            return null;
        }
    }

    private void ReturnNotificationApi(HttpStandardReturn standardReturn, string api)
    {
        try
        {
            var codigoRetorno = ParseReturnCode(standardReturn.ReturnCode);
            var propertyNotification = $"Codigo Retorno: {codigoRetorno}";

            ProcessStatusCodeNotification(standardReturn, api, codigoRetorno, propertyNotification);
        }
        catch (JsonException ex)
        {
            AddApplicationErrorNotification(ex, standardReturn, api);
        }
        catch (ArgumentNullException ex)
        {
            AddApplicationErrorNotification(ex, standardReturn, api);
        }
        catch (InvalidOperationException ex)
        {
            AddApplicationErrorNotification(ex, standardReturn, api);
        }
    }

    private static int ParseReturnCode(string returnCode)
    {
        _ = int.TryParse(returnCode, out int codigoRetorno);
        return codigoRetorno;
    }

    private void ProcessStatusCodeNotification(HttpStandardReturn standardReturn, string api, int codigoRetorno, string propertyNotification)
    {
        if (IsKnownHttpStatusCode(codigoRetorno))
        {
            AddKnownStatusCodeNotification(standardReturn, api, codigoRetorno, propertyNotification);
        }
        else
        {
            ProcessUnknownStatusCode(standardReturn, api, propertyNotification);
        }
    }

    private static bool IsKnownHttpStatusCode(int codigoRetorno)
    {
        return codigoRetorno.Equals(HttpStatusCode.ExpectationFailed.GetHashCode()) ||
               codigoRetorno.Equals(HttpStatusCode.Unauthorized.GetHashCode()) ||
               codigoRetorno.Equals(HttpStatusCode.Forbidden.GetHashCode()) ||
               codigoRetorno.Equals(HttpStatusCode.InternalServerError.GetHashCode()) ||
               codigoRetorno.Equals(HttpStatusCode.ServiceUnavailable.GetHashCode()) ||
               codigoRetorno.Equals(HttpStatusCode.NotFound.GetHashCode());
    }

    private void AddKnownStatusCodeNotification(HttpStandardReturn standardReturn, string api, int codigoRetorno, string propertyNotification)
    {
        var returnMessage = standardReturn?.ReturnMessage;
        var correlationId = _standardHttpClient.CorrelationId;

        switch (codigoRetorno)
        {
            case var code when code.Equals(HttpStatusCode.ExpectationFailed.GetHashCode()):
                ProcessExpectationFailedError(standardReturn, api, propertyNotification);
                break;
            case var code when code.Equals(HttpStatusCode.Unauthorized.GetHashCode()):
                AddOriginNotification(propertyNotification, $"Token expirado/invalido ou credenciais incorretas. Correlation: {correlationId} {returnMessage}", api);
                break;
            case var code when code.Equals(HttpStatusCode.Forbidden.GetHashCode()):
                AddOriginNotification(propertyNotification, $"Acesso negado. Correlation: {correlationId} {returnMessage}", api);
                break;
            case var code when code.Equals(HttpStatusCode.InternalServerError.GetHashCode()):
                AddOriginNotification(propertyNotification, $"Houve uma falha no serviço. Correlation: {correlationId} {returnMessage}", api);
                break;
            case var code when code.Equals(HttpStatusCode.ServiceUnavailable.GetHashCode()):
                AddOriginNotification(propertyNotification, $"O serviço esta temporariamente indisponivel. Correlation: {correlationId} {returnMessage}", api);
                break;
            case var code when code.Equals(HttpStatusCode.NotFound.GetHashCode()):
                AddOriginNotification(propertyNotification, $"Endpoint não existe. Correlation: {correlationId} {returnMessage}", api);
                break;
        }
    }

    private void ProcessExpectationFailedError(HttpStandardReturn standardReturn, string api, string propertyNotification)
    {
        var commandResultErro = JsonSerializer.Deserialize<ReturnStandardErrorsModelState>(standardReturn.ReturnMessage, JsonSettings);
        foreach (var item in commandResultErro.Errors)
        {
            var message = $"{item.ErrorHost}{item.ErrorPath} Correlation: {_standardHttpClient.CorrelationId} Error Message: {item.ErrorMessage}";
            AddOriginNotification(propertyNotification, message, api);
        }
    }

    private void ProcessUnknownStatusCode(HttpStandardReturn standardReturn, string api, string propertyNotification)
    {
        if (IsHtmlErrorResponse(standardReturn.ReturnMessage))
        {
            ProcessHtmlErrorResponse(standardReturn, api, propertyNotification);
        }
        else
        {
            ProcessJsonErrorResponse(standardReturn, api, propertyNotification);
        }
    }

    private static bool IsHtmlErrorResponse(string returnMessage)
    {
        return returnMessage.Contains("!DOCTYPE html PUBLIC");
    }

    private void ProcessHtmlErrorResponse(HttpStandardReturn standardReturn, string api, string propertyNotification)
    {
        ProcessGenericErrorResponse(standardReturn, api, propertyNotification, "Houve um erro na chamada desse endpoint");
    }

    private void ProcessJsonErrorResponse(HttpStandardReturn standardReturn, string api, string propertyNotification)
    {
        var jsonSettingsNotifications = CreateNotificationJsonSettings();
        var commandResultErro = JsonSerializer.Deserialize<ReturnStandardErrors>(standardReturn.ReturnMessage, jsonSettingsNotifications);

        if (commandResultErro is null)
        {
            ProcessNullErrorResponse(standardReturn, api, propertyNotification);
        }
        else
        {
            ProcessValidErrorResponse(commandResultErro, standardReturn, api);
        }
    }

    private static JsonSerializerOptions CreateNotificationJsonSettings()
    {
        return new JsonSerializerOptions
        {
            Converters = { new NotificationRConverter() },
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    private void ProcessNullErrorResponse(HttpStandardReturn standardReturn, string api, string propertyNotification)
    {
        ProcessGenericErrorResponse(standardReturn, api, propertyNotification, "Falha ao deserializar resposta de erro do endpoint");
    }

    private void ProcessGenericErrorResponse(HttpStandardReturn standardReturn, string api, string propertyNotification, string errorDescription)
    {
        var message = $"{errorDescription}. Correlation: {_standardHttpClient.CorrelationId} ReturnMessage: {standardReturn.ReturnMessage}";
        AddOriginNotification(propertyNotification, message, api);
    }

    private void ProcessValidErrorResponse(ReturnStandardErrors commandResultErro, HttpStandardReturn standardReturn, string api)
    {
        foreach (var item in commandResultErro.Errors)
        {
            var aggregatorId = $"ReturnCode: {standardReturn.ReturnCode}, {api}";
            AddOriginNotification(item.Property, item.Message, aggregatorId);
        }
    }

    private void AddOriginNotification(string property, string message, string aggregatorId)
    {
        AddNotification(new NotificationR(
            property: property,
            message: message,
            aggregatorId: aggregatorId,
            type: "origin",
            originNotification: null));
    }

    private void AddApplicationErrorNotification(Exception ex, HttpStandardReturn standardReturn, string api)
    {
        var message = $"Ocorreu o erro: {ex.Message} ao deserializar o retorno: {standardReturn.ReturnMessage}. Correlation: {_standardHttpClient.CorrelationId} - Talvez possa ser resolvido com parametros da propriedade JsonSettings ou com uma classe para deserialização mais adequada";

        AddNotification(new NotificationR(
            property: ex.Source,
            message: message,
            aggregatorId: $"{api}",
            type: "application",
            originNotification: null));
    }

}

