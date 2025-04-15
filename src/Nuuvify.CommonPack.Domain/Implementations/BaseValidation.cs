using Nuuvify.CommonPack.Domain.Interfaces;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain;

public abstract class BaseValidation<TEntity, TValidation> : NotifiableR,
    IValidation<TEntity, TValidation>
    where TEntity : DomainEntity
    where TValidation : class
{

    public abstract Task Valid(TEntity entity);

    public IList<NotificationR> ValidationResult()
    {
        return Notifications.ToList();
    }
}
