using System.Linq.Expressions;
using System.Reflection;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Enums;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Extensions;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Helpers;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Filter;

/// <summary>
/// Factory class for creating dynamic LINQ expressions from query models with QueryOperator attributes.
/// Provides comprehensive support for both direct properties and navigation properties,
/// proper Entity Framework Core parameterization, and type-safe expression building.
/// Handles nullable types, collections, and complex filtering scenarios for dynamic query generation.
/// </summary>
internal static class ExpressionFactory
{
    /// <summary>
    /// Builds a collection of expression parsers from a query model for dynamic LINQ filtering.
    /// Analyzes each property with QueryOperator attributes, validates accessibility for both direct and navigation properties,
    /// and creates properly parameterized expressions for Entity Framework Core compatibility.
    /// </summary>
    /// <typeparam name="TEntity">The target entity type to filter against</typeparam>
    /// <param name="model">The query model containing filter criteria and values</param>
    /// <returns>Collection of expression parsers ready for dynamic LINQ query building</returns>
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

            if (!CanAccessProperty<TEntity>(criteria.FieldName))
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
    /// <summary>
    /// Gets the underlying type from a nullable type, or returns the original type if not nullable.
    /// Used to properly handle type conversions in expression building for both nullable and non-nullable types.
    /// </summary>
    /// <param name="propertyType">The type to analyze</param>
    /// <returns>The underlying non-nullable type, or the original type if not nullable</returns>
    private static Type GetNonNullable(Type propertyType)
    {
        return propertyType.IsGenericType &&
            propertyType.GetGenericTypeDefinition() == typeof(Nullable<>)
            ? Nullable.GetUnderlyingType(propertyType)
            : propertyType;
    }
    /// <summary>
    /// Validates if a property path can be accessed from the given entity type.
    /// Supports both direct properties and navigation properties (e.g., "AzEnvironment.Name").
    /// For simple properties, validates using the existing HasProperty method.
    /// For navigation properties, validates each part of the path and handles nullable types automatically.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to validate the property path against</typeparam>
    /// <param name="propertyPath">The property path to validate, can be simple (Name) or navigation (AzEnvironment.Name)</param>
    /// <returns>True if the property path is valid and accessible, false otherwise</returns>
    private static bool CanAccessProperty<TEntity>(string propertyPath)
    {
        if (string.IsNullOrWhiteSpace(propertyPath))
            return false;

        if (!propertyPath.Contains("."))
            return typeof(TEntity).HasProperty(propertyPath);

        var parts = propertyPath.Split('.');
        var currentType = typeof(TEntity);

        foreach (var part in parts)
        {
            var property = currentType.GetAllProperties()
                .FirstOrDefault(p => p.Name.Equals(part, StringComparison.OrdinalIgnoreCase));

            if (property == null)
                return false;

            currentType = property.PropertyType;

            if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                currentType = Nullable.GetUnderlyingType(currentType);
            }
        }

        return true;
    }
    /// <summary>
    /// Creates filtering criteria for a property based on its QueryOperator attributes and value.
    /// Validates operator compatibility with collection types and ensures proper attribute configuration.
    /// Returns null for properties with null values or empty collections to exclude them from filtering.
    /// </summary>
    /// <param name="model">The query model containing the property values</param>
    /// <param name="propertyInfo">The property information to analyze</param>
    /// <returns>WhereClause with filtering criteria, or null if property should be excluded</returns>
    /// <exception cref="ArgumentException">Thrown when operator is incompatible with property type</exception>
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

        if (isCollection && customValue is System.Collections.IEnumerable enumerable && !enumerable.Cast<object>().Any())
        {
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
    /// For null/default values, returns a typed null constant.
    /// For modern EF Core versions (5.0+), Expression.Constant with proper type works well as the closure pattern
    /// is automatically handled by EF Core's expression visitor.
    /// Handles type conversion for nullable types and other conversions when targetType differs from T.
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
        if (EqualityComparer<T>.Default.Equals(constant, default(T)))
        {
            return Expression.Constant(null, targetType);
        }

        if (targetType != typeof(T) && targetType != null)
        {
            return Expression.Convert(Expression.Constant(constant, typeof(T)), targetType);
        }

        return Expression.Constant(constant, targetType ?? typeof(T));
    }

    /// <summary>
    /// Extracts and validates filtering criteria from a search model.
    /// Processes properties with QueryOperator attributes, validates operator compatibility with collection types,
    /// and ensures proper configuration for filtering operations. Returns criteria ordered by UseOr flag.
    /// </summary>
    /// <param name="searchModel">The search model containing filter criteria</param>
    /// <returns>List of WhereClause objects representing valid filtering criteria, ordered by UseOr flag</returns>
    /// <exception cref="ArgumentException">Thrown when operator is incompatible with collection properties</exception>
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
