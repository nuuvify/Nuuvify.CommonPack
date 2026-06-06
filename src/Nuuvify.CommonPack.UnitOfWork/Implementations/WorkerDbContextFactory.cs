using Microsoft.EntityFrameworkCore;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

namespace Nuuvify.CommonPack.UnitOfWork;

/// <summary>
/// Creates short-lived db context instances for worker/background flows.
/// </summary>
/// <typeparam name="TContext">The db context type.</typeparam>
public class WorkerDbContextFactory<TContext> : IWorkerDbContextFactory<TContext>
    where TContext : DbContext
{
    private readonly IDbContextFactory<TContext> _dbContextFactory;

    /// <summary>
    /// Initializes a new instance of <see cref="WorkerDbContextFactory{TContext}"/>.
    /// </summary>
    /// <param name="dbContextFactory">Factory that creates short-lived db contexts.</param>
    public WorkerDbContextFactory(IDbContextFactory<TContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    /// <inheritdoc/>
    public virtual async Task<TContext> CreateAsync(
        string usernameContext = null,
        string userIdContext = null,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(usernameContext) || !string.IsNullOrWhiteSpace(userIdContext))
        {
            dbContext.SetDbContextUsername(usernameContext, userIdContext);
        }

        return dbContext;
    }
}
