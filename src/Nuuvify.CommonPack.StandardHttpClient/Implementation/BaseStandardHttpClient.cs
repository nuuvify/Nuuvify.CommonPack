using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.JsonConverter;
using Nuuvify.CommonPack.Extensions.Notificator;
using Nuuvify.CommonPack.Security.Abstraction;
using Nuuvify.CommonPack.StandardHttpClient.Results;

namespace Nuuvify.CommonPack.StandardHttpClient;

public abstract partial class BaseStandardHttpClient
{
    private const string ApplicationErrorType = "application";
    private const string OriginMessageType = "origin-message";

    private readonly IStandardHttpClient _standardHttpClient;
    private readonly ITokenService _tokenService;
    private readonly List<NotificationR> _notifications;

    /// <summary>
    /// Você pode alterar a configuração que é realizada no construtor
    /// </summary>
    /// <value></value>
    protected JsonSerializerOptions JsonSettings { get; set; }

    /// <summary>
    /// Lista de notificações coletadas durante operações HTTP
    /// </summary>
    protected ReadOnlyCollection<NotificationR> Notifications => new ReadOnlyCollection<NotificationR>(_notifications);

    /// <summary>
    /// Instância do cliente HTTP padrão para execução de requisições HTTP
    /// </summary>
    /// <value>Cliente HTTP configurado com as políticas de retry, circuit breaker e timeout</value>
    protected IStandardHttpClient StandardHttpClient => _standardHttpClient;

    /// <summary>
    /// Serviço de gerenciamento de tokens de autenticação
    /// </summary>
    /// <value>Serviço responsável por obter, renovar e gerenciar tokens de acesso para APIs</value>
    protected ITokenService TokenService => _tokenService;

