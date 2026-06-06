using Microsoft.EntityFrameworkCore;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

namespace Nuuvify.CommonPack.UnitOfWork;

/// <summary>
/// Creates short-lived <see cref="IUnitOfWork{TContext}"/> instances using <see cref="IDbContextFactory{TContext}"/>.
/// </summary>
/// <typeparam name="TContext">The db context type.</typeparam>
public sealed class UnitOfWorkFactory<TContext> : IUnitOfWorkFactory<TContext> where TContext : DbContext
{
    private readonly IDbContextFactory<TContext> _dbContextFactory;

    /// <summary>
    /// Initializes a new instance of <see cref="UnitOfWorkFactory{TContext}"/>.
    /// </summary>
    /// <param name="dbContextFactory">Factory that creates short-lived db contexts.</param>
    public UnitOfWorkFactory(IDbContextFactory<TContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    /// <inheritdoc/>
    public async Task<IUnitOfWork<TContext>> CreateAsync(
        string usernameContext = null,
        string userIdContext = null,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var unitOfWork = new UnitOfWork<TContext>(dbContext)
        {
            UsernameContext = usernameContext,
            UserIdContext = userIdContext
        };

        if (!string.IsNullOrWhiteSpace(usernameContext) || !string.IsNullOrWhiteSpace(userIdContext))
        {
            dbContext.SetDbContextUsername(usernameContext, userIdContext);
        }

        return unitOfWork;
    }
}
