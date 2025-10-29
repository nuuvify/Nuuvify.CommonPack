using Bogus;
using Nuuvify.CommonPack.UnitOfWork.Examples;

namespace Nuuvify.CommonPack.UnitOfWork.Integration.xTest.Fakers;

public static class ProductFaker
{
    private static readonly string[] Categories = { "Eletrônicos", "Roupas", "Alimentos", "Livros", "Esportes" };

    public static Product Generate()
    {
        return new Faker<Product>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.Category, f => f.PickRandom(Categories))
            .RuleFor(p => p.Price, f => f.Random.Decimal(10, 1000))
            .RuleFor(p => p.Stock, f => f.Random.Int(0, 100))
            .RuleFor(p => p.IsActive, f => f.Random.Bool(0.8f)) // 80% chance of being active
            .RuleFor(p => p.CreatedAt, f => f.Date.Past(1))
            .RuleFor(p => p.LastUpdate, f => f.Random.Bool() ? f.Date.Recent(30) : (DateTime?)null)
            .Generate();
    }

    public static List<Product> Generate(int count)
    {
        return new Faker<Product>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.Category, f => f.PickRandom(Categories))
            .RuleFor(p => p.Price, f => f.Random.Decimal(10, 1000))
            .RuleFor(p => p.Stock, f => f.Random.Int(0, 100))
            .RuleFor(p => p.IsActive, f => f.Random.Bool(0.8f))
            .RuleFor(p => p.CreatedAt, f => f.Date.Past(1))
            .RuleFor(p => p.LastUpdate, f => f.Random.Bool() ? f.Date.Recent(30) : (DateTime?)null)
            .Generate(count);
    }

    public static Product GenerateWithSpecificData(
        string name,
        string category,
        decimal price,
        int stock,
        bool isActive = true)
    {
        return new Product
        {
            Name = name,
            Description = $"Descrição para {name}",
            Category = category,
            Price = price,
            Stock = stock,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            LastUpdate = null
        };
    }
}
