using Nuuvify.CommonPack.UnitOfWork.Abstraction.Helpers;
using System.Diagnostics;
using System.Reflection;

namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Filter;

/// <summary>
/// Represents a where clause configuration for dynamic LINQ query filtering.
/// </summary>
[DebuggerDisplay("{FieldName}")]
public class WhereClause
{
    private bool _customName;

    /// <summary>
    /// Gets or sets whether the comparison should be case sensitive.
    /// </summary>
    public bool CaseSensitive { get; set; }

    /// <summary>
    /// Gets or sets the field name for the where clause.
    /// </summary>
    public string FieldName { get; set; }

    /// <summary>
    /// Gets or sets the operator to be used in the where clause.
    /// </summary>
    public WhereOperator Operator { get; set; }

    /// <summary>
    /// Gets or sets the property information associated with this clause.
    /// </summary>
    public PropertyInfo Property { get; set; }

    /// <summary>
    /// Gets or sets whether the condition should be negated (NOT).
    /// </summary>
    public bool UseNot { get; set; }

    /// <summary>
    /// Gets or sets whether to use OR logic instead of AND.
    /// </summary>
    public bool UseOr { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WhereClause"/> class with default settings.
    /// </summary>
    public WhereClause()
    {
        Operator = WhereOperator.Equals;
        UseNot = false;
        CaseSensitive = true;
    }

    /// <summary>
    /// Updates the where clause configuration based on the provided <see cref="QueryOperatorAttribute"/>.
    /// </summary>
    /// <param name="data">The attribute data to apply to this clause.</param>
    public void UpdateAttributeData(QueryOperatorAttribute data)
    {
        Operator = data.Operator;
        UseNot = data.UseNot;
        CaseSensitive = data.CaseSensitive;
        FieldName = data.HasName;
        UseOr = data.UseOr;

        if (!string.IsNullOrWhiteSpace(FieldName))
            _customName = true;
    }

    /// <summary>
    /// Updates the clause values based on the provided property information.
    /// </summary>
    /// <param name="propertyInfo">The property information to associate with this clause.</param>
    public void UpdateValues(PropertyInfo propertyInfo)
    {
        Property = propertyInfo;
        if (!_customName)
            FieldName = Property.Name;
    }

}
