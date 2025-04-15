using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Nuuvify.CommonPack.AutoHistory.Extensions;

/// <summary>
/// Represents a plugin for Microsoft.EntityFrameworkCore to support automatically recording data changes history.
/// </summary>
public static class DbContextExtensions
{
    public static string AutoHistoryUsername { get; set; }
    public static string AutoHistoryCorrelationId { get; set; }

    public static string GetAutoHistoryUsername(this DbContext context)
    {
        return AutoHistoryUsername;
    }
    public static void SetAutoHistoryCorrelationId(this DbContext context, string correlationId)
    {
        AutoHistoryCorrelationId = correlationId;
    }

    /// <summary>
    /// Ensures the automatic history
    /// <para>This method is used by UnitOfWork, where it is possible to use a parameter in SaveChanges() to record the history</para>
    /// <example>
    /// <code>
    ///     await _repository.SaveChangesAsync(ensureAutoHistory: true);
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="context">The context</param>
    /// <param name="ensureAutoHistory">
    /// <para>true = Insert into AutoHistory table</para>
    /// <para>false = Do not insert into AutoHistory table</para></param>
    /// <param name="usernameContext">Username actual context</param>
    /// <param name="toSave">true = Persists in the database</param>
    public static void EnsureAutoHistory(this DbContext context,
        bool ensureAutoHistory = false,
        string usernameContext = null,
        bool toSave = true)
    {

        EnsureAutoHistory<AutoHistory>(context, () => new AutoHistory(),
            ensureAutoHistory: ensureAutoHistory,
            usernameContext: usernameContext,
            toSave: toSave);

        SetUsernameContext(usernameContext);
    }

    ///<inheritdoc cref="EnsureAutoHistory"/>
    public static void EnsureAutoHistory<TAutoHistory>(this DbContext context,
        Func<TAutoHistory> createHistoryFactory,
        bool ensureAutoHistory = false,
        string usernameContext = null,
        bool toSave = true) where TAutoHistory : AutoHistory
    {

        if (ensureAutoHistory)
        {
            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted).ToArray();

            foreach (var entry in entries)
            {
                _ = context.Add<TAutoHistory>(entry.AutoHistory(createHistoryFactory,
                    usernameContext: usernameContext,
                    toSave: toSave));
            }

            SetUsernameContext(usernameContext);
        }

    }

    internal static void SetUsernameContext(string usernameContext = null)
    {
        AutoHistoryUsername = usernameContext.SubstringNotNull(0, 30);
    }
    internal static TAutoHistory AutoHistory<TAutoHistory>(this EntityEntry entry,
        Func<TAutoHistory> createHistoryFactory,
        string usernameContext = null,
        bool toSave = true) where TAutoHistory : AutoHistory
    {

        SetUsernameContext(usernameContext);

        var options = AutoHistoryOptions.Instance;

        if (string.IsNullOrWhiteSpace(options.ProviderName))
            throw new ArgumentException("Ohhh !! You need to configure AutoHistory in OnModelCreating", options.ProviderName);

        var formatting = options.JsonSerializerOptions();

        AutoHistoryCorrelationId = AutoHistoryCorrelationId.SubstringNotNull(0, options.CorrelationIdMaxLength);

        var maxChanged = ModelBuilderExtensions.DefaultChangedMaxLength;

        if (options.LimitChangedLength)
        {
            maxChanged = options.ChangedMaxLength ?? ModelBuilderExtensions.DefaultChangedMaxLength;
            if (maxChanged <= 0)
            {
                maxChanged = ModelBuilderExtensions.DefaultChangedMaxLength;
            }
        }

        var history = createHistoryFactory();
        history.TableName = entry.Metadata.GetTableName();

        var properties = entry.Properties;

        var json = new Dictionary<string, object>();
        switch (entry.State)
        {
            case EntityState.Added:
                foreach (var prop in properties)
                {
                    if (prop.Metadata.IsKey() || prop.Metadata.IsForeignKey())
                    {
                        continue;
                    }
                    json[prop.Metadata.Name] = prop.CurrentValue ?? null;
                }

                history.RowId = "0";
                history.Kind = EntityState.Added;
                history.After = JsonSerializer.Serialize(json, formatting).SubstringNotNull(0, maxChanged);
                history.PersistInDatabase = toSave.ToString();

                if (!string.IsNullOrWhiteSpace(AutoHistoryCorrelationId))
                    history.CorrelationId = AutoHistoryCorrelationId;

                if (!string.IsNullOrWhiteSpace(AutoHistoryUsername))
                    history.Username = AutoHistoryUsername;

                break;
            case EntityState.Modified:
                var bef = new Dictionary<string, object>();
                var aft = new Dictionary<string, object>();

                foreach (var prop in properties)
                {
                    if (prop.IsModified)
                    {
                        if (prop.OriginalValue != null)
                        {
                            if (prop.OriginalValue != prop.CurrentValue)
                            {
                                bef[prop.Metadata.Name] = prop.OriginalValue;
                            }
                            else
                            {
                                var originalValue = entry.GetDatabaseValues().GetValue<object>(prop.Metadata.Name);
                                bef[prop.Metadata.Name] = originalValue ?? null;
                            }
                        }
                        else
                        {
                            bef[prop.Metadata.Name] = null;
                        }

                        aft[prop.Metadata.Name] = prop.CurrentValue ?? null;
                    }
                }

                history.Id = Guid.NewGuid().ToString();
                history.RowId = entry.PrimaryKey();
                history.Kind = EntityState.Modified;
                history.Before = JsonSerializer.Serialize(bef, formatting).SubstringNotNull(0, maxChanged);
                history.After = JsonSerializer.Serialize(aft, formatting).SubstringNotNull(0, maxChanged);
                history.PersistInDatabase = toSave.ToString();

                if (!string.IsNullOrWhiteSpace(AutoHistoryCorrelationId))
                    history.CorrelationId = AutoHistoryCorrelationId;

                if (!string.IsNullOrWhiteSpace(AutoHistoryUsername))
                    history.Username = AutoHistoryUsername;

                break;
            case EntityState.Deleted:
                foreach (var prop in properties)
                {
                    json[prop.Metadata.Name] = prop.OriginalValue ?? null;
                }
                history.Id = Guid.NewGuid().ToString();
                history.RowId = entry.PrimaryKey();
                history.Kind = EntityState.Deleted;
                history.Before = JsonSerializer.Serialize(json, formatting).SubstringNotNull(0, maxChanged);
                history.PersistInDatabase = toSave.ToString();

                if (!string.IsNullOrWhiteSpace(AutoHistoryCorrelationId))
                    history.CorrelationId = AutoHistoryCorrelationId;

                if (!string.IsNullOrWhiteSpace(AutoHistoryUsername))
                    history.Username = AutoHistoryUsername;

                break;
            case EntityState.Detached:
            case EntityState.Unchanged:
            default:
                throw new NotSupportedException("AutoHistory only support Deleted and Modified entity.");
        }

        return history;
    }

    private static string PrimaryKey(this EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();

        var values = new List<object>();
        foreach (var property in key.Properties)
        {
            var value = entry.Property(property.Name).CurrentValue;
            if (value != null)
            {
                values.Add(value);
            }
        }

        return string.Join(",", values);
    }
}
