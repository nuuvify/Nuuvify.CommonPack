namespace Nuuvify.CommonPack.Security.Abstraction;

public class PersonQueryResult
{

    /// <summary>
    /// Email do usuario
    /// </summary>
    /// <example>fulano@zzz.com</example>
    public string Email { get; set; }

    /// <summary>
    /// Login do usuario
    /// </summary>
    /// <example>fulangi</example>
    public string Login { get; set; }

    /// <summary>
    /// Nome da pessoa
    /// </summary>
    /// <example>Giropopis Fulano</example>
    public string Name { get; set; }

}
