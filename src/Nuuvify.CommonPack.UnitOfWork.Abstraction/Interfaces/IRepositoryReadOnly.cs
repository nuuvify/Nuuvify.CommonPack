using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Nuuvify.CommonPack.Extensions.Interfaces;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;


/// <summary>
/// Example use:
/// <example>
/// <code>
///     var projection = _repository.GetFirstOrDefault(selector: b => new MyClassQueryResult { Name = b.Title, Link = b.Url }, predicate: x => x.Title.Contains(term));
///
///     var list = _repository.GetAllAsync(include: source => source.Include(blog => blog.Posts).ThenInclude(post => post.Comments));
/// PageList:
///     var items = _repository.GetPagedList(selector: b => MyClassQueryResult new { Name = b.Title, Link = b.Url });
/// or
///     return await _repository.GetPagedListAsync(pageIndex: pageIndex, pageSize: pageSize);
/// OrderBy:
///     var order = _repository.GetFirstOrDefault(predicate: x => x.Title.Contains(term), orderBy: source => source.OrderByDescending(b => b.Id).ThenBy(b => b.Name));
/// </code>
/// </example>
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IRepositoryReadOnly<TEntity> : IRepositoryValidation where TEntity : class
{

    IList<NotificationR> GetNotifications();


    /// <summary>
    /// Specifies the names of the navigation properties which should be eagerly loaded only in the subsequent query results.
    /// </summary>
    /// <param name="navigationProperties">The navigation properties to be eagerly loaded in the query results.</param>
    /// <returns>An <see cref="IRepositoryReadOnly{TEntity}"/> containing the navigation properties specified in ths method and in the Includes property.</returns>
    IRepositoryReadOnly<TEntity> Include(params string[] navigationProperties);



    /// <summary>
    /// Gets the <see cref="IPagedList{TResult}"/> based on a predicate, orderby delegate and page information. This method default no-tracking query.
    /// <example>
    /// var items = _repository.GetPagedList(selector: b => new MyClassQueryResult { Name = b.Title, Link = b.Url });
    /// </example>
    /// </summary>
    /// <remarks>This method default no-tracking query.</remarks>
    /// <param name="selector">The selector for projection.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="orderBy">A function to order elements.</param>
    /// <param name="pageIndex">The index of page.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="indexFrom">The start index value.</param>
    /// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
    /// <param name="ignoreQueryFilters">Ignore query filters</param>
    /// <returns>An <see cref="IPagedList{TResult}"/> that contains elements that satisfy the condition specified by <paramref name="predicate"/>.</returns>
    IPagedList<TResult> GetPagedList<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>> predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        int pageIndex = 0,
        int pageSize = 20,
        int indexFrom = 0,
        bool disableTracking = true,
        bool ignoreQueryFilters = false) where TResult : class;


    /// <summary>
    /// Gets the <see cref="IPagedList{TEntity}"/> based on a predicate, orderby delegate and page information. This method default no-tracking query.
    /// </summary>
    /// <remarks>This method default no-tracking query.</remarks>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="orderBy">A function to order elements.</param>
    /// <param name="pageIndex">The index of page.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="indexFrom">The start index value.</param>
    /// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
    /// <param name="ignoreQueryFilters">Ignore query filters</param>
    /// <returns>An <see cref="IPagedList{TEntity}"/> that contains elements that satisfy the condition specified by <paramref name="predicate"/>.</returns>
    IPagedList<TEntity> GetPagedList(
        Expression<Func<TEntity, bool>> predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        int pageIndex = 0,
        int pageSize = 20,
        int indexFrom = 0,
        bool disableTracking = true,
        bool ignoreQueryFilters = false);

    /// <summary>
    /// Gets the <see cref="IPagedList{TEntity}"/> based on a predicate, orderby delegate and page information. This method default no-tracking query.
    /// <example>
    /// var items = _repository.GetPagedListAsync(selector: b => new MyClassQueryResult { Name = b.Title, Link = b.Url });
    /// </example>
    /// </summary>
    /// <remarks>This method default no-tracking query.</remarks>
    /// <param name="selector">The selector for projection.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="orderBy">A function to order elements.</param>
    /// <param name="pageIndex">The index of page.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="indexFrom">The start index value.</param>
    /// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
    /// <param name="ignoreQueryFilters">Ignore query filters</param>
    /// <param name="cancellationToken">to observe while waiting for the task to complete. </param>
    /// <returns>An <see cref="IPagedList{TEntity}"/> that contains elements that satisfy the condition specified by <paramref name="predicate"/>.</returns>
    Task<IPagedList<TResult>> GetPagedListAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>> predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        int pageIndex = 0,
        int pageSize = 20,
        int indexFrom = 0,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        CancellationToken cancellationToken = default) where TResult : class;

    /// <summary>
    /// Gets the <see cref="IPagedList{TEntity}"/> based on a predicate, orderby delegate and page information. This method default no-tracking query.
    /// </summary>
    /// <remarks>This method default no-tracking query.</remarks>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="orderBy">A function to order elements.</param>
    /// <param name="pageIndex">The index of page.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="indexFrom">The start index value.</param>
    /// <param name="disableTracking"><c>True</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
    /// <param name="ignoreQueryFilters">Ignore query filters</param>
    /// <param name="cancellationToken">to observe while waiting for the task to complete. </param>
    /// <returns>An <see cref="IPagedList{TEntity}"/> that contains elements that satisfy the condition specified by <paramref name="predicate"/>.</returns>
    Task<IPagedList<TEntity>> GetPagedListAsync(
        Expression<Func<TEntity, bool>> predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        int pageIndex = 0,
        int pageSize = 20,
        int indexFrom = 0,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first or default entity based on a predicate, orderby delegate and include delegate. This method defaults to a read-only, no-tracking query.
    /// </summary>
    /// <remarks>This method defaults to a read-only, no-tracking query.</remarks>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="orderBy">A function to order elements.</param>
    /// <param name="disableTracking"><c>true</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
    /// <param name="ignoreQueryFilters">Ignore query filters</param>
    TEntity GetFirstOrDefault(
        Expression<Func<TEntity, bool>> predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false);

    /// <summary>
    /// Gets the first or default entity based on a predicate, orderby delegate and include delegate. This method defaults to a read-only, no-tracking query.
    /// <example>
    /// var items = _repository.GetFirstOrDefault(selector: b => new MyClassQueryResult { Name = b.Title, Link = b.Url });
    /// </example>
    /// </summary>
    /// <remarks>This method defaults to a read-only, no-tracking query.</remarks>
    /// <param name="selector">The selector for projection.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="orderBy">A function to order elements.</param>
    /// <param name="disableTracking"><c>true</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
    /// <param name="ignoreQueryFilters">Ignore query filters</param>
    TResult GetFirstOrDefault<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>> predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false);

    /// <summary>
    /// Gets the first or default entity based on a predicate, orderby delegate and include delegate. This method defaults to a read-only, no-tracking query.
    /// <example>
    /// var items = _repository.GetFirstOrDefaultAsync(selector: b => new MyClassQueryResult { Name = b.Title, Link = b.Url });
    /// </example>
    /// </summary>
    /// <remarks>Ex: This method defaults to a read-only, no-tracking query.</remarks>
    /// <param name="selector">The selector for projection.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="orderBy">A function to order elements.</param>
    /// <param name="disableTracking"><c>true</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
    /// <param name="ignoreQueryFilters">Ignore query filters</param>
    /// <param name="cancellationToken"></param>
    Task<TResult> GetFirstOrDefaultAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>> predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first or default entity based on a predicate, orderby delegate and include delegate. This method defaults to a read-only, no-tracking query.
    /// </summary>
    /// <remarks>Ex: This method defaults to a read-only, no-tracking query. </remarks>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="orderBy">A function to order elements.</param>
    /// <param name="disableTracking"><c>true</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
    /// <param name="ignoreQueryFilters">Ignore query filters</param>
    /// <param name="cancellationToken"></param>
    Task<TEntity> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uses raw SQL queries to fetch the specified <typeparamref name="TEntity" /> data.
    /// </summary>
    /// <param name="sql">The raw SQL.</param>
    /// <param name="parameters">The parameters.</param>
    /// <returns>An <see cref="IQueryable{TEntity}" /> that contains elements that satisfy the condition specified by raw SQL.</returns>
    IQueryable<TEntity> FromSql(string sql, params object[] parameters);

    /// <summary>
    /// Finds an entity with the given primary key values. If found, is attached to the context and returned. If no entity is found, then null is returned.
    /// </summary>
    /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
    /// <returns>The found entity or null.</returns>
    TEntity Find(params object[] keyValues);

    /// <summary>
    /// Finds an entity with the given primary key values. If found, is attached to the context and returned. If no entity is found, then null is returned.
    /// </summary>
    /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
    /// <returns>A <see cref="Task{TEntity}"/> that represents the asynchronous find operation. The task result contains the found entity or null.</returns>
    Task<TEntity> FindAsync(params object[] keyValues);

    /// <summary>
    /// Finds an entity with the given primary key values. If found, is attached to the context and returned. If no entity is found, then null is returned.
    /// </summary>
    /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
    /// <param name="cancellationToken">to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TEntity}"/> that represents the asynchronous find operation. The task result contains the found entity or null.</returns>
    Task<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken);


    /// <summary>
    /// Gets all entities. This method is not recommended, use GetAll With predicate:
    /// </summary>
    /// <returns>The <see cref="IQueryable{TEntity}"/>.</returns>
    IQueryable<TEntity> GetAll();

    /// <summary>
    /// Gets all entities based on a predicate.
    /// </summary>
    /// <remarks>Ex: This method defaults to a read-only, no-tracking query.</remarks>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="orderBy">A function to order elements.</param>
    /// <param name="disableTracking"><c>true</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
    /// <param name="ignoreQueryFilters">Ignore query filters</param>
    /// <param name="skip">Number of records that the query should skip</param>
    /// <param name="take">Number of records that the query should return, If this parameter is 0, skip, take will be ignored</param>
    IQueryable<TEntity> GetAll(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        int skip = 0,
        int take = 25);



    /// <summary>
    /// Gets all entities based on a predicate, with only the properties you choose with selector 
    /// <example>
    /// var items = _repository.GetAllAsync(selector: b => new MyClassQueryResult { Name = b.Title, Link = b.Url });
    /// </example>
    /// </summary>
    /// <remarks>Ex: This method defaults to a read-only, no-tracking</remarks>
    /// <param name="selector">Select the properties you want to get as a return.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="orderBy">A function to order elements.</param>
    /// <param name="disableTracking"><c>true</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
    /// <param name="ignoreQueryFilters">Ignore query filters</param>
    /// <param name="skip">Number of records that the query should skip</param>
    /// <param name="take">Number of records that the query should return, If this parameter is 0, skip, take will be ignored</param>
    /// <param name="cancellationToken"></param>
    Task<IList<TResult>> GetAllAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        int skip = 0,
        int take = 25,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities based on a predicate.
    /// </summary>
    /// <remarks>Ex: This method defaults to a read-only, no-tracking query.</remarks>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="orderBy">A function to order elements.</param>
    /// <param name="disableTracking"><c>true</c> to disable changing tracking; otherwise, <c>false</c>. Default to <c>true</c>.</param>
    /// <param name="ignoreQueryFilters">Ignore query filters</param>
    /// <param name="skip">Number of records that the query should skip</param>
    /// <param name="take">Number of records that the query should return, If this parameter is 0, skip, take will be ignored</param>
    /// <param name="cancellationToken"></param>
    Task<IList<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        int skip = 0,
        int take = 25,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities. This method is not recommended, use GetAll With predicate:
    /// </summary>
    Task<IList<TEntity>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the count based on a predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    int Count(Expression<Func<TEntity, bool>> predicate = null);

    /// <summary>
    /// Gets async the count based on a predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the long count based on a predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    long LongCount(Expression<Func<TEntity, bool>> predicate = null);

    /// <summary>
    /// Gets async the long count based on a predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the max based on a predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="selector"></param>
    /// <returns>decimal</returns>
    T Max<T>(Expression<Func<TEntity, bool>> predicate = null,
        Expression<Func<TEntity, T>> selector = null);

    /// <summary>
    /// Gets the async max based on a predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="selector"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>decimal</returns>
    Task<T> MaxAsync<T>(Expression<Func<TEntity, bool>> predicate = null,
        Expression<Func<TEntity, T>> selector = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the min based on a predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="selector"></param>
    /// <returns>decimal</returns>
    T Min<T>(Expression<Func<TEntity, bool>> predicate = null,
        Expression<Func<TEntity, T>> selector = null);

    /// <summary>
    /// Gets the async min based on a predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="selector"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>decimal</returns>
    Task<T> MinAsync<T>(Expression<Func<TEntity, bool>> predicate = null,
        Expression<Func<TEntity, T>> selector = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the average based on a predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="selector"></param>
    /// <returns>decimal</returns>
    decimal Average(Expression<Func<TEntity, bool>> predicate = null,
        Expression<Func<TEntity, decimal>> selector = null);

    /// <summary>
    /// Gets the async average based on a predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="selector"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>decimal</returns>
    Task<decimal> AverageAsync(Expression<Func<TEntity, bool>> predicate = null,
        Expression<Func<TEntity, decimal>> selector = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the sum based on a predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="selector"></param>
    /// <returns>decimal</returns>
    decimal Sum(Expression<Func<TEntity, bool>> predicate = null,
        Expression<Func<TEntity, decimal>> selector = null);

    /// <summary>
    /// Gets the async sum based on a predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="selector"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>decimal</returns>
    Task<decimal> SumAsync(Expression<Func<TEntity, bool>> predicate = null,
        Expression<Func<TEntity, decimal>> selector = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the Exists record based on a predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    bool Exists(Expression<Func<TEntity, bool>> predicate = null);
    /// <summary>
    /// Gets the Async Exists record based on a predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate = null,
        CancellationToken cancellationToken = default);


}


