
using Nuuvify.CommonPack.UnitOfWork.Abstraction.ValueObjects;

namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;


/// <summary>
/// Defines the interfaces for generic repository.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IRepository<TEntity> : IRepositoryReadOnly<TEntity> where TEntity : class
{

    /// <summary>
    /// Save changes to the AutoHistory table 
    /// (Important: You must have created the table in the database, and configured AutoHistory 
    /// in your DbContext class)
    /// <code>
    /// <example>
    ///     modelBuilder.EnableAutoHistory{AutoHistory}(o =>
    ///     {
    ///         o.ProviderName = Database.ProviderName;
    ///     });
    /// </example>
    /// </code>
    /// </summary>
    /// <param name="ensureAutoHistory">If configured, save changes to AutoHistory</param>
    /// <param name="actualRegistry">In a processing loop, pass the registry count, 
    /// if the result of (actualRegistry / limitCommit) is zero, that is, every time the 
    /// quantity established in limitCommit is processed, it will be implemented in the database.
    /// </param>
    /// <param name="limitCommit">Number of records to run Commit</param>
    /// <param name="toSave">Persist the data in the database</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> SaveChangesAsync(
        bool ensureAutoHistory = false,
        int actualRegistry = 1,
        int limitCommit = 1,
        bool toSave = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add the specified entity
    /// </summary>
    /// <param name="entity">The entities.</param>
    /// <param name="cancellationToken">to observe while waiting for the task to complete.</param>
    /// <returns></returns>
    Task<object> Add(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a range of entities asynchronously.
    /// </summary>
    /// <param name="entities">The entities to insert.</param>
    /// <param name="cancellationToken">to observe while waiting for the task to complete.</param>
    /// <returns></returns>
    Task Add(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the specified entity
    /// </summary>
    /// <param name="entity">The entities.</param>
    void Update(TEntity entity);

    /// <summary>
    /// Updates the specified entities.
    /// </summary>
    /// <param name="entities">The entities.</param>
    void Update(IEnumerable<TEntity> entities);

    /// <summary>
    /// Deletes the entity by the specified primary key.
    /// </summary>
    /// <param name="id">The primary key value.</param>
    /// <param name="createInstance">An instance of the object will be created and a find will be executed by the id parameter, if the record is found it will be marked as EntityState.Deleted</param>
    void Remove(object id, bool createInstance = false);

    /// <summary>
    /// Deletes the specified entity.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    void Remove(TEntity entity);

    /// <summary>
    /// Deletes the specified entities.
    /// </summary>
    /// <param name="entities">The entities.</param>
    void Remove(IEnumerable<TEntity> entities);

    /// <summary>
    /// Change entity state for patch method on web api.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="state">The entity state.</param>
    void ChangeEntityState(TEntity entity, EfEntityState state);
}

