using Nuuvify.CommonPack.UnitOfWork.Abstraction.Enums;

namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Extensions;

/// <summary>
/// Extension methods for ExpressionParameterName enum.
/// </summary>
public static class ExpressionParameterNameExtensions
{
    /// <summary>
    /// Converts an ExpressionParameterName enum value to its corresponding string representation.
    /// </summary>
    /// <param name="parameterName">The ExpressionParameterName enum value.</param>
    /// <returns>The string representation of the parameter name.</returns>
    public static string ToParameterString(this ExpressionParameterName parameterName)
    {
        return parameterName switch
        {
            ExpressionParameterName.Model => "model",
            ExpressionParameterName.Entity => "entity",
            ExpressionParameterName.Item => "item",
            ExpressionParameterName.P => "p",
            ExpressionParameterName.Param => "param",
            ExpressionParameterName.X => "x",
            _ => throw new ArgumentOutOfRangeException(nameof(parameterName), parameterName, null)
        };
    }
}