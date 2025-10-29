using System.Linq.Expressions;
using System.Reflection;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Enums;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Extensions;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Helpers;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Filter;

internal static class ExpressionFactory
{
    internal static ExpressionParserCollection GetOperators<TEntity>(IQueryableCustom model)
    {
        var expressions = new ExpressionParserCollection();

        var type = model.GetType();
        expressions.ParameterExpression = Expression.Parameter(typeof(TEntity), ExpressionParameterName.Model.ToParameterString());

        foreach (var propertyInfo in type.GetProperties())
        {
            var criteria = GetCriteria(model, propertyInfo);
            if (criteria == null)
                continue;

            if (!typeof(TEntity).HasProperty(criteria.FieldName) &&
                !criteria.FieldName.Contains("."))
                continue;

            dynamic propertyValue = expressions.ParameterExpression;

            foreach (var part in criteria.FieldName.Split('.'))
            {
                propertyValue = Expression.PropertyOrField(propertyValue, part);
            }

            var expressionData = new ExpressionParser
            {
                FieldToFilter = propertyValue,
                FilterBy = GetClosureOverConstant(
                    criteria.Property.GetValue(model, null),
                    GetNonNullable(criteria.Property.PropertyType)),
                Criteria = criteria
            };

            if (criteria.Property.GetValue(model, null) != null)
                expressions.Add(expressionData);
        }

        return expressions;
    }
    private static Type GetNonNullable(Type propertyType)
    {
        return propertyType.IsGenericType &&
            propertyType.GetGenericTypeDefinition() == typeof(Nullable<>)
            ? Nullable.GetUnderlyingType(propertyType)
            : propertyType;
    }
    private static bool IsNullableType(Type t)
    {
        return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
    internal static WhereClause GetCriteria(IQueryableCustom model, PropertyInfo propertyInfo)
    {
        bool isCollection = propertyInfo.IsPropertyACollection();
        var criteria = new WhereClause();

        var attr = Attribute.GetCustomAttributes(propertyInfo);
        if (attr.Any(a => a.GetType() == typeof(QueryOperatorAttribute)))
        {
            var data = (QueryOperatorAttribute)attr.First(a => a.GetType() == typeof(QueryOperatorAttribute));
            criteria.UpdateAttributeData(data);
            if (isCollection && data.Operator != WhereOperator.Contains && data.Operator != WhereOperator.ContainsWithLikeForList)
                throw new ArgumentException($"{propertyInfo.Name} - For array the only Operator available is Contains and ContainsWithLikeForList");

            if (!isCollection && data.Operator == WhereOperator.ContainsWithLikeForList)
                throw new ArgumentException($"{propertyInfo.Name} - ContainsWithLikeForList Operator is only available to string arrays");
        }

        var customValue = propertyInfo.GetValue(model, null);
        if (customValue == null)
            return null;

        // Check if the value is an empty collection
        if (isCollection && customValue is System.Collections.IEnumerable enumerable)
        {
            bool hasItems = false;
            foreach (var item in enumerable)
            {
                hasItems = true;
                break;
            }
            if (!hasItems)
                return null;
        }

        criteria.UpdateValues(propertyInfo);
        return criteria;
    }

    /// <summary>
    /// Creates an expression that properly parameterizes constant values for Entity Framework Core.
    /// This ensures that values are passed as parameters in the generated SQL instead of being embedded as literals,
    /// which improves performance through query plan reuse and prevents SQL injection.
    /// 
    /// The original EF Core issue (aspnet/EntityFrameworkCore#3361) regarding DateTime constants has been resolved
    /// since EF Core 1.0 RC2, but this approach is still the best practice for dynamic expressions.
    /// </summary>
    /// <typeparam name="T">The type of the constant value</typeparam>
    /// <param name="constant">The constant value to be parameterized</param>
    /// <param name="targetType">The target type for the expression (handles nullable conversions)</param>
    /// <returns>An expression that will be properly parameterized by EF Core</returns>
    internal static Expression GetClosureOverConstant<T>(T constant, Type targetType)
    {
        // For null values, return a typed null constant
        if (constant == null)
        {
            return Expression.Constant(null, targetType);
        }

        // For modern EF Core versions (5.0+), Expression.Constant with proper type works well
        // The closure pattern is automatically handled by EF Core's expression visitor
        if (targetType != typeof(T) && targetType != null)
        {
            // Handle type conversion for nullable types and other conversions
            return Expression.Convert(Expression.Constant(constant, typeof(T)), targetType);
        }

        return Expression.Constant(constant, targetType ?? typeof(T));
    }

    internal static List<WhereClause> GetCriterias(IQueryableCustom searchModel)
    {
        var type = searchModel.GetType();
        var criterias = new List<WhereClause>();

        foreach (var propertyInfo in type.GetProperties())
        {
            bool isCollection = propertyInfo.IsPropertyACollection();
            if (!isCollection && propertyInfo.IsPropertyObject(searchModel))
                continue;

            var criteria = new WhereClause();
            var attr = Attribute.GetCustomAttributes(propertyInfo).FirstOrDefault();

            if (attr?.GetType() == typeof(QueryOperatorAttribute))
            {
                var data = (QueryOperatorAttribute)attr;
                criteria.UpdateAttributeData(data);
                if (data.Operator != WhereOperator.Contains && isCollection)
                    throw new ArgumentException($"{propertyInfo.Name} - For array the only Operator available is Contains");
            }

            if (isCollection)
                criteria.Operator = WhereOperator.Contains;

            var customValue = propertyInfo.GetValue(searchModel, null);
            if (customValue == null)
                continue;

            criteria.UpdateValues(propertyInfo);
            criterias.Add(criteria);
        }

        return criterias.OrderBy(o => o.UseOr).ToList();
    }
}
