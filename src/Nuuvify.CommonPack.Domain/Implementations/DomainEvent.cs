using System;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain
{
    public abstract class DomainEvent<TSourceId> : NotifiableR, ICommandR
    {
        public TSourceId SourceId { get; private set; }
        public DateTimeOffset When { get; private set; }
        /// <summary>
        /// Pode ser utilizado com qualquer valor util para identificar esse evento,
        /// ou informe o RequestConfiguration.CorrelationId
        /// </summary>
        /// <example>Daf_AAAAA-BBBBB-CCCC</example>
        public string Version { get; private set; }
        public bool SaveChanges { get; set; } = true;
        public bool RemoveNotificationsBeginning { get; set; }

        protected DomainEvent(TSourceId sourceId, string version)
        {
            if (sourceId == null)
            {
                AddNotification(nameof(DomainEvent<TSourceId>),
                    "Não pode ser nulo");
            }

            new ValidationConcernR<DomainEvent<TSourceId>>(this)
                .AssertIsRequired(x => version)
                .AssertHasMaxLength(x => version, MaxVersion);

            if (IsValid())
            {
                SourceId = sourceId;
                Version = version;
                When = DateTimeOffset.Now;
            }
            
        }

        public const int MaxVersion = 100;

    }
}