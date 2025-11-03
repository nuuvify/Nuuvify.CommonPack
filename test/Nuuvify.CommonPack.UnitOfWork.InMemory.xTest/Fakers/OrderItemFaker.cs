using Bogus;
using Nuuvify.CommonPack.UnitOfWork.Examples;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Fakers;

/// <summary>
/// Faker para geração de dados de teste da entidade OrderItem.
/// </summary>
public static class OrderItemFaker
{
    private static readonly Faker<OrderItem> _faker = new Faker<OrderItem>()
        .RuleFor(oi => oi.ProductId, f => f.Random.Int(1, 1000))
        .RuleFor(oi => oi.Quantity, f => f.Random.Int(1, 10))
        .RuleFor(oi => oi.UnitPrice, f => f.Finance.Amount(10, 500));

    public static OrderItem Generate() => _faker.Generate();

    public static IList<OrderItem> Generate(int count) => _faker.Generate(count);
}
