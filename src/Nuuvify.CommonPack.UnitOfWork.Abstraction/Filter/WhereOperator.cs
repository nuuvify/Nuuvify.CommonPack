namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Filter;

/// <summary>
/// Defines the comparison operators available for where clause conditions.
/// </summary>
public enum WhereOperator
{
    /// <summary>
    /// Equality comparison (==).
    /// </summary>
    Equals,

    /// <summary>
    /// Not equals comparison (!=).
    /// </summary>
    NotEquals,

    /// <summary>
    /// Greater than comparison (&gt;).
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Less than comparison (&lt;).
    /// </summary>
    LessThan,

    /// <summary>
    /// Greater than or equal comparison (&gt;=).
    /// </summary>
    GreaterThanOrEqualTo,

    /// <summary>
    /// Less than or equal comparison (&lt;=).
    /// </summary>
    LessThanOrEqualTo,

    /// <summary>
    /// String contains comparison.
    /// </summary>
    Contains,

    /// <summary>
    /// String starts with comparison.
    /// </summary>
    StartsWith,

    /// <summary>
    /// Less than or equal comparison for nullable types.
    /// </summary>
    LessThanOrEqualWhenNullable,

    /// <summary>
    /// Greater than or equal comparison for nullable types.
    /// </summary>
    GreaterThanOrEqualWhenNullable,

    /// <summary>
    /// Equality comparison for nullable types.
    /// </summary>
    EqualsWhenNullable,

    /// <summary>
    /// Contains comparison with LIKE functionality for string lists.
    /// Performs OR-based string contains operations across all items in a list.
    /// </summary>
    ContainsWithLikeForList
}
