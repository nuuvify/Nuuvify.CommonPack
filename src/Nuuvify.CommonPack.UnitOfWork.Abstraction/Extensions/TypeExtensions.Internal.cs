using System.Collections.Concurrent;
using System.Reflection;

namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Extensions;

public static partial class TypeExtensions
{
    private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> s_concurrentDictionaryCustom;

    static TypeExtensions()
    {
        s_concurrentDictionaryCustom = new ConcurrentDictionary<Type, List<PropertyInfo>>();
    }

    private static bool IsGenericList(Type type)
    {
        return type.IsGenericType &&
               type.GetInterfaces().Any(
                   x =>
                   x == typeof(IEnumerable<>) || x.Name == "IEnumerable" ||
                   x == typeof(IAsyncEnumerable<>) || x.Name == "IAsyncEnumerable" ||
                   x == typeof(IList<>) || x.Name == "IList" ||
                   x == typeof(ICollection<>) || x.Name == "ICollection" ||
                   x == typeof(IReadOnlyCollection<>) || x.Name == "IReadOnlyCollection" ||
                   x == typeof(IReadOnlyList<>) || x.Name == "IReadOnlyList");
    }

    internal static List<PropertyInfo> GetAllProperties(this Type type)
    {
        if (s_concurrentDictionaryCustom.ContainsKey(type))
            return s_concurrentDictionaryCustom[type];

        var properties = type.GetProperties().ToList();
        _ = s_concurrentDictionaryCustom.TryAdd(type, properties);

        return properties;
    }

    internal static PropertyInfo GetProperty(Type type, string name)
    {
        return type
            .GetAllProperties()
            .FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    internal static PropertyInfo GetProperty<TEntity>(string name)
    {
        return typeof(TEntity)
            .GetAllProperties()
            .FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}