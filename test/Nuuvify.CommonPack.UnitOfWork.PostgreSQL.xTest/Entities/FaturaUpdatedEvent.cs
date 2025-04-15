using Nuuvify.CommonPack.Domain;

namespace Nuuvify.CommonPack.UnitOfWork.PostgreSQL.xTest;

public class FaturaUpdatedEvent : DomainEvent<Fatura>
{
    public FaturaUpdatedEvent(Fatura sourceId, string version, Fatura newFatura)
        : base(sourceId, version)
    {
        NewFatura = newFatura;
    }

    public Fatura NewFatura { get; set; }

}
