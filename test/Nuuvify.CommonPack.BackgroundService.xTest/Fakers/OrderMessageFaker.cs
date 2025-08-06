using Bogus;
using Nuuvify.CommonPack.BackgroundService.Examples;

namespace Nuuvify.CommonPack.BackgroundService.xTest.Fakers;

/// <summary>
/// Faker para gerar dados de teste para OrderMessage e OrderItem
/// </summary>
public static class OrderMessageFaker
{
    /// <summary>
    /// Gera uma instância de OrderMessage para testes
    /// </summary>
    /// <returns>OrderMessage com dados de teste</returns>
    public static OrderMessage Generate()
    {
        var faker = new Faker<OrderMessage>()
            .RuleFor(o => o.OrderId, f => f.Random.Guid().ToString())
            .RuleFor(o => o.CustomerId, f => f.Random.Guid().ToString())
            .RuleFor(o => o.TotalAmount, f => f.Random.Decimal(10, 1000))
            .RuleFor(o => o.OrderDate, f => f.Date.Recent(30))
            .RuleFor(o => o.Items, f => GenerateOrderItems(f.Random.Int(1, 5)));

        return faker.Generate();
    }

    /// <summary>
    /// Gera uma collection de OrderMessage para testes
    /// </summary>
    /// <param name="count">Quantidade de OrderMessage a serem gerados</param>
    /// <returns>Coleção de OrderMessage</returns>
    public static IEnumerable<OrderMessage> Generate(int count)
    {
        return Enumerable.Range(0, count).Select(_ => Generate());
    }

    /// <summary>
    /// Gera uma instância de OrderItem para testes
    /// </summary>
    /// <returns>OrderItem com dados de teste</returns>
    public static OrderItem GenerateOrderItem()
    {
        var faker = new Faker<OrderItem>()
            .RuleFor(i => i.ProductId, f => f.Random.Guid().ToString())
            .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
            .RuleFor(i => i.Quantity, f => f.Random.Int(1, 10))
            .RuleFor(i => i.UnitPrice, f => f.Random.Decimal(1, 100));

        return faker.Generate();
    }

    /// <summary>
    /// Gera uma collection de OrderItem para testes
    /// </summary>
    /// <param name="count">Quantidade de OrderItem a serem gerados</param>
    /// <returns>Coleção de OrderItem</returns>
    public static IList<OrderItem> GenerateOrderItems(int count)
    {
        return Enumerable.Range(0, count).Select(_ => GenerateOrderItem()).ToList();
    }

    /// <summary>
    /// Gera um OrderMessage com valor total inválido para testes de validação
    /// </summary>
    /// <returns>OrderMessage com TotalAmount inválido</returns>
    public static OrderMessage GenerateInvalidOrder()
    {
        var order = Generate();
        order.TotalAmount = -1; // Valor inválido
        return order;
    }

    /// <summary>
    /// Gera um OrderMessage com dados mínimos válidos
    /// </summary>
    /// <returns>OrderMessage com dados básicos válidos</returns>
    public static OrderMessage GenerateValidOrder()
    {
        return new OrderMessage
        {
            OrderId = Guid.NewGuid().ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            TotalAmount = 100.00m,
            OrderDate = DateTime.UtcNow,
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductId = Guid.NewGuid().ToString(),
                    ProductName = "Test Product",
                    Quantity = 1,
                    UnitPrice = 100.00m
                }
            }
        };
    }
}
