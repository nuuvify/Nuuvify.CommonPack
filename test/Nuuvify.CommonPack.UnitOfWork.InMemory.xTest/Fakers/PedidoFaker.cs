using Bogus;
using Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Entities;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Fakers;

public static class PedidoFaker
{
    private static readonly Faker<Pedido> _faker = new Faker<Pedido>()
        .CustomInstantiator(f => new Pedido(
            numeroPedido: f.Random.Int(1, 999),
            valorTotal: f.Finance.Amount(10, 1000)));

    public static Pedido Generate() => _faker.Generate();

    public static IList<Pedido> Generate(int count) => _faker.Generate(count);
}