    protected BaseStandardHttpClient(
        IStandardHttpClient standardHttpClient,
        ITokenService tokenService)
    {
        _standardHttpClient = standardHttpClient;
        _notifications = new List<NotificationR>();

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
    public virtual async Task<bool> GetTokenAsync(
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

        AddNotifications(_tokenService.Notifications);
        return token.IsValidToken();
    }
    ///<inheritdoc cref="ITokenService.GetToken"/>
    public virtual async Task<CredentialToken> GetToken(
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

        AddNotifications(_tokenService.Notifications);
        return token;
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

        AddNotification(new NotificationR(
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
    /// <param name="standardReturn">Resultado da requisição HTTP</param>
    /// <param name="api">Nome da API para identificação em logs</param>
    /// <param name="jsonDataDepth">Profundidade da propriedade 'data' no JSON. Use:
    /// - 0: Quando o objeto está no nível raiz (ex: {"id": 1, "nome": "João"})
    /// - 1: Padrão, quando há uma propriedade 'data' (ex: {"data": {"id": 1, "nome": "João"}})
    /// - 2: Para JSON aninhado (ex: {"response": {"data": {"id": 1, "nome": "João"}}})</param>
    /// <typeparam name="T">Tipo da classe para deserialização</typeparam>
    /// <returns>Objeto deserializado do tipo T ou null em caso de erro</returns>
    public virtual T ReturnClass<T>(
        HttpStandardReturn standardReturn,
        string api,
        int jsonDataDepth = 1) where T : class
    {
        if (standardReturn is null)
        {
            AddNotification(new NotificationR(
                property: "BaseStandardHttpClient.ReturnClass",
                message: $"Retorno HTTP nulo para a classe {typeof(T).Name}-Correlation: {_standardHttpClient.CorrelationId}",
                aggregatorId: api,
                type: ApplicationErrorType,
                originNotification: null));

            return (T)Convert.ChangeType(null, typeof(T), CultureInfo.InvariantCulture);
        }

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
            AddNotification(new NotificationR(
                property: "ReturnClass<T>",
                message: $"Não foi possivel deserializar o retorno para a classe {typeof(T).Name}-Correlation: {_standardHttpClient.CorrelationId}. ReturCode: {standardReturn?.ReturnCode}",
                aggregatorId: api,
                type: ApplicationErrorType,
                originNotification: null));

            AddNotification(new NotificationR(
                property: "ReturnClass<T>",
                message: messageClean,
                aggregatorId: api,
                type: OriginMessageType,
                originNotification: null));

            return (T)Convert.ChangeType(null, typeof(T), CultureInfo.InvariantCulture);
        }
        else if (standardReturn?.ReturnCode != "422")
        {

            AddNotification(new NotificationR(property: typeof(T).Name,
                message: $"Não houve sucesso no retorno da request para a classe {nameof(HttpStandardReturn)}-Correlation: {_standardHttpClient.CorrelationId}",
                aggregatorId: api,
                type: ApplicationErrorType,
                originNotification: null));

            AddNotification(new NotificationR(property: typeof(T).Name,
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
    /// <param name="standardReturn">Resultado da requisição HTTP</param>
    /// <param name="api">Nome da API para identificação em logs</param>
    /// <param name="jsonDataDepth">Profundidade da propriedade 'data' no JSON. Use:
    /// - 0: Quando o array está no nível raiz (ex: [{"id": 1}, {"id": 2}])
    /// - 1: Padrão, quando há uma propriedade 'data' (ex: {"data": [{"id": 1}, {"id": 2}]})
    /// - 2: Para JSON aninhado (ex: {"response": {"data": [{"id": 1}, {"id": 2}]}})</param>
    /// <typeparam name="T">Tipo da classe para deserialização dos itens da lista</typeparam>
    /// <returns>Lista de objetos do tipo T ou lista vazia em caso de erro</returns>
    public virtual IList<T> ReturnList<T>(
        HttpStandardReturn standardReturn,
        string api,
        int jsonDataDepth = 1) where T : class
    {

        if (standardReturn is null)
        {
            AddNotification(new NotificationR(
                property: "BaseStandardHttpClient.ReturnList",
                message: $"Retorno HTTP nulo para a classe {typeof(T).Name}-Correlation: {_standardHttpClient.CorrelationId}",
                aggregatorId: api,
                type: ApplicationErrorType,
                originNotification: null));

            return new List<T>();
        }

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
            AddNotification(new NotificationR(property: "ReturnList<>",
                message: $"Não foi possivel deserializar o retorno para a classe {typeof(T).Name}-Correlation: {_standardHttpClient.CorrelationId}. ReturCode: {standardReturn?.ReturnCode}",
                aggregatorId: api,
                type: ApplicationErrorType,
                originNotification: null));

            AddNotification(new NotificationR(property: "ReturnList<>",
                message: messageClean,
                aggregatorId: api,
                type: OriginMessageType,
                originNotification: null));

            return new List<T>();
        }
        else if (standardReturn?.ReturnCode != "422")
        {
            AddNotification(new NotificationR(property: typeof(T).Name,
                message: $"Não houve sucesso no retorno da request para a classe {nameof(HttpStandardReturn)}-Correlation: {_standardHttpClient.CorrelationId}",
                aggregatorId: api,
                type: ApplicationErrorType,
                originNotification: null));

            AddNotification(new NotificationR(property: "ReturnList<>",
                message: standardReturn?.ReturnMessage,
                aggregatorId: api,
                type: OriginMessageType,
                originNotification: null));
        }

        ReturnNotificationApi(standardReturn, api);
        return new List<T>();
    }

    /// <summary>
    /// Verifica se não há notificações (indica operação válida)
    /// </summary>
    /// <returns>True se não houver notificações, False caso contrário</returns>
    public bool IsValid()
    {
        return _notifications.Count == 0;
    }

    /// <summary>
    /// Adiciona uma notificação à lista
    /// </summary>
    /// <param name="notification">Notificação a ser adicionada</param>
    protected void AddNotification(NotificationR notification)
    {
        if (notification != null)
        {
            _notifications.Add(notification);
        }
    }

    /// <summary>
    /// Adiciona uma coleção de notificações à lista
    /// </summary>
    /// <param name="notifications">Coleção de notificações</param>
    protected void AddNotifications(IEnumerable<NotificationR> notifications)
    {
        if (notifications != null)
        {
            _notifications.AddRange(notifications);
        }
    }

    /// <summary>
    /// Remove todas as notificações
    /// </summary>
    protected void ClearNotifications()
    {
        _notifications.Clear();
    }

    /// <summary>
    /// Remove notificações por propriedade
    /// </summary>
    /// <param name="property">Nome da propriedade</param>
    /// <returns>Número de notificações removidas</returns>
    protected int RemoveNotifications(string property)
    {
        if (!string.IsNullOrEmpty(property))
        {
            return _notifications.RemoveAll(x => x.Property?.Equals(property, StringComparison.OrdinalIgnoreCase) == true);
        }
        return 0;
    }

}

