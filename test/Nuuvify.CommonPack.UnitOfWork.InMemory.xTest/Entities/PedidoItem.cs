using Nuuvify.CommonPack.Domain;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Entities;

public class PedidoItem : AggregateRoot
{
    protected PedidoItem() { }

    public PedidoItem(string descricaoItem, int quantidade, decimal valorUnitario)
    {
        DescricaoItem = descricaoItem;
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;
    }

    public string DescricaoItem { get; private set; } = string.Empty;
    public int Quantidade { get; private set; }
    public decimal ValorUnitario { get; private set; }
    public virtual Pedido? Pedido { get; private set; }

    public void Update(string novaDescricao, int novaQuantidade, decimal novoValorUnitario)
    {
        DescricaoItem = novaDescricao;
        Quantidade = novaQuantidade;
        ValorUnitario = novoValorUnitario;
    }

    public void DefinirPedido(Pedido pedido)
    {
        Pedido = pedido;
    }
}
