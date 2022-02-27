using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.JsonConverter;
using Nuuvify.CommonPack.Extensions.Notificator;
using Nuuvify.CommonPack.StandardHttpClient.Helpers;
using Nuuvify.CommonPack.StandardHttpClient.Results;


namespace Nuuvify.CommonPack.StandardHttpClient
{

    public abstract class BaseStandardHttpClient
    {
        protected readonly IStandardHttpClient _standardHttpClient;
        protected readonly ITokenService _tokenService;

        /// <summary>
        /// Você pode alterar a configuração que é realizada no construtor
        /// </summary>
        /// <value></value>
        protected JsonSerializerOptions JsonSettings { get; set; }

        protected List<NotificationR> Notifications { get; set; }




        protected BaseStandardHttpClient(IStandardHttpClient standardHttpClient,
            ITokenService tokenService)
        {
            _standardHttpClient = standardHttpClient;

            Notifications = new List<NotificationR>();

            JsonSettings = new JsonSerializerOptions
            {
                Converters =
                {
                    new JsonDateTimeOffsetToInferredTypesConverter(),
                    new JsonDateTimeToInferredTypesConverter(),
                    new JsonObjectToInferredTypesConverter()
                },
                PropertyNameCaseInsensitive = true,
                IgnoreNullValues = true
            };
            _tokenService = tokenService;

        }




        ///<inheritdoc cref="ITokenService.GetTokenAcessor"/>
        public virtual string GetTokenAcessor()
        {
            return _tokenService.GetTokenAcessor();
        }
        ///<inheritdoc cref="ITokenService.GetToken"/>
        public virtual Task<bool> GetToken()
        {
            var tokenValid = _tokenService.GetToken().Result.IsValidToken();
            return Task.FromResult(tokenValid);

        }

        /// <summary>
        /// Essa classe retorna um tipo generico (pode ser List ou Classe) para qualquer chamada Http
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public virtual T ReturnGenericClass<T>(string standardReturn, string api, bool removeStartSpace = false) where T : class
        {
            string message;
            if (!string.IsNullOrWhiteSpace(standardReturn))
            {
                var messageClean = removeStartSpace
                    ? standardReturn.GetReturnMessageWithoutRn()
                    : standardReturn;

                var jsonData = JsonSerializer.Deserialize<T>(messageClean, JsonSettings);
                if (jsonData != null)
                {
                    return jsonData;
                }
                message = $"Não foi possivel deserializar o retorno para a classe {typeof(T)}";
            }
            else
            {
                message = $"Não houve sucesso no retorno da request {standardReturn}";
            }

            Notifications.Add(new NotificationR(property: typeof(T).Name,
                message: $"{message}-Correlation: {_standardHttpClient.CorrelationId}",
                aggregatorId: api,
                type: "application",
                originNotification: null));

            return (T)Convert.ChangeType(null, typeof(T));
        }

        public virtual T ReturnClass<T>(HttpStandardReturn standardReturn, string api) where T : class
        {
            if (standardReturn.Success)
            {
                var messageClean = standardReturn.GetReturnMessageWithoutRn();

                if (!string.IsNullOrWhiteSpace(messageClean))
                {
                    return DeserealizeObject<T>(messageClean);
                }
                else if (standardReturn.ReturnCode.Equals(HttpStatusCode.NoContent.GetHashCode().ToString()))
                {
                    return (T)Convert.ChangeType(null, typeof(T));
                }
                Notifications.Add(new NotificationR(property: "ReturnClass<T>",
                    message: $"Não foi possivel deserializar o retorno para a classe {typeof(T).Name}-Correlation: {_standardHttpClient.CorrelationId}. ReturCode: {standardReturn?.ReturnCode}",
                    aggregatorId: api,
                    type: "application",
                    originNotification: null));

                Notifications.Add(new NotificationR(property: "ReturnClass<T>",
                    message: messageClean,
                    aggregatorId: api,
                    type: "origin-message",
                    originNotification: null));


                return (T)Convert.ChangeType(null, typeof(T));
            }
            else if (standardReturn?.ReturnCode != "422")
            {

                Notifications.Add(new NotificationR(property: typeof(T).Name,
                    message: $"Não houve sucesso no retorno da request para a classe {nameof(HttpStandardReturn)}-Correlation: {_standardHttpClient.CorrelationId}",
                    aggregatorId: api,
                    type: "application",
                    originNotification: null));

                Notifications.Add(new NotificationR(property: typeof(T).Name,
                        message: standardReturn?.ReturnMessage,
                        aggregatorId: api,
                        type: "origin-message",
                        originNotification: null));
            }


            ReturnNotificationApi(standardReturn, api);
            return (T)Convert.ChangeType(null, typeof(T));
        }

        public virtual IList<T> ReturnList<T>(HttpStandardReturn standardReturn, string api) where T : class
        {

            if (standardReturn.Success)
            {
                var messageClean = standardReturn.GetReturnMessageWithoutRn();

                if (!string.IsNullOrWhiteSpace(messageClean))
                {
                    return DeserealizeList<T>(messageClean);
                }
                else if (standardReturn.ReturnCode.Equals(HttpStatusCode.NoContent.GetHashCode().ToString()))
                {
                    return new List<T>();
                }
                Notifications.Add(new NotificationR(property: "ReturnList<>",
                    message: $"Não foi possivel deserializar o retorno para a classe {typeof(T).Name}-Correlation: {_standardHttpClient.CorrelationId}. ReturCode: {standardReturn?.ReturnCode}",
                    aggregatorId: api,
                    type: "application",
                    originNotification: null));

                Notifications.Add(new NotificationR(property: "ReturnList<>",
                    message: messageClean,
                    aggregatorId: api,
                    type: "origin-message",
                    originNotification: null));

                return new List<T>();
            }
            else if (standardReturn?.ReturnCode != "422")
            {
                Notifications.Add(new NotificationR(property: typeof(T).Name,
                    message: $"Não houve sucesso no retorno da request para a classe {nameof(HttpStandardReturn)}-Correlation: {_standardHttpClient.CorrelationId}",
                    aggregatorId: api,
                    type: "application",
                    originNotification: null));

                Notifications.Add(new NotificationR(property: "ReturnList<>",
                    message: standardReturn?.ReturnMessage,
                    aggregatorId: api,
                    type: "origin-message",
                    originNotification: null));
            }

            ReturnNotificationApi(standardReturn, api);
            return new List<T>();
        }

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
                    IgnoreNullValues = true

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

        public bool IsValid()
        {
            return Notifications.Count == 0;
        }

    }
}
