namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Enums;

/// <summary>
/// Enumeration for LINQ sorting commands.
/// </summary>
public enum SortCommand
{
    /// <summary>
    /// Represents the OrderBy command for ascending order on the first field.
    /// </summary>
    OrderBy,

    /// <summary>
    /// Represents the OrderByDescending command for descending order on the first field.
    /// </summary>
    OrderByDescending,

    /// <summary>
    /// Represents the ThenBy command for ascending order on subsequent fields.
    /// </summary>
    ThenBy,

    /// <summary>
    /// Represents the ThenByDescending command for descending order on subsequent fields.
    /// </summary>
    ThenByDescending
}