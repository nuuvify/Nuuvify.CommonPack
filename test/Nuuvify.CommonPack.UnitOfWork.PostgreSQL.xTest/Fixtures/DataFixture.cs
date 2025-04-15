using Bogus;

namespace Nuuvify.CommonPack.UnitOfWork.PostgreSQL.xTest.Fixtures;


public class DataFixture
{

    public IList<Fatura> GerarFaturaFake(int count = 1)
    {
        var enderecos = GerarEnderecoFake(2);

        var fake = new Faker<Fatura>("pt_BR")
            .CustomInstantiator(f => new Fatura(
                f.Random.Int(1, 9999999)
                , enderecos[0]
                , enderecos[1])
            );

        return fake.Generate(count);
    }

    public IList<Pedido> GerarPedidoFake(int count = 1, int withItens = 1)
    {

        var fake = new Faker<Pedido>("pt_BR");

        _ = fake.CustomInstantiator(f => new Pedido(
            f.Company.Random.AlphaNumeric(10).ToUpper()
            , f.Random.Int(1, 9999999)
            , DateTime.Now)
        );

        if (withItens > 0)
            _ = fake.RuleFor(p => p.Itens, (p, i) => GerarPedidoItemFake(withItens, i.Id));

        return fake.Generate(count);
    }

    public IList<PedidoItem> GerarPedidoItemFake(int count = 1, string pedidoId = null)
    {

        var fake = new Faker<PedidoItem>("pt_BR");

        _ = fake.CustomInstantiator(f => new PedidoItem(
            f.Commerce.Product()
            , Decimal.Round(f.Random.Decimal(0.10M, 9999.9999M), decimals: 4)
            , Convert.ToDecimal(f.Commerce.Price(min: 1, decimals: 4)))
        );

        if (!string.IsNullOrWhiteSpace(pedidoId))
            _ = fake.RuleFor(p => p.PedidoId, pedidoId);

        return fake.Generate(count);
    }

    public IList<Endereco> GerarEnderecoFake(int count = 1)
    {
        var passoFake = new Faker<Endereco>("pt_BR")
            .CustomInstantiator(f => new Endereco(
                f.Address.StreetAddress()
                , f.Address.City())
            );

        return passoFake.Generate(count);
    }

}
