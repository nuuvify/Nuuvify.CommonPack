using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Nuuvify.CommonPack.UnitOfWork
{
    public static class DbContextExtensions
    {
        public static string DbContextUsername { get; set; }

        /// <summary>
        /// Unique user identification
        /// </summary>
        /// <value></value>
        public static string DbContextUserId { get; set; }

        public static int AggregatesChanges { get; set; }


        public static IDictionary<object, object> FindPrimaryKeyValues<T>(this DbContext dbContext, T entity)
        {
            if (entity == null)
            {
                throw new KeyNotFoundException($"Nenhuma entidade foi informada");
            }

            var keys = FindPrimaryKeyValues(dbContext, typeof(T).GetTypeInfo());
            return keys;
        }

        public static IDictionary<object, object> FindPrimaryKeyValues(this DbContext dbContext, TypeInfo typeInfo)
        {
            var keys = new Dictionary<object, object>();

            if (typeInfo is null)
            {
                throw new KeyNotFoundException($"Não foi encontrado chave primaria na entidade {typeInfo}");
            }

            var properties = dbContext.Model.FindEntityType(typeInfo).FindPrimaryKey().Properties;

            foreach (var item in properties)
            {
                keys.Add(item.FindContainingPrimaryKey(), item.Name);
            }

            return keys;
        }

        /// <summary>
        /// Use zero to automatically increment the counter. Use -1 to reset the counter.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int SetAggregatesChanges(this DbContext context, int value = 0)
        {
            if (value == 0)
            {
                return AggregatesChanges++;
            }
            else
            {
                AggregatesChanges = value < 0 ? 0 : value;
                return AggregatesChanges;
            }
        }

        /// <summary>
        /// Returns the number of objects changed in the SaveChanges operation
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static int GetAggregatesChanges(this DbContext context)
        {
            return AggregatesChanges;
        }

        public static string GetDbContextUserId(this DbContext context)
        {
            return DbContextUserId ?? "anonymous";
        }
        public static string GetDbContextUsername(this DbContext context)
        {
            return DbContextUsername ?? "anonymous";
        }
        public static void SetDbContextUsername(this DbContext context, string username, string userId = null)
        {

            DbContextUsername = username;
            DbContextUserId = userId;
        }

        public static Dictionary<string, object> GetNameServerAndDataSource(this DbContext context)
        {
            var cnnString = context?.Database.GetDbConnection().ConnectionString.Split(';');
            var server = cnnString?.FirstOrDefault(s => s.StartsWith("Server", StringComparison.InvariantCultureIgnoreCase));
            var database = cnnString?.FirstOrDefault(s => s.StartsWith("Database", StringComparison.InvariantCultureIgnoreCase));

            server ??= cnnString?.FirstOrDefault(s => s.StartsWith("Data Source", StringComparison.InvariantCultureIgnoreCase));

            return new Dictionary<string, object>
            {
                { "server", server },
                { "database", database }
            };

        }
        public static Dictionary<string, object> GetNameServerAndDataSource(this IDbConnection dbConn, string connectionString)
        {
            string[] cnnString;

            if (string.IsNullOrWhiteSpace(dbConn?.ConnectionString))
            {
                cnnString = connectionString.Split(';');
            }
            else
            {
                cnnString = dbConn?.ConnectionString.Split(';');
            }
            var server = cnnString?.FirstOrDefault(s => s.StartsWith("Server", StringComparison.InvariantCultureIgnoreCase));
            var database = cnnString?.FirstOrDefault(s => s.StartsWith("Database", StringComparison.InvariantCultureIgnoreCase));

            server ??= cnnString?.FirstOrDefault(s => s.StartsWith("Data Source", StringComparison.InvariantCultureIgnoreCase));

            return new Dictionary<string, object>
            {
                { "server", server },
                { "database", database }
            };

        }

    }

}
