namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

/// <summary>
/// Defines an opt-in factory for creating short-lived db context instances for worker/background flows.
/// </summary>
/// <typeparam name="TContext">The db context type handled by the factory.</typeparam>
public interface IWorkerDbContextFactory<TContext> : IShortLivedDbContextFactory<TContext>
    where TContext : class
{
}
