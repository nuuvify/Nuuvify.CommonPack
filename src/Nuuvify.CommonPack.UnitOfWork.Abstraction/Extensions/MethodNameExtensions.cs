using Nuuvify.CommonPack.UnitOfWork.Abstraction.Enums;

namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Extensions;

/// <summary>
/// Extension methods for MethodName enum.
/// </summary>
public static class MethodNameExtensions
{
    /// <summary>
    /// Converts a MethodName enum value to its corresponding string representation.
    /// </summary>
    /// <param name="methodName">The MethodName enum value.</param>
    /// <returns>The string representation of the method name.</returns>
    public static string ToMethodString(this MethodName methodName)
    {
        return methodName switch
        {
            MethodName.ToUpper => "ToUpper",
            MethodName.StartsWith => "StartsWith",
            MethodName.Contains => "Contains",
            _ => throw new ArgumentOutOfRangeException(nameof(methodName), methodName, null)
        };
    }
}