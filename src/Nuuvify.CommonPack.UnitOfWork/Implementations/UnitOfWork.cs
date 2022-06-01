using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Nuuvify.CommonPack.AutoHistory.Extensions;

namespace Nuuvify.CommonPack.UnitOfWork
{
    /// <summary>
    /// Represents the default implementation of the <see cref="IUnitOfWork"/> and <see cref="IUnitOfWork{TContext}"/> interface.
    /// </summary>
    /// <typeparam name="TContext">The type of the db context.</typeparam>
    public class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : DbContext
    {
        private bool IsDisposed { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork{TContext}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public UnitOfWork(TContext context)
        {
            DbContext = context ?? throw new ArgumentNullException(nameof(context));

            var userAudit = UsernameContext ?? DbContext.GetDbContextUsername();
            var userIdAudit = UserIdContext ?? DbContext.GetDbContextUserId();
            DbContext.SetDbContextUsername(userAudit, userIdAudit);
        }

        ///<inheritdoc/>
        public TContext DbContext { get; }

        ///<inheritdoc/>
        public virtual string UsernameContext { get; set; }
        ///<inheritdoc/>
        public virtual string UserIdContext { get; set; }

        ///<inheritdoc/>
        public int ExecuteSqlCommand(string sql, params object[] parameters)
        {
            return DbContext.Database.ExecuteSqlRaw(sql, parameters);
        }

        ///<inheritdoc/>
        public IQueryable<TEntity> FromSql<TEntity>(string sql, params object[] parameters) where TEntity : class
        {
            return DbContext.Set<TEntity>().FromSqlRaw(sql, parameters);
        }

        ///<inheritdoc/>
        public virtual async Task<int> SaveChangesAsync(bool ensureAutoHistory = false, int actualRegistry = 1, int limitCommit = 1, bool toSave = true)
        {

            if (ensureAutoHistory)
            {
                DbContext.EnsureAutoHistory(ensureAutoHistory: ensureAutoHistory, usernameContext: UsernameContext, toSave: toSave);
            }


            if (toSave)
            {
                CheckDisposed();
                if (CommitAsync(actualRegistry, limitCommit))
                {

                    var usuarioLogado = DbContext.GetAutoHistoryUsername() ?? DbContext.GetDbContextUsername();
                    var userId = DbContext.GetDbContextUserId();

                    UsernameContext = string.IsNullOrWhiteSpace(usuarioLogado) ? "Anonymous" : usuarioLogado;
                    UserIdContext = string.IsNullOrWhiteSpace(userId) ? "Anonymous" : userId;

                    DbContext.SetAggregatesChanges(-1);

                    foreach (var entry in DbContext.ChangeTracker.Entries()
                        .Where(entry => entry.CurrentValues.Properties
                            .FirstOrDefault(x => x.Name == "DataCadastro") != null))
                    {
                        DbContext.SetAggregatesChanges();
                        Debug.WriteLine($"Entity: {entry.Entity.GetType().Name}, State: {entry.State}");


                        switch (entry.State)
                        {
                            case EntityState.Added:
                                entry.Property("DataCadastro").CurrentValue = PropertyDateType(entry.Property("DataCadastro").Metadata);
                                entry.Property("UsuarioCadastro").CurrentValue = UsernameContext;
                                entry.Property("DataAlteracao").IsModified = false;
                                entry.Property("UsuarioAlteracao").IsModified = false;
                                break;
                            case EntityState.Modified:
                                entry.Property("DataCadastro").IsModified = false;
                                entry.Property("UsuarioCadastro").IsModified = false;
                                entry.Property("DataAlteracao").CurrentValue = PropertyDateType(entry.Property("DataAlteracao").Metadata);
                                entry.Property("UsuarioAlteracao").CurrentValue = UsernameContext;
                                break;
                            case EntityState.Detached:
                                break;
                            case EntityState.Unchanged:
                                break;
                            case EntityState.Deleted:
                                break;
                            default:
                                break;
                        }

                    }

                    foreach (var entry in DbContext.ChangeTracker.Entries()
                        .Where(entry => entry.CurrentValues.Properties
                            .FirstOrDefault(x => x.Name == "UsuarioIdCadastro") != null))
                    {
                        Debug.WriteLine($"UserId Entity: {entry.Entity.GetType().Name}, State: {entry.State}");


                        switch (entry.State)
                        {
                            case EntityState.Added:
                                entry.Property("UsuarioIdCadastro").CurrentValue = UserIdContext;
                                entry.Property("UsuarioIdAlteracao").IsModified = false;
                                break;
                            case EntityState.Modified:
                                entry.Property("UsuarioIdCadastro").IsModified = false;
                                entry.Property("UsuarioIdAlteracao").CurrentValue = UserIdContext;
                                break;
                            case EntityState.Detached:
                                break;
                            case EntityState.Unchanged:
                                break;
                            case EntityState.Deleted:
                                break;
                            default:
                                break;
                        }

                    }


                    return await DbContext.SaveChangesAsync();
                }
            }

            return await Task.FromResult(0);
        }

        ///<inheritdoc/>
        public virtual async Task<int> SaveChangesAsync(bool ensureAutoHistory = false, int actualRegistry = 1, int limitCommit = 1, bool toSave = true, params IUnitOfWork[] unitOfWorks)
        {
            CheckDisposed();

            using (var ts = new TransactionScope())
            {
                var count = 0;
                foreach (var unitOfWork in unitOfWorks)
                {
                    count += await unitOfWork.SaveChangesAsync(ensureAutoHistory, actualRegistry, limitCommit, toSave);
                }

                count += await SaveChangesAsync(ensureAutoHistory, actualRegistry, limitCommit, toSave);

                ts.Complete();

                return count;
            }


        }

        private object PropertyDateType(IPropertyBase property)
        {
            var propertyClrType = property.ClrType.ToString();
            if (propertyClrType.Contains("System.DateTimeOffset"))
            {
                return DateTimeOffset.Now;
            }

            return DateTime.Now;

        }

        private static bool CommitAsync(int actualRegistry = 1, int limitCommit = 1)
        {
            Math.DivRem(actualRegistry, limitCommit, out int resto);
            if (resto == 0)
            {
                return true;
            }

            return false;
        }

        public void CheckDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(UnitOfWork));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && DbContext != null && !IsDisposed)
            {
                DbContext.Dispose();
                IsDisposed = true;
            }
        }


    }
}
