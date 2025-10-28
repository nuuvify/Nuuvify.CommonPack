using System.Linq.Expressions;
using System.Reflection;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Enums;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Extensions;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Helpers;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Filter;

/// <summary>
/// Provides extension methods for filtering <see cref="IQueryable{T}"/> collections using dynamic expressions.
/// </summary>
public static class FiltersExtensions
{
    /// <summary>
    /// Applies dynamic filtering to an <see cref="IQueryable{TEntity}"/> based on the provided model's criteria.
    /// </summary>
    /// <typeparam name="TEntity">The type of entities in the queryable collection.</typeparam>
    /// <param name="result">The queryable collection to filter.</param>
    /// <param name="model">The model containing filter criteria with properties decorated with <see cref="QueryOperatorAttribute"/>.</param>
    /// <returns>A filtered <see cref="IQueryable{TEntity}"/>.</returns>
    /// <remarks>
    /// This method uses reflection to analyze the model's properties and their attributes to build dynamic WHERE clauses.
    /// Properties should be decorated with <see cref="QueryOperatorAttribute"/> to specify filter operators.
    /// </remarks>
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

    public static Expression<Func<TEntity, bool>> FilterExpression<TEntity>(
        this IQueryable<TEntity> result,
        IQueryableCustom model)
    {
        if (model == null) return null;
        Expression lastExpression = null;

        var operations = ExpressionFactory.GetOperators<TEntity>(model);
        foreach (var expression in operations.Ordered())
        {
            if (!expression.Criteria.CaseSensitive)
            {
                expression.FieldToFilter = Expression.Call(expression.FieldToFilter,
                    typeof(string).GetMethods()
                        .First(m => m.Name == MethodName.ToUpper.ToMethodString() && m.GetParameters().Length == 0));

                expression.FilterBy = Expression.Call(expression.FilterBy,
                    typeof(string).GetMethods()
                        .First(m => m.Name == MethodName.ToUpper.ToMethodString() && m.GetParameters().Length == 0));
            }

            var actualExpression = GetExpression<TEntity>(expression);

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
    /// Creates an expression for ContainsWithLikeForList operator that performs OR-based string contains operations.
    /// This method processes a collection of patterns and creates OR expressions for each pattern against the target field.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being filtered.</typeparam>
    /// <param name="expression">The expression parser containing field and filter information.</param>
    /// <returns>An OR-based expression that checks if the field contains any of the patterns in the list.</returns>
    private static Expression ContainsWithLikeForListExpression<TEntity>(ExpressionParser expression)
    {
        Expression orExpression = null;

        if (expression.FilterBy is ConstantExpression constantExpression)
        {
            // Handle different types of collections
            System.Collections.IEnumerable patterns = null;

            if (constantExpression.Value is System.Collections.IEnumerable enumerable)
            {
                patterns = enumerable;
            }
            else if (constantExpression.Value != null)
            {
                // If it's not an enumerable but has a value, try to convert it
                var valueType = constantExpression.Value.GetType();
                if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(List<>) && valueType.GetGenericArguments()[0] == typeof(string))
                {
                    patterns = (System.Collections.IEnumerable)constantExpression.Value;
                }
            }

            if (patterns != null)
            {
                foreach (var pattern in patterns)
                {
                    if (pattern == null || string.IsNullOrEmpty(pattern.ToString())) continue;

                    var patternString = pattern.ToString();
                    var likePattern = Expression.Constant(patternString, typeof(string));
                    var containsMethod = typeof(string).GetMethod(MethodName.Contains.ToMethodString(), new[] { typeof(string) });

                    if (containsMethod != null)
                    {
                        var containsExpression = Expression.Call(expression.FieldToFilter, containsMethod, likePattern);

                        if (orExpression == null)
                        {
                            orExpression = containsExpression;
                        }
                        else
                        {
                            orExpression = Expression.OrElse(orExpression, containsExpression);
                        }
                    }
                }
            }
        }

        return orExpression ?? Expression.Constant(false);
    }

}
