using Nuuvify.CommonPack.Domain;

namespace Nuuvify.CommonPack.UnitOfWork.Oracle.xTest;

public class Pedido : DomainEntity
{
    protected Pedido() { }
    public Pedido(string codigoCliente, int numeroPedido, DateTime dataPedido)
    {
        CodigoCliente = codigoCliente;
        NumeroPedido = numeroPedido;
        DataPedido = dataPedido;

        Itens = new List<PedidoItem>();
    }

    public string CodigoCliente { get; private set; }
    public int NumeroPedido { get; private set; }
    public DateTime DataPedido { get; private set; }

    public string FaturaId { get; private set; }
    public virtual Fatura FaturaPedido { get; private set; }

    public virtual ICollection<PedidoItem> Itens { get; private set; }

    public void AdicionarItem(PedidoItem item)
    {
        item.DefinirPedido(this);
        Itens.Add(item);

    }

    public void AdicionarItem(IList<PedidoItem> itens)
    {
        foreach (var item in itens)
        {
            AdicionarItem(item);
        }

    }
    public void DefinirFatura(Fatura fatura)
    {
        FaturaId = fatura.Id;
        FaturaPedido = fatura;
    }
}
