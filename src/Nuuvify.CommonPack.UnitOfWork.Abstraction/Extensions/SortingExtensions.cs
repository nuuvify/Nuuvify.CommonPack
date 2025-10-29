using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Extensions;

/// <summary>
/// Provides extension methods for sorting <see cref="IQueryable{T}"/> collections.
/// </summary>
public static partial class SortingExtensions
{
    /// <summary>
    /// Sorts the queryable collection using the sort criteria from the provided model.
    /// </summary>
    /// <typeparam name="TEntity">The type of entities in the queryable collection.</typeparam>
    /// <typeparam name="TModel">The type of the model that implements <see cref="IQuerySort"/>.</typeparam>
    /// <param name="result">The queryable collection to sort.</param>
    /// <param name="fields">The model containing sort criteria.</param>
    /// <returns>A sorted <see cref="IQueryable{TEntity}"/>.</returns>
    public static IQueryable<TEntity> Sort<TEntity, TModel>(
        this IQueryable<TEntity> result,
        TModel fields) where TModel : IQuerySort
    {
        return Sort(result, fields.Sort);
    }

    /// <summary>
    /// Sorts the queryable collection using the specified sort fields string.
    /// </summary>
    /// <typeparam name="TEntity">The type of entities in the queryable collection.</typeparam>
    /// <param name="result">The queryable collection to sort.</param>
    /// <param name="fields">A string containing sort fields. Format: "field1,field2 desc,field3 asc".</param>
    /// <returns>A sorted <see cref="IQueryable{TEntity}"/>.</returns>
    /// <remarks>
    /// The sort fields string supports:
    /// - Multiple fields separated by commas
    /// - Direction indicators: "asc" or "desc" (default is ascending)
    /// - Prefix indicators: "A-" for ascending, "D-" for descending
    /// </remarks>
    public static IQueryable<TEntity> Sort<TEntity>(
        this IQueryable<TEntity> result,
        string fields)
    {
        if (string.IsNullOrWhiteSpace(fields)) return result;

        var useThenBy = false;
        foreach (var sortTerm in fields.Fields())
        {
            var property = TypeExtensions.GetProperty<TEntity>(sortTerm.FieldName());

            if (property != null)
            {
                var command = GetSortCommand(useThenBy, sortTerm.IsDescending());
                result = result.OrderBy(property, command.ToCommandString());
            }

            useThenBy = true;
        }

        return result;
    }
}
