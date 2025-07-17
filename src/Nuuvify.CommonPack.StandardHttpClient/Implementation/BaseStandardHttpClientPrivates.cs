using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nuuvify.CommonPack.Extensions.Notificator;
using Nuuvify.CommonPack.StandardHttpClient.Helpers;
using Nuuvify.CommonPack.StandardHttpClient.Results;


namespace Nuuvify.CommonPack.StandardHttpClient;


public abstract partial class BaseStandardHttpClient
{

    private IList<T> DeserealizeList<T>(string message) where T : class
    {

        var jsonData = JsonSerializer.Deserialize<DeserializeListSuccess<T>>(message, JsonSettings);
        if (!(jsonData is null) && jsonData.Data != null)
        {
            var returnData = jsonData.Data;
            var returnList = returnData?.ToList<T>();
            return returnList;
        }


        return new List<T>();
    }

    private T DeserealizeObject<T>(string message) where T : class
    {

        var jsonData = JsonSerializer.Deserialize<DeserializeObjectSuccess<T>>(message, JsonSettings);
        if (!(jsonData is null) && jsonData.Data != null)
        {
            var returnData = jsonData.Data;
            return returnData;
        }


        return (T)Convert.ChangeType(null, typeof(T));
    }

    private void ReturnNotificationApi(HttpStandardReturn standardReturn, string api)
    {
        try
        {

            var jsonSettingsNotifications = new JsonSerializerOptions
            {
                Converters =
                    {
                        new NotificationRConverter()
                    },
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull

            };


            int.TryParse(standardReturn.ReturnCode, out int codigoRetorno);
            var returnMessage = standardReturn?.ReturnMessage;

            var propertyNotification = $"Codigo Retorno: {codigoRetorno}";




            if (codigoRetorno.Equals(HttpStatusCode.ExpectationFailed.GetHashCode()))
            {
                var commandResultErro = JsonSerializer.Deserialize<ReturnStandardErrorsModelState>(standardReturn.ReturnMessage, JsonSettings);
                foreach (var item in commandResultErro.Errors)
                {
                    Notifications.Add(new NotificationR(property: propertyNotification, message: $"{item.ErrorHost}{item.ErrorPath} Correlation: {_standardHttpClient.CorrelationId} Error Message: {item.ErrorMessage}", aggregatorId: $"{api}", type: "origin", originNotification: null));
                }
            }
            else if (codigoRetorno.Equals(HttpStatusCode.Unauthorized.GetHashCode()))
            {
                Notifications.Add(new NotificationR(property: propertyNotification, message: $"Token expirado/invalido ou credenciais incorretas. Correlation: {_standardHttpClient.CorrelationId} {returnMessage}", aggregatorId: $"{api}", type: "origin", originNotification: null));
            }
            else if (codigoRetorno.Equals(HttpStatusCode.Forbidden.GetHashCode()))
            {
                Notifications.Add(new NotificationR(property: propertyNotification, message: $"Acesso negado. Correlation: {_standardHttpClient.CorrelationId} {returnMessage}", aggregatorId: $"{api}", type: "origin", originNotification: null));
            }
            else if (codigoRetorno.Equals(HttpStatusCode.InternalServerError.GetHashCode()))
            {
                Notifications.Add(new NotificationR(property: propertyNotification, message: $"Houve uma falha no serviço. Correlation: {_standardHttpClient.CorrelationId} {returnMessage}", aggregatorId: $"{api}", type: "origin", originNotification: null));
            }
            else if (codigoRetorno.Equals(HttpStatusCode.ServiceUnavailable.GetHashCode()))
            {
                Notifications.Add(new NotificationR(property: propertyNotification, message: $"O serviço esta temporariamente indisponivel. Correlation: {_standardHttpClient.CorrelationId} {returnMessage}", aggregatorId: $"{api}", type: "origin", originNotification: null));
            }
            else if (codigoRetorno.Equals(HttpStatusCode.NotFound.GetHashCode()))
            {
                Notifications.Add(new NotificationR(property: propertyNotification, $"Endpoint não existe. Correlation: {_standardHttpClient.CorrelationId} {returnMessage}", aggregatorId: $"{api}", type: "origin", originNotification: null));
            }
            else
            {

                if (standardReturn.ReturnMessage.Contains("!DOCTYPE html PUBLIC"))
                {
                    Notifications.Add(
                        new NotificationR(
                            property: propertyNotification, $"Houve um erro na chamada desse endpoint. Correlation: {_standardHttpClient.CorrelationId} ReturnMessage: {standardReturn.ReturnMessage}",
                            aggregatorId: $"{api}",
                            type: "origin",
                            originNotification: null));
                }
                else
                {
                    var commandResultErro = JsonSerializer.Deserialize<ReturnStandardErrors>(standardReturn.ReturnMessage, jsonSettingsNotifications);
                    if (commandResultErro is null)
                    {
                        Notifications.Add(
                            new NotificationR(
                                property: propertyNotification,
                                message: $"Houve um erro na chamada desse endpoint. Correlation: {_standardHttpClient.CorrelationId} ReturnMessage: {standardReturn.ReturnMessage}",
                                aggregatorId: $"{api}",
                                type: "origin",
                                originNotification: null));
                    }
                    else
                    {
                        foreach (var item in commandResultErro.Errors)
                        {
                            Notifications.Add(
                                new NotificationR(
                                    property: item.Property,
                                    message: item.Message,
                                    aggregatorId: $"ReturnCode: {standardReturn.ReturnCode}, {api}",
                                    type: "origin",
                                    originNotification: null));
                        }
                    }

                }
            }

        }
        catch (Exception ex)
        {

            Notifications.Add(new NotificationR(property: ex.Source,
                 message: $"Ocorreu o erro: {ex.Message} ao deserializar o retorno: {standardReturn.ReturnMessage}. Correlation: {_standardHttpClient.CorrelationId} - Talvez possa ser resolvido com parametros da propriedade JsonSettings ou com uma classe para deserialização mais adequada",
                 aggregatorId: $"{api}",
                 type: "application",
                 originNotification: null));

        }
    }



}

