namespace Nuuvify.CommonPack.Mediator.Interfaces;

public interface INotificationHandler<TNotification>
    where TNotification : INotification
{
    Task Handle(TNotification notification, CancellationToken cancellationToken);
}

