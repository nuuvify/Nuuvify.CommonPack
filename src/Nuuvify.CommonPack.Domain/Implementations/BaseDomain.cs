using AutoMapper;
using MediatR;
using Nuuvify.CommonPack.Domain.Interfaces;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain;

public abstract class BaseDomain : NotifiableR, IBaseDomain
{
    protected readonly IMediator _mediator;
    protected readonly IMapper _mapper;

    protected BaseDomain(
        IMediator mediator,
        IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    public virtual IList<NotificationR> ValidationResult()
    {
        return Notifications.ToList();
    }

}
