using Nuuvify.CommonPack.Domain.Implementations;
using Nuuvify.CommonPack.Domain.Interfaces;
using Nuuvify.CommonPack.MediatoR.Implementation;

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
