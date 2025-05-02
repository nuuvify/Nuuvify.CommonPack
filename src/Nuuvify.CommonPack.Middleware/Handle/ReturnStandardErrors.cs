namespace Nuuvify.CommonPack.Middleware.Handle;

internal sealed class ReturnStandardErrors
{
    public bool Success { get; set; }
    public IEnumerable<NotificationR> Errors { get; set; }
}

internal sealed class NotificationR
{
    public string Property { get; set; }
    public string Message { get; set; }
}
