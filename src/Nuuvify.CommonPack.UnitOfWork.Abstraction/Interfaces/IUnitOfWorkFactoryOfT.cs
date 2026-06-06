namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

/// <summary>
/// Defines an opt-in factory for creating short-lived <see cref="IUnitOfWork{TContext}"/> instances.
/// </summary>
/// <typeparam name="TContext">The db context type handled by the unit of work.</typeparam>
public interface IUnitOfWorkFactory<TContext> where TContext : class
{
    /// <summary>
    /// Creates a new independent <see cref="IUnitOfWork{TContext}"/> instance.
    /// </summary>
    /// <param name="usernameContext">Optional username used for audit context.</param>
    /// <param name="userIdContext">Optional user identifier used for audit context.</param>
    /// <param name="cancellationToken">Cancellation token for context creation.</param>
    /// <returns>A new short-lived unit of work.</returns>
    Task<IUnitOfWork<TContext>> CreateAsync(
        string usernameContext = null,
        string userIdContext = null,
        CancellationToken cancellationToken = default);
}
