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

        Expression lastExpression = null;

        var operations = ExpressionFactory.GetOperators<TEntity>(model);
        foreach (var expression in operations.Ordered())
        {
            if (expression.Criteria.Operator == WhereOperator.ContainsWithLikeForList)
            {
            }
            else if (!expression.Criteria.CaseSensitive)
            {
                expression.FieldToFilter = Expression.Call(expression.FieldToFilter,
                    typeof(string).GetMethods()
                        .First(m => m.Name == MethodName.ToUpper.ToMethodString() && m.GetParameters().Length == 0));

                expression.FilterBy = Expression.Call(expression.FilterBy,
                    typeof(string).GetMethods()
                        .First(m => m.Name == MethodName.ToUpper.ToMethodString() && m.GetParameters().Length == 0));
            }

            var actualExpression = GetExpression<TEntity>(expression);

            if (actualExpression == null)
            {
                continue;
            }

            if (expression.Criteria.UseNot)
            {
                actualExpression = Expression.Not(actualExpression);
            }

            if (lastExpression == null)
            {
                lastExpression = actualExpression;
            }
            else
            {
                if (expression.Criteria.UseOr)
                    lastExpression = Expression.Or(lastExpression, actualExpression);
                else
                    lastExpression = Expression.And(lastExpression, actualExpression);
            }
        }

        return lastExpression != null ? Expression.Lambda<Func<TEntity, bool>>(lastExpression, operations.ParameterExpression) : null;
    }
}