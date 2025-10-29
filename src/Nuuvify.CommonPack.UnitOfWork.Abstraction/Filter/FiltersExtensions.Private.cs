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

    private static Expression ContainsWithLikeForListExpression<TEntity>(ExpressionParser expression)
    {
        Expression orExpression = null;

        Expression filterExpression = expression.FilterBy;

        if (filterExpression is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
        {
            filterExpression = unaryExpression.Operand;
        }

        if (filterExpression is not ConstantExpression constantExpression)
        {
            return null;
        }

        if (constantExpression.Value is not System.Collections.IEnumerable patterns)
        {
            return null;
        }

        foreach (var pattern in patterns)
        {
            if (pattern == null || string.IsNullOrEmpty(pattern.ToString()))
                continue;

            var patternString = pattern.ToString();

            Expression likePattern;
            Expression fieldToFilter = expression.FieldToFilter;

            if (!expression.Criteria.CaseSensitive)
            {
                fieldToFilter = Expression.Call(expression.FieldToFilter,
                    typeof(string).GetMethods()
                        .First(m => m.Name == MethodName.ToUpper.ToMethodString() && m.GetParameters().Length == 0));

                likePattern = Expression.Constant(patternString.ToUpper(), typeof(string));
            }
            else
            {
                likePattern = Expression.Constant(patternString, typeof(string));
            }

            var containsMethod = typeof(string).GetMethod(MethodName.Contains.ToMethodString(), new[] { typeof(string) });

            if (containsMethod != null)
            {
                var containsExpression = Expression.Call(fieldToFilter, containsMethod, likePattern);

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

        return orExpression;
    }
}
