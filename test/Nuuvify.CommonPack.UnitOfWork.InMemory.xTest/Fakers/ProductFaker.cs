using Bogus;
using Nuuvify.CommonPack.UnitOfWork.Examples;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Fakers;

/// <summary>
/// Faker para geração de dados de teste da entidade Product.
/// </summary>
public static class ProductFaker
{
    private static readonly Faker<Product> _faker = new Faker<Product>()
        .RuleFor(p => p.Name, f => f.Commerce.ProductName())
        .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
        .RuleFor(p => p.Category, f => f.Commerce.Categories(1)[0])
        .RuleFor(p => p.Price, f => f.Finance.Amount(10, 1000))
        .RuleFor(p => p.Stock, f => f.Random.Int(0, 100))
        .RuleFor(p => p.IsActive, f => f.Random.Bool(0.8f))
        .RuleFor(p => p.CreatedAt, f => f.Date.Past(2))
        .RuleFor(p => p.LastUpdate, f => f.Random.Bool() ? f.Date.Recent() : null);

    public static Product Generate() => _faker.Generate();

    public static IList<Product> Generate(int count) => _faker.Generate(count);
}
