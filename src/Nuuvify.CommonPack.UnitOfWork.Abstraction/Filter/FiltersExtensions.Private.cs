using System.Linq.Expressions;
using System.Reflection;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Enums;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Extensions;

namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Filter;

public static partial class FiltersExtensions
{
    private static Expression GetExpression<TEntity>(ExpressionParser expression)
    {
        return expression.Criteria.Operator switch
        {
            WhereOperator.Equals => Expression.Equal(expression.FieldToFilter, expression.FilterBy),
            WhereOperator.NotEquals => Expression.NotEqual(expression.FieldToFilter, expression.FilterBy),
            WhereOperator.GreaterThan => Expression.GreaterThan(expression.FieldToFilter, expression.FilterBy),
            WhereOperator.LessThan => Expression.LessThan(expression.FieldToFilter, expression.FilterBy),
            WhereOperator.GreaterThanOrEqualTo => Expression.GreaterThanOrEqual(expression.FieldToFilter, expression.FilterBy),
            WhereOperator.LessThanOrEqualTo => Expression.LessThanOrEqual(expression.FieldToFilter, expression.FilterBy),
            WhereOperator.Contains => ContainsExpression<TEntity>(expression),
            WhereOperator.GreaterThanOrEqualWhenNullable => GreaterThanOrEqualWhenNullable(expression.FieldToFilter, expression.FilterBy),
            WhereOperator.LessThanOrEqualWhenNullable => LessThanOrEqualWhenNullable(expression.FieldToFilter, expression.FilterBy),
            WhereOperator.EqualsWhenNullable => EqualsWhenNullable(expression.FieldToFilter, expression.FilterBy),
            WhereOperator.StartsWith => Expression.Call(expression.FieldToFilter,
                                typeof(string).GetMethods()
                                    .First(m => m.Name == MethodName.StartsWith.ToMethodString() && m.GetParameters().Length == 1),
                                expression.FilterBy),
            WhereOperator.ContainsWithLikeForList => ContainsWithLikeForListExpression<TEntity>(expression),
            _ => Expression.Equal(expression.FieldToFilter, expression.FilterBy),
        };
    }

    private static Expression LessThanOrEqualWhenNullable(Expression e1, Expression e2)
    {
        if (IsNullableType(e1.Type) &&
            !IsNullableType(e2.Type))
            e2 = Expression.Convert(e2, e1.Type);

        else if (!IsNullableType(e1.Type) &&
                IsNullableType(e2.Type))
            e1 = Expression.Convert(e1, e2.Type);

        return Expression.LessThanOrEqual(e1, e2);
    }

    private static Expression GreaterThanOrEqualWhenNullable(Expression e1, Expression e2)
    {
        if (IsNullableType(e1.Type) && !IsNullableType(e2.Type))
            e2 = Expression.Convert(e2, e1.Type);

        else if (!IsNullableType(e1.Type) && IsNullableType(e2.Type))
            e1 = Expression.Convert(e1, e2.Type);

        return Expression.GreaterThanOrEqual(e1, e2);
    }

    private static Expression EqualsWhenNullable(Expression e1, Expression e2)
    {
        if (IsNullableType(e1.Type) && !IsNullableType(e2.Type))
            e2 = Expression.Convert(e2, e1.Type);

        else if (!IsNullableType(e1.Type) && IsNullableType(e2.Type))
            e1 = Expression.Convert(e1, e2.Type);

        return Expression.Equal(e1, e2);
    }

