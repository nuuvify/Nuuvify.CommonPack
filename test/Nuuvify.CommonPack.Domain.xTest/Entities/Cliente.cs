namespace Nuuvify.CommonPack.Domain.xTest.Entities;

public class Cliente : AggregateRoot
{
    public Cliente(int codigo, string nome)
    {
        Codigo = codigo;
        Nome = nome;
    }

    public int Codigo { get; private set; }
    public string Nome { get; private set; }
    public DateTime? DataUltimoPedido { get; set; }

}
