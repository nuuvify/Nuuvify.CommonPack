using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.SqlServer;
using Nuuvify.CommonPack.Security.JwtCredentials;
using Nuuvify.CommonPack.Security.JwtCredentials.Interfaces;
using Nuuvify.CommonPack.Security.JwtCredentials.Jwk;
using Nuuvify.CommonPack.Security.JwtCredentials.Jwks;
using Nuuvify.CommonPack.Security.JwtCredentials.Jwt;

namespace Microsoft.Extensions.DependencyInjection;

public static class JwkSetManagerDependencyInjection
{
    /// <summary>
    /// Sets the signing credential.
    /// </summary>
    /// <returns></returns>
    public static IJwksBuilder AddJwksManager(this IServiceCollection services,
        Action<JwksOptions> action = null)
    {
        if (action != null)
            _ = services.Configure(action);

        _ = services.AddScoped<IJwkService, JwkService>();
        _ = services.AddScoped<IJwkSetService, JwkSetService>();
        _ = services.AddSingleton<IJwkStore, InMemoryStore>();

        return new JwksBuilder(services);
    }

    /// <summary>
    /// Sets the signing credential.
    /// </summary>
    /// <returns></returns>
    public static IJwksBuilder PersistKeysInCache(this IJwksBuilder builder)
    {
        _ = builder.Services.AddSingleton<IJwkStore, InMemoryStore>();

        return builder;
    }

    /// <summary>
    /// Configura AddDistributedSqlServerCache para armazenamento de tokens, a tabela deve ser criada
    /// no banco conforme exemplo: 
    /// <example>
    /// <code>
    /// CREATE TABLE SeuSchema.Tokens
    /// (
    ///     Id nvarchar(449) COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
    ///     Value varbinary(MAX) NOT NULL,
    ///     ExpiresAtTime datetimeoffset NOT NULL,
    ///     SlidingExpirationInSeconds bigint NULL,
    ///     AbsoluteExpiration datetimeoffset NULL,
    ///     CONSTRAINT Pk_CacheToken_Id PRIMARY KEY(Id)
    /// )
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="connectionString">String para conexão no banco SqlServer</param>
    /// <param name="schemaName">Nome do schema do banco onde ficara a tabela Tokens, exemplo: SeuSchema.Tokens</param>
    /// <param name="tableName">Nome da tabela usada para armazenar o token</param>
    public static IJwksBuilder CacheTokenSetup(this IJwksBuilder builder,
        string connectionString,
        string schemaName = "cache",
        string tableName = "Tokens")
    {

        _ = builder.Services.AddScoped<IJwtSetService, JwtSetService>();

        _ = builder.Services.AddKeyedScoped<IDistributedCache>("SqlServerCache", (serviceProvider, key) =>
        {
            var options = new SqlServerCacheOptions
            {
                ConnectionString = connectionString,
                SchemaName = schemaName,
                TableName = tableName,
            };
            return new SqlServerCache(options);
        });

        return builder;

    }

}
