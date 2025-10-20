using Bogus;
using Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Entities;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Fakers;

public static class PedidoItemFaker
{
    private static readonly Faker<PedidoItem> _faker = new Faker<PedidoItem>()
        .CustomInstantiator(f => new PedidoItem(
            descricaoItem: f.Commerce.ProductName(),
            quantidade: f.Random.Int(1, 10),
            valorUnitario: f.Finance.Amount(1, 100)));

    public static PedidoItem Generate() => _faker.Generate();

    public static IList<PedidoItem> Generate(int count) => _faker.Generate(count);
}
