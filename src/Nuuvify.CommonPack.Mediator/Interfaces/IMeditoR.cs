using System.Threading;
using System.Threading.Tasks;

namespace Nuuvify.CommonPack.MediatoR.Interfaces;


public interface IMediatoR
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;
}
