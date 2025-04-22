using Nuuvify.CommonPack.Extensions.Implementation;

namespace Nuuvify.CommonPack.UnitOfWork.SqlServer.xTest;

public class PedidoItem : DomainEntity
{

    protected PedidoItem() { }
    public PedidoItem(string codigoMercadoria, decimal quantidade, decimal valorUnitario)
    {
        CodigoMercadoria = codigoMercadoria.SubstringNotNull(0, 10);
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;

    }

    public string CodigoMercadoria { get; private set; }
    public decimal Quantidade { get; private set; }
    public decimal ValorUnitario { get; private set; }

    public virtual Pedido Pedido { get; private set; }
    public string PedidoId { get; private set; }

    public void DefinirPedido(Pedido pedido)
    {
        PedidoId = pedido.Id;
        Pedido = pedido;
    }
}
