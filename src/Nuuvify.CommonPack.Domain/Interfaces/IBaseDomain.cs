using System.Collections.Generic;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain.Interfaces
{
    public interface IBaseDomain
    {
        IList<NotificationR> ValidationResult();

    }
}
