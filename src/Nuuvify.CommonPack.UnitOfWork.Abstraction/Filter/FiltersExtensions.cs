using System.Linq.Expressions;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Enums;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Extensions;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Helpers;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Filter;

/// <summary>
/// Provides extension methods for filtering <see cref="IQueryable{T}"/> collections using dynamic filter expressions.
/// This class enables building complex LINQ queries dynamically based on decorated model properties.
/// </summary>
public static partial class FiltersExtensions
{
    /// <summary>
    /// Applies dynamic filtering to an <see cref="IQueryable{TEntity}"/> based on filter criteria defined in the model.
    /// </summary>
    /// <typeparam name="TEntity">The type of entities in the queryable collection.</typeparam>
    /// <param name="result">The queryable collection to filter.</param>
    /// <param name="model">
    /// The filter model containing properties decorated with <see cref="QueryOperatorAttribute"/>
    /// that define the filter criteria and operators to apply.
    /// </param>
    /// <returns>
    /// A filtered <see cref="IQueryable{TEntity}"/> with WHERE clauses applied based on the model's criteria.
    /// If the model is null or no valid filters exist, returns the original queryable unchanged.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method uses reflection to analyze the model's properties and their <see cref="QueryOperatorAttribute"/>
    /// decorations to build dynamic LINQ WHERE expressions at runtime.
    /// </para>
    /// <para>
    /// The method supports various filter operators including:
    /// <list type="bullet">
    /// <item>Equality operators (Equals, NotEquals)</item>
    /// <item>Comparison operators (GreaterThan, LessThan, etc.)</item>
    /// <item>String operators (Contains, StartsWith, ContainsWithLikeForList)</item>
    /// <item>Nullable-aware operators (EqualsWhenNullable, etc.)</item>
    /// </list>
    /// </para>
    /// <para>
    /// Filters can be combined using AND/OR logic and support case-sensitive/insensitive matching.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class ProductFilter : IQueryableCustom
    /// {
    ///     [QueryOperator(WhereOperator.Contains, CaseSensitive = false)]
    ///     public string Name { get; set; }
    ///
    ///     [QueryOperator(WhereOperator.GreaterThanOrEqualTo)]
    ///     public decimal? MinPrice { get; set; }
    ///
    ///     [QueryOperator(WhereOperator.LessThanOrEqualTo)]
    ///     public decimal? MaxPrice { get; set; }
    /// }
    ///
    /// var filter = new ProductFilter
    /// {
    ///     Name = "Laptop",
    ///     MinPrice = 500,
    ///     MaxPrice = 2000
    /// };
    ///
    /// var query = dbContext.Products
    ///     .Filter(filter);
    ///
    /// // Generated SQL:
    /// // SELECT * FROM Products
    /// // WHERE UPPER(Name) LIKE '%LAPTOP%'
    /// //   AND Price &gt;= 500
    /// //   AND Price &lt;= 2000
    /// </code>
    /// </example>
    public static IQueryable<TEntity> Filter<TEntity>(
        this IQueryable<TEntity> result,
        IQueryableCustom model)
    {
        if (model == null)
        {
            return result;
        }

        var lastExpression = result.FilterExpression(model);
        return lastExpression == null
            ? result
            : result.Where(lastExpression);
    }

    /// <summary>
    /// Builds a dynamic filter expression based on the model's criteria without executing it against the queryable.
    /// </summary>
    /// <typeparam name="TEntity">The type of entities being filtered.</typeparam>
    /// <param name="result">The queryable collection (used for type inference only, not modified).</param>
    /// <param name="model">
    /// The filter model containing properties decorated with <see cref="QueryOperatorAttribute"/>
    /// that define the filter criteria and operators.
    /// </param>
    /// <returns>
    /// An <see cref="Expression{TDelegate}"/> of type <see cref="Func{TEntity, Boolean}"/> representing the combined filter logic,
    /// or null if the model is null or no valid filters are defined.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method generates the filter expression tree without applying it to the queryable,
    /// allowing for expression inspection, composition, or deferred execution.
    /// </para>
    /// <para>
    /// The expression tree combines multiple filter criteria using AND/OR operators based on
    /// the <see cref="QueryOperatorAttribute.UseOr"/> setting on each property.
    /// </para>
    /// <para>
    /// For <see cref="WhereOperator.ContainsWithLikeForList"/>, case-insensitivity is handled
    /// internally within the operator implementation rather than at the expression level.
    /// </para>
    /// <para>
    /// Filters that return null (such as empty lists in ContainsWithLikeForList) are automatically
    /// skipped to avoid generating invalid SQL like "WHERE 0 = 1".
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class ProductFilter : IQueryableCustom
    /// {
    ///     [QueryOperator(WhereOperator.ContainsWithLikeForList, CaseSensitive = false)]
    ///     public List&lt;string&gt; Categories { get; set; }
    ///
    ///     [QueryOperator(WhereOperator.GreaterThan)]
    ///     public decimal MinPrice { get; set; }
    /// }
    ///
    /// var filter = new ProductFilter
    /// {
    ///     Categories = new List&lt;string&gt; { "Electronics", "Computers" },
    ///     MinPrice = 100
    /// };
    ///
    /// var filterExpression = dbContext.Products
    ///     .FilterExpression(filter);
    ///
    /// // Expression tree: p =&gt; (p.Category.Contains("ELECTRONICS") || p.Category.Contains("COMPUTERS"))
    /// //                      &amp;&amp; p.Price &gt; 100
    ///
    /// // Can be applied later or combined with other expressions:
    /// var query = dbContext.Products.Where(filterExpression);
    /// </code>
    /// </example>
    public static Expression<Func<TEntity, bool>> FilterExpression<TEntity>(
        this IQueryable<TEntity> result,
        IQueryableCustom model)
    {
        if (model == null) return null;

        var operations = ExpressionFactory.GetOperators<TEntity>(model);
        var combinedExpression = BuildCombinedFilterExpression<TEntity>(operations);

        return combinedExpression != null
            ? Expression.Lambda<Func<TEntity, bool>>(combinedExpression, operations.ParameterExpression)
            : null;
    }

