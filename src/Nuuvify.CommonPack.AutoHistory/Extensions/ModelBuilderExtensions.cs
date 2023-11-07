using System;
using Nuuvify.CommonPack.UnitOfWork.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace Nuuvify.CommonPack.AutoHistory.Extensions
{
    /// <summary>
    /// Represents a plugin for Microsoft.EntityFrameworkCore to support automatically recording data changes history.
    /// </summary>
    public static class ModelBuilderExtensions
    {
        public const int DefaultChangedMaxLength = 4000;

        /// <summary>
        /// Enables the automatic recording change history
        /// <example>
        /// <code>
        ///     modelBuilder.EnableAutoHistory{AutoHistory}(o =>
        ///     {
        ///         o.ProviderName = Database.ProviderName;
        ///     });
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ModelBuilder"/> to enable auto history feature</param>
        /// <param name="configure">Select settings for your history table field size and serialization parameters.</param>
        /// <returns>The <see cref="ModelBuilder"/>had enabled auto history feature</returns>
        public static ModelBuilder EnableAutoHistory(this ModelBuilder modelBuilder,
            Action<AutoHistoryOptions> configure)
        {
            return EnableAutoHistory<AutoHistory>(modelBuilder, configure);
        }


        ///<inheritdoc cref="EnableAutoHistory"/>
        public static ModelBuilder EnableAutoHistory<TAutoHistory>(this ModelBuilder modelBuilder,
            Action<AutoHistoryOptions> configure)
            where TAutoHistory : AutoHistory
        {

            var options = AutoHistoryOptions.Instance;
            configure?.Invoke(options);



            ProviderSelected.ProviderName = options.ProviderName;

            if (ProviderSelected.IsProviderOracle())
            {
                return OracleModelBuilderExtensions.EnableAutoHistory<TAutoHistory>(modelBuilder, configure);
            }
            else if (ProviderSelected.IsProviderSqlServer() || ProviderSelected.IsProviderSqLite())
            {
                return SqlServerModelBuilderExtensions.EnableAutoHistory<TAutoHistory>(modelBuilder, configure);
            }
            else if (ProviderSelected.IsProviderDb2())
            {
                return Db2ModelBuilderExtensions.EnableAutoHistory<TAutoHistory>(modelBuilder, configure);
            }
            else if (ProviderSelected.IsProviderPostgreSQL())
            {
                return PostgreSQLModelBuilderExtensions.EnableAutoHistory<TAutoHistory>(modelBuilder, configure);
            }
            else
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(configure),
                    $"Please enter a valid provider, only the following providers are supported: {ProviderSelected.GetSuportedProviders()}");

            }

        }


    }
}
