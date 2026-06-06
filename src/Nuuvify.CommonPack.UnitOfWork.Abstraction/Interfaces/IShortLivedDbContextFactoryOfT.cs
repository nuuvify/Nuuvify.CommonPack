namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

/// <summary>
/// Defines an opt-in factory for creating short-lived DbContext instances with optional audit context.
/// </summary>
/// <typeparam name="TContext">The db context type handled by the factory.</typeparam>
public interface IShortLivedDbContextFactory<TContext>
    where TContext : class
{
    /// <summary>
    /// Creates a new independent short-lived db context instance.
    /// </summary>
    /// <param name="usernameContext">Optional username used for audit context.</param>
    /// <param name="userIdContext">Optional user identifier used for audit context.</param>
    /// <param name="cancellationToken">Cancellation token for context creation.</param>
    /// <returns>A new short-lived db context.</returns>
    Task<TContext> CreateAsync(
        string usernameContext = null,
        string userIdContext = null,
        CancellationToken cancellationToken = default);
}
