using Microsoft.Extensions.DependencyInjection;
using Nuuvify.CommonPack.MediatoR.Interfaces;

namespace Nuuvify.CommonPack.MediatoR.Implementation;


public class MediatoR : IMediatoR
{
    private readonly IServiceProvider _provider;

    public MediatoR(IServiceProvider provider)
    {
        _provider = provider;
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IRequestHandler<,>)
            .MakeGenericType(request.GetType(), typeof(TResponse));
        var handler = _provider.GetService(handlerType)
            ?? throw new InvalidOperationException($"Handler not found for {request.GetType().Name}");

        return await (Task<TResponse>)handlerType
            .GetMethod("Handle")!
            .Invoke(handler, [request, cancellationToken])!;
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notification.GetType());
        var handlers = _provider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            await (Task)handlerType
                .GetMethod("Handle")!
                .Invoke(handler, [notification, cancellationToken])!;
        }
    }
}
