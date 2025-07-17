using System.Collections.Generic;

namespace Nuuvify.CommonPack.Domain.xTest.Entities
{
    public class ClienteUpdatedEvent : DomainEvent<string>
    {

        public ClienteUpdatedEvent(Cliente updateCliente, string version)
            : base(updateCliente.Id, version)
        {
            UpdateCliente = updateCliente;
        }

        public Cliente UpdateCliente { get; }
        public string InformacaoQueEuQuero { get; set; }
        public IEnumerable<string> MinhaLista { get; set; }
        
    }
}