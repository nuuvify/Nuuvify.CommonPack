using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Nuuvify.CommonPack.Extensions.Notificator;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

namespace Nuuvify.CommonPack.UnitOfWork;

/// <summary>
/// Represents a default generic repository implements the <see cref="IRepository{TEntity}"/> interface.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public partial class RepositoryReadOnly<TEntity> : NotifiableR, IRepositoryReadOnly<TEntity> where TEntity : class
{

    protected readonly DbContext _dbContext;
    protected readonly DbSet<TEntity> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryReadOnly{TEntity}"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public RepositoryReadOnly(DbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbSet = _dbContext.Set<TEntity>();

        Includes = new List<string>();
    }

    private IQueryable<TEntity> SetWithIncludes(IQueryable<TEntity> query)
    {
        foreach (var include in Includes)
        {
            query = query.Include(include);
        }

        Includes = new List<string>();

        return query;
    }
    private IList<string> Includes { get; set; }

    public IRepositoryReadOnly<TEntity> Include(params string[] navigationProperties)
    {
        foreach (string navigationProperty in navigationProperties)
        {
            Includes.Add(navigationProperty);
        }

        return this;
    }

    ///<inheritdoc/>
    public async Task<IList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    ///<inheritdoc/>
    public async Task<IList<TResult>> GetAllAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        int skip = 0,
        int take = 25,
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

        if (take > 0)
        {
            query = query.Skip(skip).Take(take);
        }

        if (orderBy != null)
        {
            return await orderBy(query).Select(selector).ToArrayAsync(cancellationToken);
        }
        else
        {
            return await query.Select(selector).ToArrayAsync(cancellationToken);
        }

    }

    ///<inheritdoc/>
    public async Task<IList<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        int skip = 0,
        int take = 25,
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
            query = orderBy(query);
        }

        if (take > 0)
        {
            query = query.Skip(skip).Take(take);
        }
        return await query.ToListAsync(cancellationToken);

    }

    ///<inheritdoc/>
    public IQueryable<TEntity> GetAll()
    {
        return _dbSet;
    }

    ///<inheritdoc/>
    public IQueryable<TEntity> GetAll(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        bool disableTracking = true,
        bool ignoreQueryFilters = false,
        int skip = 0,
        int take = 25)
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
            query = orderBy(query);
        }

        if (take > 0)
        {
            query = query.Skip(skip).Take(take);
        }

        return query;

    }

    ///<inheritdoc/>
    public virtual TResult GetFirstOrDefault<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>> predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
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
            return orderBy(query).Select(selector).FirstOrDefault();
        }
        else
        {
            return query.Select(selector).FirstOrDefault();
        }
    }

    ///<inheritdoc/>
    public virtual TEntity GetFirstOrDefault(
        Expression<Func<TEntity, bool>> predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
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
            return orderBy(query).FirstOrDefault();
        }
        else
        {
            return query.FirstOrDefault();
        }
    }

    /// <inheritdoc />
    public virtual async Task<TEntity> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
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
            return await orderBy(query).FirstOrDefaultAsync(cancellationToken);
        }
        else
        {
            return await query.FirstOrDefaultAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public virtual async Task<TResult> GetFirstOrDefaultAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        Expression<Func<TEntity, bool>> predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
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
            return await orderBy(query).Select(selector).FirstOrDefaultAsync(cancellationToken);
        }
        else
        {
            return await query.Select(selector).FirstOrDefaultAsync(cancellationToken);
        }
    }

    public virtual IList<NotificationR> GetNotifications()
    {
        var notifications = Notifications.ToList();

        return notifications;
    }

}
