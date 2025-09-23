using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.JsonConverter;
using Nuuvify.CommonPack.Extensions.Notificator;
using Nuuvify.CommonPack.StandardHttpClient.Results;

namespace Nuuvify.CommonPack.StandardHttpClient;

public abstract partial class BaseStandardHttpClient
{
    private const string ApplicationErrorType = "application";
    private const string OriginMessageType = "origin-message";

    private readonly IStandardHttpClient _standardHttpClient;
    private readonly ITokenService _tokenService;

    /// <summary>
    /// Você pode alterar a configuração que é realizada no construtor
    /// </summary>
    /// <value></value>
    protected JsonSerializerOptions JsonSettings { get; set; }

    protected Collection<NotificationR> Notifications { get; set; }

    protected BaseStandardHttpClient(
        IStandardHttpClient standardHttpClient,
        ITokenService tokenService)
    {
        _standardHttpClient = standardHttpClient;

        Notifications = new Collection<NotificationR>();

        JsonSettings = new JsonSerializerOptions
        {
            Converters =
                {
                    new JsonDateTimeOffsetToInferredTypesConverter(),
                    new JsonDateTimeToInferredTypesConverter(),
                    new JsonObjectToInferredTypesConverter()
                },
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        _tokenService = tokenService;

    }

    ///<inheritdoc cref="ITokenService.GetTokenAcessor"/>
    public virtual string GetTokenAcessor()
    {
        return _tokenService.GetTokenAcessor();
    }
    ///<inheritdoc cref="ITokenService.GetToken"/>
    public virtual async Task<bool> GetToken(
        string login = null,
        string password = null,
        string userClaim = null,
        CancellationToken cancellationToken = default)
    {
        var token = await _tokenService.GetToken(
            login: login,
            password: password,
            userClaim: userClaim,
            cancellationToken: cancellationToken
        );
        return token.IsValidToken();
    }

    /// <summary>
    /// Essa classe retorna um tipo generico (pode ser List ou Classe) para qualquer chamada Http <br/>
    /// Para deserializar XML, use a classe do dotnet conforme documentação aqui: <br/>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/standard/serialization/examples-of-xml-serialization"></seealso>
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

        Notifications.Add(new NotificationR(
            property: typeof(T).Name,
            message: $"{message}-Correlation: {_standardHttpClient.CorrelationId}",
            aggregatorId: api,
            type: ApplicationErrorType,
            originNotification: null));

        return (T)Convert.ChangeType(null, typeof(T), CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Deserializa um retorno JSON para uma classe C# <br/>
    /// Para deserializar XML, use sua propria classe conforme essa documentação: <seealso cref="ReturnGenericClass"/>
    /// </summary>
    public virtual T ReturnClass<T>(
        HttpStandardReturn standardReturn,
        string api,
        int jsonDataDepth = 0) where T : class
    {
        if (standardReturn.Success)
        {
            var messageClean = standardReturn.GetReturnMessageWithoutRn();

            if (!string.IsNullOrWhiteSpace(messageClean))
            {
                return DeserealizeObject<T>(messageClean, jsonDataDepth);
            }
            else if (standardReturn.ReturnCode.Equals(
                HttpStatusCode.NoContent.GetHashCode().ToString(CultureInfo.InvariantCulture),
                StringComparison.Ordinal))
            {
                return (T)Convert.ChangeType(null, typeof(T), CultureInfo.InvariantCulture);
            }
            Notifications.Add(new NotificationR(
                property: "ReturnClass<T>",
                message: $"Não foi possivel deserializar o retorno para a classe {typeof(T).Name}-Correlation: {_standardHttpClient.CorrelationId}. ReturCode: {standardReturn?.ReturnCode}",
                aggregatorId: api,
                type: ApplicationErrorType,
                originNotification: null));

            Notifications.Add(new NotificationR(
                property: "ReturnClass<T>",
                message: messageClean,
                aggregatorId: api,
                type: OriginMessageType,
                originNotification: null));

            return (T)Convert.ChangeType(null, typeof(T), CultureInfo.InvariantCulture);
        }
        else if (standardReturn?.ReturnCode != "422")
        {

            Notifications.Add(new NotificationR(property: typeof(T).Name,
                message: $"Não houve sucesso no retorno da request para a classe {nameof(HttpStandardReturn)}-Correlation: {_standardHttpClient.CorrelationId}",
                aggregatorId: api,
                type: ApplicationErrorType,
                originNotification: null));

            Notifications.Add(new NotificationR(property: typeof(T).Name,
                    message: standardReturn?.ReturnMessage,
                    aggregatorId: api,
                    type: OriginMessageType,
                    originNotification: null));
        }

        ReturnNotificationApi(standardReturn, api);
        return (T)Convert.ChangeType(null, typeof(T), CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Deserializa uma lista JSON para uma classe C# com navegação por profundidade de propriedades Data <br/>
    /// Para deserializar XML, use sua propria classe conforme essa documentação: <seealso cref="ReturnGenericClass"/>
    /// </summary>
    public virtual IList<T> ReturnList<T>(
        HttpStandardReturn standardReturn,
        string api,
        int jsonDataDepth = 0) where T : class
    {

        if (standardReturn.Success)
        {
            var messageClean = standardReturn.GetReturnMessageWithoutRn();

            if (!string.IsNullOrWhiteSpace(messageClean))
            {
                return DeserealizeList<T>(messageClean, jsonDataDepth);
            }
            else if (standardReturn.ReturnCode.Equals(
                HttpStatusCode.NoContent.GetHashCode().ToString(CultureInfo.InvariantCulture),
                StringComparison.Ordinal))
            {
                return new List<T>();
            }
            Notifications.Add(new NotificationR(property: "ReturnList<>",
                message: $"Não foi possivel deserializar o retorno para a classe {typeof(T).Name}-Correlation: {_standardHttpClient.CorrelationId}. ReturCode: {standardReturn?.ReturnCode}",
                aggregatorId: api,
                type: ApplicationErrorType,
                originNotification: null));

            Notifications.Add(new NotificationR(property: "ReturnList<>",
                message: messageClean,
                aggregatorId: api,
                type: OriginMessageType,
                originNotification: null));

            return new List<T>();
        }
        else if (standardReturn?.ReturnCode != "422")
        {
            Notifications.Add(new NotificationR(property: typeof(T).Name,
                message: $"Não houve sucesso no retorno da request para a classe {nameof(HttpStandardReturn)}-Correlation: {_standardHttpClient.CorrelationId}",
                aggregatorId: api,
                type: ApplicationErrorType,
                originNotification: null));

            Notifications.Add(new NotificationR(property: "ReturnList<>",
                message: standardReturn?.ReturnMessage,
                aggregatorId: api,
                type: OriginMessageType,
                originNotification: null));
        }

        ReturnNotificationApi(standardReturn, api);
        return new List<T>();
    }

    public bool IsValid()
    {
        return Notifications.Count == 0;
    }

}

