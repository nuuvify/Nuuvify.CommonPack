namespace Nuuvify.CommonPack.Middleware.Abstraction;

/// <summary>
/// Essa implementaçao evita a dependencia das classes do AspNet em projetos como o de dominio.
/// Essa classe é um encapsulamento das interfaces IConfiguration e IHostEnvironment, que
/// aqui são representadas por IConfigurationCustom
/// </summary>
public interface IConfigurationCustom
{

    /// <summary>
    /// Para converter um IDictionary em Dictionary, execute:
    /// _configuration.GetSection(path)
    ///               .ToDictionary(x => x.Key, x => x.Value);
    /// </summary>
    /// <remarks>
    /// Se você precisa ler uma tag appsettings.json como essa: 
    ///     "SuporteEmailEndereco": {
    ///        "suporte@nuuve.com.br": "Lincoln",
    ///        "lzoca00@gmail.com": "Lincoln Fake"
    ///      }
    /// Onde a KEY é variante, utilize GetChildren
    /// </remarks>
    /// <param name="key"></param>
    /// <returns></returns>
    IDictionary<string, string> GetSection(string key);

    /// <summary>
    /// <see cref="GetSection"/>
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    IDictionary<string, string> GetChildren(string path);

    string GetConnectionString(string name);

    /// <summary>
    /// Obtem o valor de uma tag do arquivo appsettings. Utilize : para representar os agrupamentos do json
    /// </summary>
    /// <param name="key">Nome da tag</param>
    /// <returns></returns>
    string GetSectionValue(string key);

    /// <summary>
    /// Retorna o CorrelationId referente a Request atual
    /// </summary>
    /// <returns></returns>
    string GetCorrelationId();

    /// <summary>
    /// Retorna uma instancia de uma classe, com dados parametrozados no appsettings.json
    /// </summary>
    /// <typeparam name="TConfiguration">Classe da qual deseja obter uma instancia</typeparam>
    TConfiguration ConfigurationOptions<TConfiguration>(string getSection) where TConfiguration : class;

    string EnvironmentName
    {
        get;
        set;
    }

    string ApplicationName
    {
        get;
        set;
    }

    string ContentRootPath
    {
        get;
        set;
    }

    string ApplicationVersion { get; }
    string ApplicationBuild { get; }

    ///<inheritdoc cref = "Nuuvify.CommonPack.Middleware.Abstraction.RequestConfiguration.ApplicationRelease"/>
    string ApplicationRelease { get; }

    bool IsDevelopment();
    bool IsEnvironment(string name);
    bool IsProduction();
    bool IsStaging();

}
