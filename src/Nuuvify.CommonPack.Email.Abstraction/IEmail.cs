using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Email.Abstraction;

public interface IEmail
{
    /// <summary>
    /// Obtem as mensagens de inconsistencias ocorridas dentro da classe
    /// </summary>
    List<NotificationR> Notifications { get; set; }

    /// <summary>
    /// Obtem as mensagens para log, não são inconsistencias
    /// </summary>
    List<NotificationR> LogMessage { get; set; }

    /// <summary>
    /// Essa propriedade mantem a configuração do servidor de email
    /// </summary>
    /// <value></value>
    EmailServerConfiguration EmailServerConfiguration { get; set; }

    bool IsValid();

    /// <summary>
    /// Crie a tag EmailServerConfiguration no arquivo appsettings.json
    /// <example>
    /// <code>
    /// EmailServerConfiguration: {
    ///     ServerHost: smpt.gmail.com
    ///     Port: Porta para seu provedor de email
    ///     Security: None, SSL, TLS
    ///     AccountUserName: fulano@gmail.com
    ///     AccountPassword: senha do seu provedor de email
    /// }
    /// 
    /// Para ler os endereços de e-mail no appsettings.json utilize:
    /// 
    /// var emails = _configuration.GetChildren("EmailConfig:RemetenteEmail")
    ///                            .ToDictionary(x => x.Key, x => x.Value);
    /// </code> 
    /// </example>
    /// </summary>
    /// <param name="recipients">Lista do tipo Dictionary onde KEY=e-mail, VALUE=Nome</param>
    /// <param name="senders">Lista do tipo Dictionary onde KEY=e-mail, VALUE=Nome</param>
    /// <param name="subject">Titulo do email</param>
    /// <param name="message">Monte sua mensagem ou template usando HTML, mas passe aqui como string, o metodo ira entender que é HTML</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<object> EnviarAsync(
        Dictionary<string, string> recipients,
        Dictionary<string, string> senders,
        string subject,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna o template html informado, com as variaveis substituidas e pronto para ser utilizado no envio de email
    /// a biblioteca de envio de email ira sempre converter para HTML
    /// </summary>
    /// <param name="variables">Key = Variaveis contidas no tamplate, Value = Valor que severa substituir a variavel</param>
    /// <param name="templateFullName">Caminho e nome completo do template, Exemplo: Path.Combine(AppSettingsConfig.ProjectPath, "Templates", "template-email.html")</param>
    /// <returns></returns>
    string GetEmailTemplate(
        Dictionary<string, string> variables,
        string templateFullName);

    /// <summary>
    /// Inclua todos os arquivos que forem anexos ao e-mail
    /// </summary>
    /// <param name="pathFileFullName">Informe o caminho e nome completo do arquivo</param>
    /// <param name="midiaType">Conforme a classe EmailMidiaType</param>
    /// <param name="midiaSubType">Conforme a classe  EmailMidiaSubType</param>
    /// <returns></returns>
    IEmail WithAttachment(
        string pathFileFullName,
        EmailMidiaType midiaType,
        EmailMidiaSubType midiaSubType);

    /// <summary>
    /// Inclua todos os arquivos que forem anexos ao e-mail
    /// </summary>
    /// <param name="fileStream">Informe o arquivo como FileStream</param>
    /// <param name="midiaType">Conforme a classe EmailMidiaType</param>
    /// <param name="midiaSubType">Conforme a classe EmailMidiaSubType</param>
    /// <param name="fullFileName">Informe o nome completo do arquivo (exemplo: texto_001.txt)</param>
    /// <returns></returns>
    IEmail WithAttachment(
        Stream fileStream,
        EmailMidiaType midiaType,
        EmailMidiaSubType midiaSubType,
        string fullFileName);

    /// <summary>
    /// Remove todos os arquivos anexados
    /// </summary>
    /// <returns></returns>
    IEmail RemoveAllAttachments();

    /// <summary>
    /// Remove todas as mensagens de Log
    /// </summary>
    /// <returns></returns>
    IEmail RemoveAllLogMessage();

    /// <summary>
    /// Remove todas as notificações
    /// </summary>
    /// <returns></returns>
    IEmail RemoveAllNotifications();

    /// <summary>
    /// Limpa as propriedades da instancia para que seja possivel incluir novos valores.
    /// Isso é especialmente util para envio de email dentro de um Looping
    /// </summary>
    /// <returns></returns>
    void ResetMailInstance();

    /// <summary>
    /// Numero de documentos anexados
    /// </summary>
    /// <value></value>
    int CountAttachments();

}
