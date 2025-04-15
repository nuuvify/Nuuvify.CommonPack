namespace Nuuvify.CommonPack.Middleware.Abstraction.Results;

public class ReturnStandardErrors<TNotification> where TNotification : class
{
    public bool Success { get; set; }
    public IEnumerable<TNotification> Errors { get; set; }
}
