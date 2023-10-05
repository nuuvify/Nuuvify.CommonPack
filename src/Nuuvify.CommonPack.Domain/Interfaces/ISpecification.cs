using System.Collections.Generic;
using System.Threading.Tasks;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain.Interfaces
{
    public interface ISpecification<TEntity> 
        where TEntity : DomainEntity
    {

        /// <summary>
        /// Deve retornar <c>await Task.CompletedTask;</c>
        /// <para>Deve ser utilizado AddNotification() ou new ValidationConcernR{SuaClasse}(this)</para>
        /// <para>para incluir uma lista de notificações</para>
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task IsSatisfactory(TEntity entity);
        IList<NotificationR> ValidationResult();
    }
}
