
using Nuuvify.CommonPack.Domain.Interfaces;
using Nuuvify.CommonPack.Mediator.Implementation;
using Nuuvify.CommonPack.Mediator.Interfaces;

namespace Nuuvify.CommonPack.Domain;

public abstract class BaseDomain : NotifiableR, IBaseDomain
{
    protected readonly IMediatoR _mediator;

    protected BaseDomain(
        IMediatoR mediator)
    {
        _mediator = mediator;
    }

    public virtual IList<NotificationR> ValidationResult()
    {
        return Notifications.ToList();
    }

}
