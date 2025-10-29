using Bogus;
using Nuuvify.CommonPack.UnitOfWork.Examples;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Fakers;

/// <summary>
/// Faker para geração de dados de teste da entidade Order.
/// </summary>
public static class OrderFaker
{
    private static readonly Faker<Order> _faker = new Faker<Order>()
        .RuleFor(o => o.CustomerId, f => f.Random.Int(1, 1000))
        .RuleFor(o => o.CustomerName, f => f.Person.FullName)
        .RuleFor(o => o.CustomerEmail, f => f.Person.Email)
        .RuleFor(o => o.OrderDate, f => f.Date.Recent(30))
        .RuleFor(o => o.TotalAmount, f => f.Finance.Amount(50, 5000))
        .RuleFor(o => o.Status, f => f.PickRandom<OrderStatus>())
        .RuleFor(o => o.IsActive, f => f.Random.Bool(0.9f))
        .RuleFor(o => o.Items, f => OrderItemFaker.Generate(f.Random.Int(1, 5)));

    public static Order Generate() => _faker.Generate();

    public static IList<Order> Generate(int count) => _faker.Generate(count);
}
