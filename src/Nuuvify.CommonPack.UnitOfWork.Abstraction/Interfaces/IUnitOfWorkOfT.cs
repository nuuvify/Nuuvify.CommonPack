using System.Threading;
using System.Threading.Tasks;

namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces
{
    /// <summary>
    /// Defines the interface(s) for generic unit of work.
    /// </summary>
    public interface IUnitOfWork<TContext> : IUnitOfWork where TContext : class
    {

        /// <value>Gets the db context.</value>
        /// <returns>The instance of type <typeparamref name="TContext"/>.</returns>
        TContext DbContext { get; }

        /// <summary>
        /// Saves all changes made in this context to the database with distributed transaction.
        /// if the result of:
        /// <example>
        /// <code>
        ///      Math.DivRem(actualRegistry, limitCommit, out int resto);
        ///      if (resto == 0)
        /// </code> 
        /// </example>
        /// resto is zero, that is, every time the quantity established in limitCommit is processed, 
        /// it will be implemented in the database.
        /// </summary>
        /// <param name="ensureAutoHistory">If configured, save changes to AutoHistory</param>
        /// <param name="actualRegistry">In a processing loop, pass the registry count</param>
        /// <param name="limitCommit">Number of records to run Commit</param>
        /// <param name="toSave">Persist the data in the database</param>
        /// <param name="cancellationToken"></param>
        /// <param name="unitOfWorks">An optional <see cref="IUnitOfWork"/> array.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous save operation. The task result contains the number of state entities written to database.</returns>
        Task<int> SaveChangesAsync(bool ensureAutoHistory = false, int actualRegistry = 1, int limitCommit = 1, bool toSave = true, CancellationToken cancellationToken = default, params IUnitOfWork[] unitOfWorks);

    }
}
