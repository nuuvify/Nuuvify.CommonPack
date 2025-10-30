using Microsoft.EntityFrameworkCore;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Collections;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;
using Nuuvify.CommonPack.UnitOfWork.Examples;
using Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Fakers;
using Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Fixtures;
using System.Globalization;
using Xunit;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Tests;

/// <summary>
/// Testes para o método PagedList.From() e funcionalidades do UnitOfWork.Abstraction.
/// Testa diretamente as funcionalidades do PagedList sem depender de classes Example.
/// </summary>
[Trait("Category", "Unit")]
public sealed class PagedListConversionTest : IClassFixture<ProductDbContextFixture>
{
    private readonly ProductDbContextFixture _fixture;

    public PagedListConversionTest(ProductDbContextFixture fixture)
    {
        _fixture = fixture;
        SeedDatabase().GetAwaiter().GetResult();
    }

    private ExampleDbContext CreateContext() => _fixture.CreateContext();

    private IRepository<Product> CreateProductRepository()
    {
        var context = CreateContext();
        using var unitOfWork = new UnitOfWork<ExampleDbContext>(context)
        {
            UsernameContext = "PagedListTestUser"
        };
        return new Repository<Product>(context, unitOfWork);
    }

    private IRepository<Order> CreateOrderRepository()
    {
        var context = CreateContext();
        using var unitOfWork = new UnitOfWork<ExampleDbContext>(context)
        {
            UsernameContext = "PagedListTestUser"
        };
        return new Repository<Order>(context, unitOfWork);
    }

    private async Task SeedDatabase()
    {
        var context = CreateContext();
        if (await context.Products.AnyAsync())
            return;

        var products = ProductFaker.Generate(50);
        foreach (var product in products)
        {
            product.IsActive = true;
        }

        await context.Products.AddRangeAsync(products);

        var orders = OrderFaker.Generate(20);
        foreach (var order in orders)
        {
            foreach (var item in order.Items)
            {
                item.Product = products[Random.Shared.Next(products.Count)];
            }
        }

        await context.Orders.AddRangeAsync(orders);
        _ = await context.SaveChangesAsync();
    }

    #region Testes Básicos do PagedList.From()