    private static bool IsNullableType(Type t)
    {
        return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    private static Expression ContainsExpression<TEntity>(ExpressionParser expression)
    {
        if (expression.Criteria.Property.IsPropertyACollection())
        {
            var methodToApplyContains = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Single(x => x.Name == MethodName.Contains.ToMethodString() && x.GetParameters().Length == 2)
                .MakeGenericMethod(expression.FieldToFilter.Type);
            return Expression.Call(methodToApplyContains, expression.FilterBy, expression.FieldToFilter);
        }
        else
        {
            var methodToApplyContains = expression.FieldToFilter.Type.GetMethods()
                .First(m => m.Name == MethodName.Contains.ToMethodString() && m.GetParameters().Length == 1);

            return Expression.Call(expression.FieldToFilter, methodToApplyContains, expression.FilterBy);
        }
    }

    /// <summary>
    /// Creates a complex OR expression that checks if a field contains any of the patterns from a list.
    /// Supports case-sensitive and case-insensitive matching based on criteria configuration.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being filtered</typeparam>
    /// <param name="expression">The expression parser containing field and filter information</param>
    /// <returns>An OR expression combining all pattern matches, or null if no valid patterns found</returns>
    private static Expression ContainsWithLikeForListExpression<TEntity>(ExpressionParser expression)
    {
        var patterns = ExtractPatternsFromExpression(expression.FilterBy);
        if (patterns == null)
            return null;

        Expression orExpression = null;

        foreach (var pattern in patterns)
        {
            if (IsValidPattern(pattern))
            {
                var containsExpression = CreateContainsExpressionForPattern(expression, pattern);
                orExpression = CombineWithOrExpression(orExpression, containsExpression);
            }
        }

        return orExpression;
    }

    /// <summary>
    /// Extracts the enumerable patterns from the filter expression, handling conversion expressions if necessary.
    /// </summary>
    /// <param name="filterExpression">The filter expression to extract patterns from</param>
    /// <returns>Enumerable of patterns or null if extraction fails</returns>
    private static System.Collections.IEnumerable ExtractPatternsFromExpression(Expression filterExpression)
    {
        var actualExpression = UnwrapConversionExpression(filterExpression);

        if (actualExpression is not ConstantExpression constantExpression)
            return null;

        return constantExpression.Value as System.Collections.IEnumerable;
    }

    /// <summary>
    /// Unwraps conversion expressions to get the underlying expression.
    /// </summary>
    /// <param name="expression">The expression to unwrap</param>
    /// <returns>The unwrapped expression or the original if no conversion found</returns>
    private static Expression UnwrapConversionExpression(Expression expression)
    {
        return expression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression
            ? unaryExpression.Operand
            : expression;
    }

    /// <summary>
    /// Validates if a pattern is valid (not null and not empty).
    /// </summary>
    /// <param name="pattern">The pattern to validate</param>
    /// <returns>True if pattern is valid, false otherwise</returns>
    private static bool IsValidPattern(object pattern)
    {
        return pattern != null && !string.IsNullOrEmpty(pattern.ToString());
    }

    /// <summary>
    /// Creates a contains expression for a specific pattern, handling case sensitivity based on criteria.
    /// </summary>
    /// <param name="expression">The expression parser containing field and criteria information</param>
    /// <param name="pattern">The pattern to create the contains expression for</param>
    /// <returns>A contains expression or null if method cannot be found</returns>
    private static Expression CreateContainsExpressionForPattern(ExpressionParser expression, object pattern)
    {
        var patternString = pattern.ToString();
        var (fieldExpression, patternExpression) = CreateCaseSensitiveExpressions(expression, patternString);

        var containsMethod = GetStringContainsMethod();
        return containsMethod != null
            ? Expression.Call(fieldExpression, containsMethod, patternExpression)
            : null;
    }

    /// <summary>
    /// Creates field and pattern expressions handling case sensitivity based on criteria configuration.
    /// </summary>
    /// <param name="expression">The expression parser containing field and criteria information</param>
    /// <param name="patternString">The pattern string to process</param>
    /// <returns>Tuple containing the field expression and pattern expression</returns>
    private static (Expression fieldExpression, Expression patternExpression) CreateCaseSensitiveExpressions(
        ExpressionParser expression, string patternString)
    {
        if (expression.Criteria.CaseSensitive)
        {
            return (expression.FieldToFilter, Expression.Constant(patternString, typeof(string)));
        }

        var upperFieldExpression = Expression.Call(expression.FieldToFilter, GetToUpperMethod());
        var upperPatternExpression = Expression.Constant(patternString.ToUpper(), typeof(string));

        return (upperFieldExpression, upperPatternExpression);
    }

    /// <summary>
    /// Gets the string ToUpper method for case-insensitive comparisons.
    /// </summary>
    /// <returns>The ToUpper method info</returns>
    private static MethodInfo GetToUpperMethod()
    {
        return typeof(string).GetMethods()
            .First(m => m.Name == MethodName.ToUpper.ToMethodString() && m.GetParameters().Length == 0);
    }

    /// <summary>
    /// Gets the string Contains method for pattern matching.
    /// </summary>
    /// <returns>The Contains method info</returns>
    private static MethodInfo GetStringContainsMethod()
    {
        return typeof(string).GetMethod(MethodName.Contains.ToMethodString(), new[] { typeof(string) });
    }

    /// <summary>
    /// Combines an existing OR expression with a new contains expression using OrElse.
    /// </summary>
    /// <param name="existingOrExpression">The existing OR expression (can be null)</param>
    /// <param name="newContainsExpression">The new contains expression to add</param>
    /// <returns>Combined OR expression or the new expression if existing was null</returns>
    private static Expression CombineWithOrExpression(Expression existingOrExpression, Expression newContainsExpression)
    {
        if (newContainsExpression == null)
            return existingOrExpression;

        return existingOrExpression == null
            ? newContainsExpression
            : Expression.OrElse(existingOrExpression, newContainsExpression);
    }
}
