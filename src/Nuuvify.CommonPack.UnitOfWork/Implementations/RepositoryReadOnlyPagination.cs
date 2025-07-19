using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

namespace Nuuvify.CommonPack.UnitOfWork;

public partial class RepositoryReadOnly<TEntity> : IRepositoryReadOnly<TEntity> where TEntity : class
{

    ///<inheritdoc/>
    public virtual IPagedList<TEntity> GetPagedList(
        Expression<Func<TEntity, bool>> predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        int pageIndex = 0,
        int pageSize = 20,
        int indexFrom = 0,
        bool disableTracking = true,
        bool ignoreQueryFilters = false)
    {

        IQueryable<TEntity> query = _dbSet;

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (Includes?.Count > 0)
        {
            query = SetWithIncludes(query);
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        if (orderBy != null)
        {
            return orderBy(query).ToPagedList(pageIndex, pageSize, indexFrom);
        }
        else
        {
            return query.ToPagedList(pageIndex, pageSize, indexFrom);
        }
    }

    ///<inheritdoc/>
    public virtual IPagedList<TResult> GetPagedList<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>> predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        int pageIndex = 0,
        int pageSize = 20,
        int indexFrom = 0,
        bool disableTracking = true,
        bool ignoreQueryFilters = false) where TResult : class
    {
        IQueryable<TEntity> query = _dbSet;

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (Includes?.Count > 0)
        {
            query = SetWithIncludes(query);
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        if (orderBy != null)
        {
            return orderBy(query).Select(selector).ToPagedList(pageIndex, pageSize, indexFrom);
        }
        else
        {
            return query.Select(selector).ToPagedList(pageIndex, pageSize, indexFrom);
        }
    }

    ///<inheritdoc/>
    public virtual Task<IPagedList<TEntity>> GetPagedListAsync(
        Expression<Func<TEntity, bool>> predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        int pageIndex = 0,
        int pageSize = 20,
        int indexFrom = 0,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        CancellationToken cancellationToken = default)
    {

        IQueryable<TEntity> query = _dbSet;

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (Includes?.Count > 0)
        {
            query = SetWithIncludes(query);
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        if (orderBy != null)
        {
            return orderBy(query).ToPagedListAsync(pageIndex, pageSize, indexFrom, cancellationToken);
        }
        else
        {
            return query.ToPagedListAsync(pageIndex, pageSize, indexFrom, cancellationToken);
        }
    }

    ///<inheritdoc/>
    public virtual Task<IPagedList<TResult>> GetPagedListAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>> predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        int pageIndex = 0,
        int pageSize = 20,
        int indexFrom = 0,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        CancellationToken cancellationToken = default) where TResult : class
    {
        IQueryable<TEntity> query = _dbSet;

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (Includes?.Count > 0)
        {
            query = SetWithIncludes(query);
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (ignoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        if (orderBy != null)
        {
            return orderBy(query).Select(selector).ToPagedListAsync(pageIndex, pageSize, indexFrom, cancellationToken);
        }
        else
        {
            return query.Select(selector).ToPagedListAsync(pageIndex, pageSize, indexFrom, cancellationToken);
        }
    }

}

