using Nuuvify.CommonPack.UnitOfWork.Abstraction.Filter;

namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Helpers;

/// <summary>
/// Attribute to define query operator settings for properties used in dynamic filtering.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class QueryOperatorAttribute : Attribute
{
    /// <summary>
    /// Gets or sets whether the comparison should be case sensitive. Default is true.
    /// </summary>
    public bool CaseSensitive { get; set; } = true;

    /// <summary>
    /// Gets or sets a custom name for the property in the query.
    /// </summary>
    public string HasName { get; set; }

    /// <summary>
    /// Gets or sets the maximum value for numeric comparisons.
    /// </summary>
    public int Max { get; set; }

    /// <summary>
    /// Gets or sets the comparison operator to use. Default is <see cref="WhereOperator.Equals"/>.
    /// </summary>
    public WhereOperator Operator { get; set; } = WhereOperator.Equals;

    /// <summary>
    /// Gets or sets whether to negate the condition (NOT). Default is false.
    /// </summary>
    public bool UseNot { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to use OR logic instead of AND. Default is false.
    /// </summary>
    public bool UseOr { get; set; } = false;
}
