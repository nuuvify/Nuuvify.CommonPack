using System.Reflection;

namespace Nuuvify.CommonPack.Extensions.Implementation;


public static class ReflectionConstantsExtension
{

    /// <summary>
    /// Retorna uma lista do tipo FieldInfo contendo os campos "public const" de uma determinada 
    /// classe
    /// <example>
    /// <code>
    /// foreach (var item in typeof(PolicyGroupConstants).GetPublicConstants())
    /// {
    ///     var value = item.GetValue(item.Name).ToString();
    ///
    ///     options.AddPolicy(value,
    ///         policy => policy.Requirements.Add(new ControllerRequirement(value)));
    ///
    /// }
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static IEnumerable<FieldInfo> GetPublicConstants(this Type type)
    {

        var consts = type
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
            .Concat(type.GetNestedTypes(BindingFlags.Public)
                .SelectMany(GetPublicConstants));

        return consts;

    }
}
