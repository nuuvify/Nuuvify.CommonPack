namespace Nuuvify.CommonPack.Security.Abstraction;

public class PersonRoleQueryResult
{
    /// <summary>
    /// Login do usuario
    /// </summary>
    /// <example>fulangi</example>
    public string Login { get; set; }
    /// <summary>
    /// Nome do grupo
    /// </summary>
    /// <example>Contabilidade-Users</example>
    public string Group { get; set; }
}
