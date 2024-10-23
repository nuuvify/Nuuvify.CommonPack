using System.Linq.Expressions;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Nuuvify.CommonPack.UnitOfWork;


public partial class RepositoryReadOnly<TEntity> : IRepositoryReadOnly<TEntity> where TEntity : class
{

    ///<inheritdoc/>
    public virtual IQueryable<TEntity> FromSql(string sql, params object[] parameters) => _dbSet.FromSqlRaw(sql, parameters);

    ///<inheritdoc/>
    public virtual TEntity Find(params object[] keyValues) => _dbSet.Find(keyValues);

    ///<inheritdoc/>
    public virtual Task<TEntity> FindAsync(params object[] keyValues) => _dbSet.FindAsync(keyValues).AsTask();

    ///<inheritdoc/>
    public virtual Task<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken) => _dbSet.FindAsync(keyValues, cancellationToken).AsTask();

    ///<inheritdoc/>
    public virtual int Count(Expression<Func<TEntity, bool>> predicate = null)
    {
        if (predicate == null)
        {
            return _dbSet.Count();
        }
        else
        {
            return _dbSet.Count(predicate);
        }
    }

    ///<inheritdoc/>
    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
        {
            return await _dbSet.CountAsync(cancellationToken);
        }
        else
        {
            return await _dbSet.CountAsync(predicate, cancellationToken);
        }
    }

    ///<inheritdoc/>
    public virtual long LongCount(Expression<Func<TEntity, bool>> predicate = null)
    {
        if (predicate == null)
        {
            return _dbSet.LongCount();
        }
        else
        {
            return _dbSet.LongCount(predicate);
        }
    }

    ///<inheritdoc/>
    public virtual async Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate = null, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
        {
            return await _dbSet.LongCountAsync(cancellationToken);
        }
        else
        {
            return await _dbSet.LongCountAsync(predicate, cancellationToken);
        }
    }

    ///<inheritdoc/>
    public virtual T Max<T>(Expression<Func<TEntity, bool>> predicate = null, Expression<Func<TEntity, T>> selector = null)
    {
        if (predicate == null)
            return _dbSet.Max(selector);
        else
            return _dbSet.Where(predicate).Max(selector);
    }

    ///<inheritdoc/>
    public virtual async Task<T> MaxAsync<T>(Expression<Func<TEntity, bool>> predicate = null, Expression<Func<TEntity, T>> selector = null, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            return await _dbSet.MaxAsync(selector, cancellationToken);
        else
            return await _dbSet.Where(predicate).MaxAsync(selector, cancellationToken);
    }

    ///<inheritdoc/>
    public virtual T Min<T>(Expression<Func<TEntity, bool>> predicate = null, Expression<Func<TEntity, T>> selector = null)
    {
        if (predicate == null)
            return _dbSet.Min(selector);
        else
            return _dbSet.Where(predicate).Min(selector);
    }

    ///<inheritdoc/>
    public virtual async Task<T> MinAsync<T>(Expression<Func<TEntity, bool>> predicate = null, Expression<Func<TEntity, T>> selector = null, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            return await _dbSet.MinAsync(selector, cancellationToken);
        else
            return await _dbSet.Where(predicate).MinAsync(selector, cancellationToken);
    }

    ///<inheritdoc/>
    public virtual decimal Average(Expression<Func<TEntity, bool>> predicate = null,
        Expression<Func<TEntity, decimal>> selector = null)
    {
        if (predicate == null)
            return _dbSet.Average(selector);
        else
            return _dbSet.Where(predicate).Average(selector);
    }

    ///<inheritdoc/>
    public virtual async Task<decimal> AverageAsync(Expression<Func<TEntity, bool>> predicate = null,
        Expression<Func<TEntity, decimal>> selector = null,
        CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            return await _dbSet.AverageAsync(selector);
        else
            return await _dbSet.Where(predicate).AverageAsync(selector);
    }

    ///<inheritdoc/>
    public virtual decimal Sum(Expression<Func<TEntity, bool>> predicate = null, Expression<Func<TEntity, decimal>> selector = null)
    {
        if (predicate == null)
            return _dbSet.Sum(selector);
        else
            return _dbSet.Where(predicate).Sum(selector);
    }

    ///<inheritdoc/>
    public virtual async Task<decimal> SumAsync(Expression<Func<TEntity, bool>> predicate = null,
        Expression<Func<TEntity, decimal>> selector = null,
        CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            return await _dbSet.SumAsync(selector, cancellationToken);
        else
            return await _dbSet.Where(predicate).SumAsync(selector, cancellationToken);
    }

    ///<inheritdoc/>
    public bool Exists(Expression<Func<TEntity, bool>> predicate = null)
    {
        if (predicate == null)
        {
            return _dbSet.Any();
        }
        else
        {
            return _dbSet.Any(predicate);
        }
    }

    ///<inheritdoc/>
    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate = null,
        CancellationToken cancellationToken = default)
    {
        if (predicate == null)
        {
            return await _dbSet.AnyAsync(cancellationToken);
        }
        else
        {
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }
    }


}

