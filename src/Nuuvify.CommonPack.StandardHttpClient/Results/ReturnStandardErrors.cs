using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.StandardHttpClient.Results;

internal class ReturnStandardErrors
{
    public bool Success { get; set; }
    public IEnumerable<NotificationR> Errors { get; set; }
}
