namespace Nuuvify.CommonPack.Security.Abstraction;

public class PersonWithRolesQueryResult : PersonQueryResult
{

    public PersonWithRolesQueryResult()
    {
        Groups = new List<PersonRoleQueryResult>();
    }

    public virtual IEnumerable<PersonRoleQueryResult> Groups { get; set; }
}
