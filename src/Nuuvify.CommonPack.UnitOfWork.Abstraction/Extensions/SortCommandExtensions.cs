using Nuuvify.CommonPack.UnitOfWork.Abstraction.Enums;

namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Extensions;

/// <summary>
/// Extension methods for SortCommand enum.
/// </summary>
public static class SortCommandExtensions
{
    /// <summary>
    /// Converts a SortCommand enum value to its corresponding string representation.
    /// </summary>
    /// <param name="command">The SortCommand enum value.</param>
    /// <returns>The string representation of the command.</returns>
    public static string ToCommandString(this SortCommand command)
    {
        return command switch
        {
            SortCommand.OrderBy => "OrderBy",
            SortCommand.OrderByDescending => "OrderByDescending",
            SortCommand.ThenBy => "ThenBy",
            SortCommand.ThenByDescending => "ThenByDescending",
            _ => throw new ArgumentOutOfRangeException(nameof(command), command, null)
        };
    }
}