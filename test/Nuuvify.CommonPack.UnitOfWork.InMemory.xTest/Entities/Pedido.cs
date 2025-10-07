using Nuuvify.CommonPack.Domain;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Entities;

public class Pedido : AggregateRoot
{
    protected Pedido() { }

    public Pedido(int numeroPedido, decimal valorTotal)
    {
        NumeroPedido = numeroPedido;
        ValorTotal = valorTotal;
        Itens = new List<PedidoItem>();
    }

    public int NumeroPedido { get; private set; }
    public decimal ValorTotal { get; private set; }
    public virtual Fatura? Fatura { get; private set; }
    public virtual ICollection<PedidoItem> Itens { get; private set; } = new List<PedidoItem>();

    public void Update(decimal novoValorTotal)
    {
        ValorTotal = novoValorTotal;
    }

    public void DefinirFatura(Fatura fatura)
    {
        Fatura = fatura;
    }

    public void AdicionarItem(PedidoItem item)
    {
        item.DefinirPedido(this);
        Itens.Add(item);
    }

    public void AdicionarItens(IList<PedidoItem> itens)
    {
        foreach (var item in itens)
        {
            AdicionarItem(item);
        }
    }
}
