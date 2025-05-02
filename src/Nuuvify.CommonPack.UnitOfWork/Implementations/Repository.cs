using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.ValueObjects;

namespace Nuuvify.CommonPack.UnitOfWork;

/// <summary>
/// Represents a default generic repository implements the <see cref="IRepository{TEntity}"/> interface.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public class Repository<TEntity> : RepositoryReadOnly<TEntity>, IRepository<TEntity> where TEntity : class
{
    protected readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="unitOfWork"></param>
    public Repository(DbContext dbContext, IUnitOfWork unitOfWork)
        : base(dbContext)
    {

        _unitOfWork = unitOfWork;
    }

    ///<inheritdoc/>
    public virtual async Task<object> Add(TEntity entity, CancellationToken cancellationToken = default)
    {
        var taskReturn = await _dbSet.AddAsync(entity, cancellationToken);

        return taskReturn;
    }

    ///<inheritdoc/>
    public virtual async Task Add(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    ///<inheritdoc/>
    public virtual void Update(TEntity entity)
    {
        _ = _dbSet.Update(entity);
    }

    ///<inheritdoc/>
    public virtual void Update(IEnumerable<TEntity> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    ///<inheritdoc/>
    public virtual void Remove(TEntity entity)
    {
        _ = _dbSet.Remove(entity);
    }

    ///<inheritdoc/>
    public virtual void Remove(object id, bool createInstance = false)
    {
        if (createInstance)
        {
            var typeInfo = typeof(TEntity).GetTypeInfo();
            var key = _dbContext.Model.FindEntityType(typeInfo)
                                      .FindPrimaryKey().Properties[0];

            var property = typeInfo.GetProperty(key?.Name);
            if (property != null)
            {
                var entity = Activator.CreateInstance(typeInfo, true);
                property.SetValue(entity, id);
                _dbContext.Entry(entity).State = EntityState.Deleted;
            }
            else
            {
                var entity = FindAsync(keyValues: id).Result;
                if (entity != null)
                {
                    Remove(entity);
                }
            }
        }
        else
        {
            var entity = FindAsync(keyValues: id).Result;
            if (entity != null)
            {
                Remove(entity);
            }
        }
    }

    ///<inheritdoc/>
    public virtual void Remove(IEnumerable<TEntity> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    ///<inheritdoc/>
    public virtual async Task<int> SaveChangesAsync(bool ensureAutoHistory = false, int actualRegistry = 1, int limitCommit = 1, bool toSave = true, CancellationToken cancellationToken = default)
    {
        _unitOfWork.UsernameContext = _dbContext.GetDbContextUsername() ?? _unitOfWork.UsernameContext;
        _unitOfWork.UserIdContext = _dbContext.GetDbContextUserId() ?? _unitOfWork.UserIdContext;
        _dbContext.SetDbContextUsername(_unitOfWork.UsernameContext, _unitOfWork.UserIdContext);

        return await _unitOfWork.SaveChangesAsync(ensureAutoHistory, actualRegistry, limitCommit, toSave, cancellationToken);
    }

    ///<inheritdoc/>
    public void ChangeEntityState(TEntity entity, EfEntityState state)
    {
        _dbContext.Entry(entity).State = (EntityState)state;
    }

}

