namespace Nuuvify.CommonPack.Domain.xTest.Entities;

public class ClienteAddedEvent : DomainEvent<string>
{

    public ClienteAddedEvent(Cliente newCliente, string version)
        : base(newCliente?.Id, version)
    {
        NewCliente = newCliente;
    }

    public Cliente NewCliente { get; }
    public string InformacaoQueEuQuero { get; set; }
    public IEnumerable<string> MinhaLista { get; set; }

}
