using Nuuvify.CommonPack.UnitOfWork.Abstraction.Filter;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IQueryable{T}"/> to apply filtering, sorting, and pagination.
/// </summary>
public static class IQueryableExtensions
{
    /// <summary>
    /// Applies filtering, sorting, and pagination to an <see cref="IQueryable{TEntity}"/> based on the provided model.
    /// </summary>
    /// <typeparam name="TEntity">The type of entities in the queryable collection.</typeparam>
    /// <param name="result">The queryable collection to apply operations to.</param>
    /// <param name="model">The model containing filter, sort, and pagination criteria.</param>
    /// <returns>A modified <see cref="IQueryable{TEntity}"/> with applied operations.</returns>
    /// <remarks>
    /// This method applies operations in the following order:
    /// 1. Filtering (if the model implements filtering criteria)
    /// 2. Sorting (if the model implements <see cref="IQuerySort"/>)
    /// 3. Pagination (if the model implements <see cref="IPagedList{TEntity}"/>)
    /// </remarks>
    public static IQueryable<TEntity> Apply<TEntity>(
        this IQueryable<TEntity> result,
        IQueryableCustom model)
    {
        result = result.Filter(model);

        if (model is IQuerySort sort)
            result = result.Sort(sort);

        if (model is IPagedList<TEntity> pagination)
            result = result.Skip(pagination.Skip).Take(pagination.Take);

        return result;
    }
}
