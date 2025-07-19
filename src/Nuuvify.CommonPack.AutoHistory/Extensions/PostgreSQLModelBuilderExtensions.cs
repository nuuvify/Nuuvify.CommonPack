using System;
using Microsoft.EntityFrameworkCore;

namespace Nuuvify.CommonPack.AutoHistory.Extensions
{
    internal static class PostgreSQLModelBuilderExtensions
    {
        internal static ModelBuilder EnableAutoHistory<TAutoHistory>(this ModelBuilder modelBuilder,
            Action<AutoHistoryOptions> configure) where TAutoHistory : AutoHistory
        {
            var options = AutoHistoryOptions.Instance;
            configure?.Invoke(options);


            modelBuilder.Entity<TAutoHistory>(b =>
            {

                b.ToTable(b.Metadata.ClrType.Name.ToLower());

                b.HasKey(c => c.Id).HasName("pk_autohistory_id");

                b.Property(c => c.Id).IsRequired().IsUnicode(false).HasMaxLength(36);
                b.Property(c => c.RowId).IsRequired().IsUnicode(false).HasMaxLength(options.RowIdMaxLength);
                b.Property(c => c.CorrelationId).IsUnicode(false).HasMaxLength(options.CorrelationIdMaxLength);
                b.Property(c => c.TableName).IsUnicode(false).IsRequired().HasMaxLength(options.TableMaxLength);

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

                b.Property(c => c.Before)
                    .HasColumnType($"json");

                b.Property(c => c.After)
                    .HasColumnType($"json");

                b.Property(c => c.Kind)
                    .IsRequired()
                    .IsUnicode(false)
                    .HasMaxLength(options.KindMaxLength)
                    .HasConversion(v => v.ToString(),
                                   v => (EntityState)Enum.Parse(typeof(EntityState), v));

                b.Property(c => c.Created).IsRequired();
                b.Property(c => c.Username).IsUnicode(false).HasMaxLength(36);
                b.Property(c => c.PersistInDatabase).IsUnicode(false).HasColumnName("persist").HasMaxLength(10);

            });

            return modelBuilder;
        }
    }

}