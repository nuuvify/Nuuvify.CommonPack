
using Nuuvify.CommonPack.Mediator.Implementation;
using Nuuvify.CommonPack.Mediator.Interfaces;

namespace Nuuvify.CommonPack.Domain;

/// <summary>
/// Use essa classe base em uma classe de outro contexto, para que ela seja disparada pelo mediator.Publish(SuaClasseCommandResult)
/// Exemplo:
/// onde a classe TransportadorObserve, é outro contexto e, SuaClasseCommandResult foi populada no contexto original.
/// public class TransportadorObserve : INotificationHandler{SuaClasseCommandResult}
/// </summary>
/// <typeparam name="TSourceId">Representa um object, que devera ser utilizado em um Observable</typeparam>
public abstract class DomainEvent<TSourceId> : NotifiableR, INotification
{
    public TSourceId SourceId { get; private set; }
    public DateTimeOffset When { get; private set; }
    /// <summary>
    /// Pode ser utilizado com qualquer valor util para identificar esse evento,
    /// ou informe o RequestConfiguration.CorrelationId
    /// </summary>
    /// <example>Daf_AAAAA-BBBBB-CCCC</example>
    public string Version { get; private set; }

    protected DomainEvent(TSourceId sourceId, string version)
    {
        if (sourceId == null)
        {
            AddNotification(nameof(DomainEvent<TSourceId>),
                "Não pode ser nulo");
        }

        _ = new ValidationConcernR<DomainEvent<TSourceId>>(this)
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