    /// <summary>
    /// Builds a combined filter expression from multiple expression parsers.
    /// Processes each expression parser to apply case sensitivity, negation, and logical combination (AND/OR).
    /// </summary>
    /// <typeparam name="TEntity">The entity type being filtered</typeparam>
    /// <param name="operations">Collection of expression parsers to process</param>
    /// <returns>Combined expression tree, or null if no valid expressions found</returns>
    private static Expression BuildCombinedFilterExpression<TEntity>(ExpressionParserCollection operations)
    {
        Expression combinedExpression = null;

        foreach (var expressionParser in operations.Ordered())
        {
            var processedExpression = ProcessSingleExpression<TEntity>(expressionParser);

            if (processedExpression != null)
            {
                combinedExpression = CombineExpressions(combinedExpression, processedExpression, expressionParser.Criteria.UseOr);
            }
        }

        return combinedExpression;
    }

    /// <summary>
    /// Processes a single expression parser to handle case sensitivity, create the filter expression, and apply negation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being filtered</typeparam>
    /// <param name="expressionParser">The expression parser to process</param>
    /// <returns>Processed expression tree, or null if expression creation failed</returns>
    private static Expression ProcessSingleExpression<TEntity>(ExpressionParser expressionParser)
    {
        ApplyCaseSensitivityTransforms(expressionParser);

        var actualExpression = GetExpression<TEntity>(expressionParser);

        if (actualExpression == null)
            return null;

        return ApplyNegationIfNeeded(actualExpression, expressionParser.Criteria.UseNot);
    }

    /// <summary>
    /// Applies case sensitivity transformations to expression fields when case-insensitive filtering is required.
    /// ContainsWithLikeForList operator handles case sensitivity internally and is skipped.
    /// </summary>
    /// <param name="expressionParser">The expression parser to transform</param>
    private static void ApplyCaseSensitivityTransforms(ExpressionParser expressionParser)
    {
        if (expressionParser.Criteria.Operator == WhereOperator.ContainsWithLikeForList)
        {
            return; // ContainsWithLikeForList handles case sensitivity internally
        }

        if (!expressionParser.Criteria.CaseSensitive)
        {
            var toUpperMethod = GetStringToUpperMethod();
            expressionParser.FieldToFilter = Expression.Call(expressionParser.FieldToFilter, toUpperMethod);
            expressionParser.FilterBy = Expression.Call(expressionParser.FilterBy, toUpperMethod);
        }
    }

    /// <summary>
    /// Gets the string ToUpper method for case-insensitive string comparisons.
    /// </summary>
    /// <returns>MethodInfo for the string ToUpper method</returns>
    private static System.Reflection.MethodInfo GetStringToUpperMethod()
    {
        return typeof(string).GetMethods()
            .First(m => m.Name == MethodName.ToUpper.ToMethodString() && m.GetParameters().Length == 0);
    }

    /// <summary>
    /// Applies logical negation to an expression if the UseNot flag is set.
    /// </summary>
    /// <param name="expression">The expression to potentially negate</param>
    /// <param name="useNot">Whether to apply negation</param>
    /// <returns>Negated expression if useNot is true, otherwise the original expression</returns>
    private static Expression ApplyNegationIfNeeded(Expression expression, bool useNot)
    {
        return useNot ? Expression.Not(expression) : expression;
    }

    /// <summary>
    /// Combines two expressions using either AND or OR logic.
    /// If the existing expression is null, returns the new expression as the starting point.
    /// </summary>
    /// <param name="existingExpression">The existing combined expression (can be null)</param>
    /// <param name="newExpression">The new expression to combine</param>
    /// <param name="useOr">Whether to use OR logic (true) or AND logic (false)</param>
    /// <returns>Combined expression using the specified logical operator</returns>
    private static Expression CombineExpressions(Expression existingExpression, Expression newExpression, bool useOr)
    {
        if (existingExpression == null)
            return newExpression;

        return useOr
            ? Expression.Or(existingExpression, newExpression)
            : Expression.And(existingExpression, newExpression);
    }
}
