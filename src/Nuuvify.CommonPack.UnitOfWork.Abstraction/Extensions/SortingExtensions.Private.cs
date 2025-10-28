using System.Linq.Expressions;
using System.Reflection;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Enums;

namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Extensions;

public static partial class SortingExtensions
{
    /// <summary>
    /// Determines the appropriate sort command based on whether it's a subsequent sort and sort direction.
    /// </summary>
    /// <param name="useThenBy">Indicates if this is a subsequent sort operation.</param>
    /// <param name="isDescending">Indicates if the sort should be in descending order.</param>
    /// <returns>The appropriate SortCommand enum value.</returns>
    private static SortCommand GetSortCommand(bool useThenBy, bool isDescending)
    {
        return (useThenBy, isDescending) switch
        {
            (false, false) => SortCommand.OrderBy,
            (false, true) => SortCommand.OrderByDescending,
            (true, false) => SortCommand.ThenBy,
            (true, true) => SortCommand.ThenByDescending
        };
    }

    private static IQueryable<TEntity> OrderBy<TEntity>(
        this IQueryable<TEntity> source,
        PropertyInfo propertyInfo,
        string command)
    {
        var type = typeof(TEntity);
        var parameter = Expression.Parameter(type, ExpressionParameterName.P.ToParameterString());

        dynamic propertyValue = parameter;
        if (propertyInfo.Name.Contains("."))
        {
            var parts = propertyInfo.Name.Split('.');
            for (var i = 0; i < parts.Length - 1; i++)
            {
                propertyValue = Expression.PropertyOrField(propertyValue, parts[i]);
            }
        }

        var propertyAccess = Expression.MakeMemberAccess(propertyValue, propertyInfo);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);
        var resultExpression = Expression.Call(typeof(Queryable), command, new[] { type, propertyInfo.PropertyType },
            source.Expression, Expression.Quote(orderByExpression));

        return source.Provider.CreateQuery<TEntity>(resultExpression);
    }

}
