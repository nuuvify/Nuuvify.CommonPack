namespace Nuuvify.CommonPack.Domain.xTest.Entities;

public class ClienteMyRoleEvent : DomainEvent<Cliente>
{
    public ClienteMyRoleEvent(Cliente sourceId, string version, Cliente newCliente)
        : base(sourceId, version)
    {
        NewCliente = newCliente;
    }

    public Cliente NewCliente { get; set; }

}
