using System.Collections.Generic;
using Nuuvify.CommonPack.Domain;

namespace Nuuvify.CommonPack.UnitOfWork.SqlServer.xTest
{
    public class Fatura : AggregateRoot
    {
        protected Fatura() { }
        public Fatura(int numeroFatura, Endereco enderecoFatura, Endereco enderecoEntrega)
        {
            NumeroFatura = numeroFatura;
            EnderecoFatura = enderecoFatura;
            EnderecoEntrega = enderecoEntrega;

            Pedidos = new List<Pedido>();
        }

        public int NumeroFatura { get; private set; }
        public string Observacao { get; private set; }
        public virtual Endereco EnderecoFatura { get; private set; }
        public virtual Endereco EnderecoEntrega { get; private set; }

        public virtual ICollection<Pedido> Pedidos { get; private set; }


        public void Update(string observacao)
        {
            Observacao = observacao;
        }


        public void AdicionarPedido(Pedido pedido)
        {
            pedido.DefinirFatura(this);
            Pedidos.Add(pedido);
        }
        public void AdicionarPedido(IList<Pedido> pedidos)
        {
            foreach (var pedido in pedidos)
            {
                AdicionarPedido(pedido);
            }

        }
    }
}
