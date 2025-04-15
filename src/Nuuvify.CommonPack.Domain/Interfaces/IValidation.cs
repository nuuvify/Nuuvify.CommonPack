using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain.Interfaces;

public interface IValidation<TEntity, TValidation>
    where TEntity : DomainEntity
    where TValidation : class
{

    /// <summary>
    /// Deve retornar <c>await Task.CompletedTask;</c>
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task Valid(TEntity entity);
    IList<NotificationR> ValidationResult();
}
