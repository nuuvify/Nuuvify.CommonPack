using System.Text.Json;
using Nuuvify.CommonPack.Security.Abstraction;
using Nuuvify.CommonPack.Security.JwtCredentials.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;


namespace Nuuvify.CommonPack.Security.JwtStore.Ef;

internal partial class DatabaseJwtStore<TContext> : IJwtStore
    where TContext : DbContext, IJwtCacheContext
{
    private readonly TContext _context;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseJwtStore<TContext>> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions;



    public DatabaseJwtStore(
        TContext context,
        ILogger<DatabaseJwtStore<TContext>> logger,
        IServiceProvider serviceProvider)
    {
        _context = context;
        _serviceProvider = serviceProvider;
        _logger = logger;


        _jsonSerializerOptions = new JsonSerializerOptions
        {
            Converters =
                {
                    //new OpenIdTokenConverter()
                },
        };


    }



    public async Task Clear(string username, string cacheType, CancellationToken cancellationToken = default)
    {

        if (cancellationToken.IsCancellationRequested)
        {
            await Task.FromCanceled(cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            _logger.LogInformation("Não foi informado um username para remover token");
        }
        else
        {

            var cache = _serviceProvider.GetRequiredKeyedService<IDistributedCache>(cacheType);

            _logger.LogInformation("Removendo token para: {Username}", username);
            await cache.RemoveAsync(username, cancellationToken);
        }


        await Task.CompletedTask;

    }


    public async Task ClearAll(string cacheType, CancellationToken cancellationToken = default)
    {

        _logger.LogInformation("Limpando todo o cache");

        var cache = _serviceProvider.GetRequiredKeyedService<IDistributedCache>(cacheType);

        var tokens = await _context.Tokens.ToListAsync(cancellationToken);
        foreach (var item in tokens)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                await Task.FromCanceled(cancellationToken);
            }
            await cache.RemoveAsync(item.Id, cancellationToken);
        }


        _logger.LogInformation("Limpeza do cache concluido.");

        await Task.CompletedTask;

    }


    public async Task<CredentialToken> Get(string username, string cacheType, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            await Task.FromCanceled(cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }
        else
        {
            var cache = _serviceProvider.GetRequiredKeyedService<IDistributedCache>(cacheType);

            var resultCache = await cache.GetStringAsync(username, cancellationToken);
            if (resultCache == null)
            {
                return null;
            }
            else
            {
                var token = JsonSerializer.Deserialize<CredentialToken>(resultCache, _jsonSerializerOptions);
                return token;
            }

        }

    }


    public void Set(string username, CredentialToken tokenResult, string cacheType)
    {

        if (!string.IsNullOrWhiteSpace(username) && tokenResult != null)
        {
            var cacheOptions = new DistributedCacheEntryOptions();
            cacheOptions.SetAbsoluteExpiration(tokenResult.Expires);

            var resultCache = JsonSerializer.Serialize(tokenResult);
            var cache = _serviceProvider.GetRequiredKeyedService<IDistributedCache>(cacheType);
            cache.SetString(username, resultCache, cacheOptions);

        }


    }



}
