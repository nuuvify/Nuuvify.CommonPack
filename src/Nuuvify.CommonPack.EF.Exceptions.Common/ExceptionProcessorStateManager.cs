using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace EntityFramework.Exceptions.Common;

public abstract class ExceptionProcessorStateManager<T> : StateManager where T : DbException
{
    protected internal enum DatabaseError
    {
        UniqueConstraint,
        CannotInsertNull,
        MaxLength,
        NumericOverflow,
        ReferenceConstraint,
        CustomException,
        CustomDbException,
        CustomDbUpdateException,
    }

    protected IDictionary<string, string> CustomErrors { get; set; }
    protected string CustomNewMessage { get; set; }

    protected ExceptionProcessorStateManager(StateManagerDependencies dependencies)
        : base(dependencies)
    {

    }

    ///<inheritdoc cref="SaveChangesAsync"/>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        try
        {
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }
        catch (DbUpdateException originalException)
        {
            var exception = GetException(originalException);

            if (exception != null)
            {
                throw exception;
            }

            throw;
        }
    }

    /// <summary>
    /// Para IBM DB2 até a versão 3.1.0.400 não é possivel usar SaveChangesAsync, 
    /// pois causa exception ao fazer Add, apesar de funcionar com Update e Delete, dessa
    /// forma utilize SaveChanges (sincrono) para DB2
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (DbUpdateException originalException)
        {
            var exception = GetException(originalException);

            if (exception != null)
            {
                throw exception;
            }

            throw;
        }
    }

    private Exception GetException(DbUpdateException ex)
    {
        if (ex.GetBaseException() is T dbException &&
            GetDatabaseError(dbException) is DatabaseError error)
        {
            var entries = ex.Entries.Select(entry =>
                base.GetOrCreateEntry(entry.Entity, entry.Metadata)).ToList();

            return ExceptionFactory.Create(error, ex, entries, CustomNewMessage, CustomErrors);
        }
        else if (typeof(T).Name.Equals("DB2Exception", StringComparison.OrdinalIgnoreCase) ||
                 typeof(T).Name.Equals("OracleException", StringComparison.OrdinalIgnoreCase))
        {
            var entries = ex.Entries.Select(entry =>
                base.GetOrCreateEntry(entry.Entity, entry.Metadata)).ToList();

            return ExceptionFactory.Create(DatabaseError.CustomDbUpdateException, ex, entries, CustomNewMessage, CustomErrors);
        }

        return ex;
    }

    protected abstract DatabaseError? GetDatabaseError(T dbException);
}

