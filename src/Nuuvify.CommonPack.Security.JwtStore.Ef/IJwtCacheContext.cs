using Nuuvify.CommonPack.Security.JwtCredentials.Model;
using Microsoft.EntityFrameworkCore;

namespace Nuuvify.CommonPack.Security.JwtStore.Ef;

/// <summary>
/// Use o attributo [PropertyName("Tokens")] para definir o nome da propriedade em seu contexto do EFCore 
/// que representara a tabela no banco de dados, caso a tabela usada para cache no seu banco, não tenha o nome "Tokens",
/// caso sua tabela, tenho o mesmo nome "Tokens", não precisa incluir esse atributo. 
/// </summary>
public interface IJwtCacheContext
{
    /// <summary>
    /// A collection of <see cref="T:Nuuvify.CommonPack.Security.JwtCredentials.Model.JwtCacheToken" />
    /// </summary>
    DbSet<JwtCacheToken> Tokens { get; set; }
}

