using System.Collections.Generic;

namespace Nuuvify.CommonPack.Middleware.Handle
{
    internal class ReturnStandardErrors
    {
        public bool Success { get; set; }
        public IEnumerable<NotificationR> Errors { get; set; }
    }

    internal class NotificationR 
    {
        public string Message { get; set; }
    }
}