    [Fact]
    public async Task From_ShouldConvertPagedListToDto_PreservingMetadata()
    {
        // Arrange
        var repository = CreateProductRepository();
        var pagedProducts = await repository.GetPagedListAsync(
            predicate: p => p.IsActive,
            orderBy: query => query.OrderBy(p => p.Name),
            pageIndex: 1,
            pageSize: 10);

        // Act - Testar PagedList.From() diretamente
        var pagedDtos = PagedList.From<ProductDto, Product>(
            source: pagedProducts,
            converter: products => products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Category = p.Category,
                Stock = p.Stock
            }));

        // Assert
        Assert.NotNull(pagedDtos);
        Assert.NotEmpty(pagedDtos.Items);
        Assert.Equal(pagedProducts.PageIndex, pagedDtos.PageIndex);
        Assert.Equal(pagedProducts.PageSize, pagedDtos.PageSize);
        Assert.Equal(pagedProducts.TotalCount, pagedDtos.TotalCount);
        Assert.Equal(pagedProducts.TotalPages, pagedDtos.TotalPages);

        var firstDto = pagedDtos.Items[0];
        var firstProduct = pagedProducts.Items[0];
        Assert.Equal(firstProduct.Id, firstDto.Id);
        Assert.Equal(firstProduct.Name, firstDto.Name);
        Assert.Equal(firstProduct.Price, firstDto.Price);
    }

    [Fact]
    public async Task From_ShouldApplyCustomConverter_WithBusinessLogic()
    {
        // Arrange
        var repository = CreateProductRepository();
        var pagedProducts = await repository.GetPagedListAsync(pageIndex: 1, pageSize: 20);

        // Act
        var pagedSummary = PagedList.From<ProductSummaryDto, Product>(
            source: pagedProducts,
            converter: products => products.Select(p => new ProductSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                InStock = p.Stock > 0,
                DaysOld = (DateTime.UtcNow - p.CreatedAt).Days
            }));

        // Assert
        Assert.NotNull(pagedSummary);
        Assert.NotEmpty(pagedSummary.Items);
        Assert.Equal(pagedProducts.TotalCount, pagedSummary.TotalCount);
        Assert.All(pagedSummary.Items, dto =>
        {
            Assert.NotEqual(0, dto.Id);
            Assert.NotEmpty(dto.Name);
            Assert.True(dto.DaysOld >= 0);
        });
    }

    [Fact]
    public async Task From_ShouldHandleEmptyPagedList()
    {
        // Arrange
        var repository = CreateProductRepository();
        var emptyPaged = await repository.GetPagedListAsync(
            predicate: p => p.Id < 0,
            pageIndex: 1,
            pageSize: 10);

        // Act
        var emptyDtos = PagedList.From<ProductDto, Product>(
            source: emptyPaged,
            converter: products => products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Category = p.Category,
                Stock = p.Stock
            }));

        // Assert
        Assert.NotNull(emptyDtos);
        Assert.Empty(emptyDtos.Items);
        Assert.Equal(0, emptyDtos.TotalCount);
    }

    [Fact]
    public void From_ShouldConvertEmptyPagedList_CreatedWithEmptyMethod()
    {
        // Arrange
        var emptyProducts = PagedList.Empty<Product>();

        // Act
        var emptyDtos = PagedList.From<ProductDto, Product>(
            source: emptyProducts,
            converter: products => products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Category = p.Category,
                Stock = p.Stock
            }));

        // Assert
        Assert.NotNull(emptyDtos);
        Assert.Empty(emptyDtos.Items);
        Assert.Equal(0, emptyDtos.TotalCount);
    }

    #endregion

    #region Testes de Conversão em Pipeline

    [Fact]
    public async Task From_ShouldSupportMultipleConversions()
    {
        // Arrange
        var repository = CreateProductRepository();
        var pagedProducts = await repository.GetPagedListAsync(pageIndex: 1, pageSize: 10);

        // Act - Primeira conversão
        var pagedBasicDto = PagedList.From<ProductBasicDto, Product>(
            source: pagedProducts,
            converter: products => products.Select(p => new ProductBasicDto
            {
                Id = p.Id.ToString(CultureInfo.InvariantCulture),
                Name = p.Name,
                Price = p.Price
            }));

        // Segunda conversão
        var pagedDisplayDto = PagedList.From<ProductDisplayDto, ProductBasicDto>(
            source: pagedBasicDto,
            converter: dtos => dtos.Select(dto => new ProductDisplayDto
            {
                Id = dto.Id,
                DisplayName = $"#{dto.Id} - {dto.Name}",
                DisplayPrice = $"R$ {dto.Price:N2}",
                ShowBadge = dto.Price > 500
            }));

        // Assert
        Assert.NotNull(pagedDisplayDto);
        Assert.NotEmpty(pagedDisplayDto.Items);
        Assert.Equal(pagedProducts.TotalCount, pagedDisplayDto.TotalCount);
        Assert.Equal(pagedProducts.PageIndex, pagedDisplayDto.PageIndex);

        var firstDisplay = pagedDisplayDto.Items[0];
        Assert.StartsWith("#", firstDisplay.DisplayName);
        Assert.Contains("R$", firstDisplay.DisplayPrice);
    }

    [Fact]
    public async Task From_ShouldPreserveMetadata_ThroughMultipleConversions()
    {
        // Arrange
        var repository = CreateProductRepository();
        var pagedProducts = await repository.GetPagedListAsync(pageIndex: 2, pageSize: 15);

        // Act
        var step1 = PagedList.From<ProductDto, Product>(
            source: pagedProducts,
            converter: products => products.Select(p => new ProductDto { Id = p.Id, Name = p.Name, Price = p.Price, Category = p.Category, Stock = p.Stock }));

        var step2 = PagedList.From<ProductSummaryDto, ProductDto>(
            source: step1,
            converter: dtos => dtos.Select(dto => new ProductSummaryDto { Id = dto.Id, Name = dto.Name, Price = dto.Price, InStock = dto.Stock > 0, DaysOld = 0 }));

        var step3 = PagedList.From<ProductBasicDto, ProductSummaryDto>(
            source: step2,
            converter: summaries => summaries.Select(s => new ProductBasicDto { Id = s.Id.ToString(CultureInfo.InvariantCulture), Name = s.Name, Price = s.Price }));

        // Assert
        Assert.Equal(pagedProducts.PageIndex, step3.PageIndex);
        Assert.Equal(pagedProducts.PageSize, step3.PageSize);
        Assert.Equal(pagedProducts.TotalCount, step3.TotalCount);
    }

    #endregion

    #region Testes com Agregação

    [Fact]
    public async Task From_ShouldConvertWithAggregation()
    {
        // Arrange
        var repository = CreateOrderRepository();
        var pagedOrders = await repository.GetPagedListAsync(pageIndex: 1, pageSize: 10);

        // Act
        var pagedSummary = PagedList.From<OrderSummaryDto, Order>(
            source: pagedOrders,
            converter: orders => orders.Select(o => new OrderSummaryDto
            {
                OrderId = o.Id.ToString(CultureInfo.InvariantCulture),
                OrderDate = o.OrderDate,
                TotalItems = o.Items.Count,
                TotalAmount = o.Items.Sum(i => i.Quantity * i.UnitPrice)
            }));

        // Assert
        Assert.NotNull(pagedSummary);
        Assert.NotEmpty(pagedSummary.Items);
        Assert.Equal(pagedOrders.TotalCount, pagedSummary.TotalCount);
    }

    #endregion

    #region Testes de GetPagedListAsync

    [Fact]
    public async Task GetPagedListAsync_ShouldFilterByPredicate()
    {
        // Arrange
        var repository = CreateProductRepository();

        // Act
        var pagedActive = await repository.GetPagedListAsync(
            predicate: p => p.IsActive,
            pageIndex: 1,
            pageSize: 10);

        // Assert
        Assert.NotEmpty(pagedActive.Items);
        Assert.All(pagedActive.Items, product => Assert.True(product.IsActive));
    }

    [Fact]
    public async Task GetPagedListAsync_ShouldOrderResults()
    {
        // Arrange
        var repository = CreateProductRepository();

        // Act
        var pagedOrdered = await repository.GetPagedListAsync(
            orderBy: query => query.OrderBy(p => p.Name),
            pageIndex: 1,
            pageSize: 20);

        // Assert
        Assert.NotEmpty(pagedOrdered.Items);
        var names = pagedOrdered.Items.Select(p => p.Name).ToList();
        var sortedNames = names.OrderBy(n => n).ToList();
        Assert.Equal(sortedNames, names);
    }

    [Fact]
    public async Task GetPagedListAsync_ShouldCalculatePagesCorrectly()
    {
        // Arrange
        var repository = CreateProductRepository();

        // Act
        var page1 = await repository.GetPagedListAsync(pageIndex: 1, pageSize: 10);
        var page2 = await repository.GetPagedListAsync(pageIndex: 2, pageSize: 10);

        // Assert
        Assert.Equal(1, page1.PageIndex);
        Assert.Equal(2, page2.PageIndex);
        Assert.Equal(page1.TotalCount, page2.TotalCount);

        if (page1.TotalCount > 10)
        {
            var page1Ids = page1.Items.Select(p => p.Id).ToHashSet();
            var page2Ids = page2.Items.Select(p => p.Id).ToHashSet();
            Assert.NotEqual(page1Ids, page2Ids);
        }
    }

    #endregion

    #region Testes do PagedList.Empty()

    [Fact]
    public void Empty_ShouldCreateEmptyPagedList()
    {
        // Act
        var empty = PagedList.Empty<Product>();

        // Assert
        Assert.NotNull(empty);
        Assert.Empty(empty.Items);
        Assert.Equal(0, empty.PageIndex);
        Assert.Equal(0, empty.PageSize);
        Assert.Equal(0, empty.TotalCount);
    }

    [Fact]
    public void Empty_ShouldBeConvertibleToAnotherType()
    {
        // Arrange
        var emptyProducts = PagedList.Empty<Product>();

        // Act
        var emptyDtos = PagedList.From<ProductDto, Product>(
            source: emptyProducts,
            converter: products => products.Select(p => new ProductDto { Id = p.Id, Name = p.Name, Price = p.Price, Category = p.Category, Stock = p.Stock }));

        // Assert
        Assert.NotNull(emptyDtos);
        Assert.Empty(emptyDtos.Items);
    }

    #endregion

    #region Testes de Metadados

    [Fact]
    public async Task PagedList_ShouldCalculateTotalPagesCorrectly()
    {
        // Arrange
        var repository = CreateProductRepository();
        var totalCount = await CreateContext().Products.CountAsync();
        var pageSize = 7;

        // Act
        var paged = await repository.GetPagedListAsync(pageIndex: 1, pageSize: pageSize);

        // Assert
        var expectedTotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        Assert.Equal(expectedTotalPages, paged.TotalPages);
    }

    #endregion
}

#region DTOs

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public int Stock { get; set; }
}

public class ProductSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool InStock { get; set; }
    public int DaysOld { get; set; }
}

public class ProductBasicDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class ProductDisplayDto
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DisplayPrice { get; set; } = string.Empty;
    public bool ShowBadge { get; set; }
}

public class OrderSummaryDto
{
    public string OrderId { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public int TotalItems { get; set; }
    public decimal TotalAmount { get; set; }
}

#endregion
