using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Nuuvify.CommonPack.Security.JwtCredentials.Model;

namespace Nuuvify.CommonPack.Security.JwtStore.Ef;

internal partial class DatabaseJwtStore<TContext>
{
    public async Task<List<JwtCacheToken>> GetTokensAsync(DbContext context, CancellationToken cancellationToken)
    {
        // Obtém o tipo do contexto
        var contextType = context.GetType();

        // Encontra a propriedade com o atributo PropertyName("Tokens")
        var property = contextType.GetProperties()
            .FirstOrDefault(p => p.GetCustomAttribute<PropertyNameAttribute>()?.Name == "Tokens");

        if (property == null)
        {
            throw new InvalidOperationException("A propriedade com o nome 'Tokens' não foi encontrada.");
        }

        // Obtém o valor da propriedade (DbSet<JwtCacheToken>)
        var dbSet = property.GetValue(context) as IQueryable<JwtCacheToken>;

        if (dbSet == null)
        {
            throw new InvalidOperationException("A propriedade 'Tokens' não é um DbSet<JwtCacheToken>.");
        }


        return await dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }
}
