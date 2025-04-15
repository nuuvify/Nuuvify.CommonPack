
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;
/// <summary>
/// Defines the interface(s) for unit of work.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <value>Get or Set user current context</value>
    string UsernameContext { get; set; }
    /// <value>Get or Set unique user identification current context</value>
    string UserIdContext { get; set; }

    /// <summary>
    /// Asynchronously saves all changes made in this unit of work to the database.
    /// </summary>
    /// <param name="ensureAutoHistory">If configured, save changes to AutoHistory</param>
    /// <param name="actualRegistry">In a processing loop, pass the registry count, 
    /// if the result of:
    /// <code>
    ///      Math.DivRem(actualRegistry, limitCommit, out int resto);
    ///      if (resto == 0)
    /// </code> 
    /// resto is zero, that is, every time the quantity established in limitCommit is processed, 
    /// it will be implemented in the database.
    /// </param>
    /// <param name="limitCommit">Number of records to run Commit</param>
    /// <param name="toSave">Persist the data in the database</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous save operation. The task result contains the number of state entities written to database.</returns>
    Task<int> SaveChangesAsync(bool ensureAutoHistory = false, int actualRegistry = 1, int limitCommit = 1, bool toSave = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the specified SQL command with the ExecuteSqlRaw method 
    /// </summary>
    /// <param name="sql">The raw SQL.</param>
    /// <param name="parameters">The parameters.</param>
    /// <returns>The number of state entities written to database.</returns>
    int ExecuteSqlCommand(string sql, params object[] parameters);

    /// <summary>
    /// Uses raw SQL queries to fetch the specified <typeparamref name="TEntity"/> data.
    /// Use the FromSqlRaw
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="sql">The raw SQL.</param>
    /// <param name="parameters">The parameters.</param>
    /// <returns>An <see cref="IQueryable{T}"/> that contains elements that satisfy the condition specified by raw SQL.</returns>
    IQueryable<TEntity> FromSql<TEntity>(string sql, params object[] parameters) where TEntity : class;

}
