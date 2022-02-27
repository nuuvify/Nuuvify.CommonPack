using System;
using Microsoft.EntityFrameworkCore;

namespace Nuuvify.CommonPack.AutoHistory.Extensions
{
    internal static class SqlServerModelBuilderExtensions
    {
        internal static ModelBuilder EnableAutoHistory<TAutoHistory>(this ModelBuilder modelBuilder,
            Action<AutoHistoryOptions> configure) where TAutoHistory : AutoHistory
        {
            var options = AutoHistoryOptions.Instance;
            configure?.Invoke(options);


            modelBuilder.Entity<TAutoHistory>(b =>
            {

                b.ToTable(b.Metadata.ClrType.Name.ToUpper());

                b.HasKey(c => c.Id).HasName("PK_AUTOHISTORY_ID");
                b.Property(c => c.Id).IsRequired().HasColumnName("ID").HasColumnType($"VARCHAR(36)");
                b.Property(c => c.RowId).IsRequired().HasColumnName("ROW_ID").HasColumnType($"VARCHAR({options.RowIdMaxLength})");
                b.Property(c => c.CorrelationId).HasColumnName("CORRELATION_ID").HasColumnType($"VARCHAR({options.CorrelationIdMaxLength})");
                b.Property(c => c.TableName).IsRequired().HasColumnName("TABLE_NAME").HasColumnType($"VARCHAR({options.TableMaxLength})");

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

                b.Property(c => c.Before).HasColumnName("BEFORE")
                    .HasColumnType($"NVARCHAR({max})");

                b.Property(c => c.After).HasColumnName("AFTER")
                    .HasColumnType($"NVARCHAR({max})");

                b.Property(c => c.Kind)
                    .IsRequired()
                    .HasColumnName("KIND")
                    .HasColumnType($"VARCHAR({options.KindMaxLength})")
                    .HasConversion(v => v.ToString(),
                                   v => (EntityState)Enum.Parse(typeof(EntityState), v));

                b.Property(c => c.Created).IsRequired().HasColumnName("CREATED");
                b.Property(c => c.Username).HasColumnName("USERNAME").HasColumnType($"VARCHAR(36)");
                b.Property(c => c.PersistInDatabase).HasColumnName("PERSIST").HasColumnType($"VARCHAR(10)");

            });

            return modelBuilder;
        }
    }

}