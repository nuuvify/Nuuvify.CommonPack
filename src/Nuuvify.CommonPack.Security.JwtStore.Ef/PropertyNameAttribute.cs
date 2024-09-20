
namespace Nuuvify.CommonPack.Security.JwtStore.Ef;

[AttributeUsage(AttributeTargets.Property)]
public class PropertyNameAttribute : Attribute
{
    public string Name { get; }

    public PropertyNameAttribute(string name)
    {
        Name = name;
    }
}
