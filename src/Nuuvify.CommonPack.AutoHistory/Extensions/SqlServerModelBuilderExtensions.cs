using Microsoft.EntityFrameworkCore;

namespace Nuuvify.CommonPack.AutoHistory.Extensions;

internal static class SqlServerModelBuilderExtensions
{
    internal static ModelBuilder EnableAutoHistory<TAutoHistory>(this ModelBuilder modelBuilder,
        Action<AutoHistoryOptions> configure) where TAutoHistory : AutoHistory
    {
        var options = AutoHistoryOptions.Instance;
        configure?.Invoke(options);

        _ = modelBuilder.Entity<TAutoHistory>(b =>
        {

            _ = b.ToTable(b.Metadata.ClrType.Name.ToUpper());

            _ = b.HasKey(c => c.Id).HasName("PK_AUTOHISTORY_ID");
            _ = b.Property(c => c.Id).IsRequired().HasColumnName("ID").IsUnicode(false).HasMaxLength(36);
            _ = b.Property(c => c.RowId).IsRequired().HasColumnName("ROW_ID").IsUnicode(false).HasMaxLength(options.RowIdMaxLength);
            _ = b.Property(c => c.CorrelationId).HasColumnName("CORRELATION_ID").IsUnicode(false).HasMaxLength(options.CorrelationIdMaxLength);
            _ = b.Property(c => c.TableName).IsRequired().HasColumnName("TABLE_NAME").IsUnicode(false).HasMaxLength(options.TableMaxLength);

            var max = 0;
            if (options.LimitChangedLength)
            {
                max = options.ChangedMaxLength ?? ModelBuilderExtensions.DefaultChangedMaxLength;
                if (max <= 0) max = ModelBuilderExtensions.DefaultChangedMaxLength;
            }
            else
            {
                max = ModelBuilderExtensions.DefaultChangedMaxLength;
            }

            _ = b.Property(c => c.Before).HasColumnName("BEFORE")
                .HasColumnType($"NVARCHAR({max})");

            _ = b.Property(c => c.After).HasColumnName("AFTER")
                .HasColumnType($"NVARCHAR({max})");

            _ = b.Property(c => c.Kind)
                .IsRequired()
                .HasColumnName("KIND")
                .IsUnicode(false)
                .HasMaxLength(options.KindMaxLength)
                .HasConversion(v => v.ToString(),
                               v => (EntityState)Enum.Parse(typeof(EntityState), v));

            _ = b.Property(c => c.Created).IsRequired().HasColumnName("CREATED");
            _ = b.Property(c => c.Username).HasColumnName("USERNAME").IsUnicode(false).HasMaxLength(36);
            _ = b.Property(c => c.PersistInDatabase).HasColumnName("PERSIST").IsUnicode(false).HasMaxLength(10);

        });

        return modelBuilder;
    }
}
