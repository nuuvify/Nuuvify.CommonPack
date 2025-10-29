using Moq;
using Nuuvify.CommonPack.UnitOfWork.Examples;
using Nuuvify.CommonPack.UnitOfWork.Integration.xTest.Fakers;
using Nuuvify.CommonPack.UnitOfWork.Integration.xTest.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Nuuvify.CommonPack.UnitOfWork.Integration.xTest.Tests;

/// <summary>
/// Integration tests for the ExamplesQueryOperators class using SQL Server in Docker via Testcontainers.
/// Tests query operators, sorting, filtering, and pagination functionalities with a real database.
/// </summary>
/// <remarks>
/// These tests use Testcontainers to run SQL Server in Docker, providing:
/// - ✅ Case-insensitive filtering (COLLATE SQL_Latin1_General_CP1_CI_AS)
/// - ✅ Real database constraints and triggers
/// - ✅ Behavior identical to production
/// - ✅ Automatic container cleanup after tests
/// 
/// Requirements: Docker must be installed and running.
/// </remarks>
[Trait("Category", "Integration")]
public sealed class ExamplesQueryOperatorsTest : IClassFixture<SqlServerDbContextFixture>, IDisposable
{
    private readonly ExampleDbContext _context;
    private readonly ExamplesQueryOperators _service;
    private readonly Mock<ILogger<ExamplesQueryOperators>> _loggerMock;

    public ExamplesQueryOperatorsTest(SqlServerDbContextFixture fixture, ITestOutputHelper output)
    {
        _context = fixture.CreateContext(output);
        _loggerMock = new Mock<ILogger<ExamplesQueryOperators>>();
        _service = new ExamplesQueryOperators(_context, _loggerMock.Object);
    }

    #region GetProductByNameAsync Tests

    [Fact]
    public async Task GetProductByNameAsync_WithMatchingName_ShouldReturnProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            ProductFaker.GenerateWithSpecificData("iPhone 15", "Eletrônicos", 5000m, 10),
            ProductFaker.GenerateWithSpecificData("iPhone 14", "Eletrônicos", 4000m, 15),
            ProductFaker.GenerateWithSpecificData("Samsung Galaxy", "Eletrônicos", 3500m, 20)
        };

        _context.Products.AddRange(products);
        _ = await _context.SaveChangesAsync();

        var filterModel = new ProductCaseInsensitiveFilterModel
        {
            NameIgnoreCase = "iPhone",
            PageIndex = 0,
            PageSize = 10
        };

        // Act
        var result = await _service.GetProductByNameAsync(filterModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item => Assert.Contains("iPhone", item.Name));
    }

    [Fact]
    public async Task GetProductByNameAsync_CaseInsensitive_ShouldMatchRegardlessOfCase()
    {
        // Arrange
        var products = new List<Product>
        {
            ProductFaker.GenerateWithSpecificData("PRODUTO MAIÚSCULO", "Eletrônicos", 100m, 5),
            ProductFaker.GenerateWithSpecificData("produto minúsculo", "Eletrônicos", 200m, 10)
        };

        _context.Products.AddRange(products);
        _ = await _context.SaveChangesAsync();

        var filterModel = new ProductCaseInsensitiveFilterModel
        {
            NameIgnoreCase = "produto",
            PageIndex = 0,
            PageSize = 10
        };

        // Act
        var result = await _service.GetProductByNameAsync(filterModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GetProductByNameAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var products = ProductFaker.Generate(25);
        _context.Products.AddRange(products);
        _ = await _context.SaveChangesAsync();

        var filterModel = new ProductCaseInsensitiveFilterModel
        {
            PageIndex = 2,
            PageSize = 10
        };

        // Act
        var result = await _service.GetProductByNameAsync(filterModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.PageIndex);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(5, result.Items.Count);
    }

    #endregion

    #region GetProductByNameWithSortAsync Tests

    [Fact]
    public async Task GetProductByNameWithSortAsync_ShouldSortByNameAndIsActive()
    {
        // Arrange
        var products = new List<Product>
        {
            ProductFaker.GenerateWithSpecificData("Zebra Product", "Eletrônicos", 100m, 5, isActive: true),
            ProductFaker.GenerateWithSpecificData("Alpha Product", "Eletrônicos", 200m, 10, isActive: true),
            ProductFaker.GenerateWithSpecificData("Beta Product", "Eletrônicos", 150m, 7, isActive: false)
        };

        _context.Products.AddRange(products);
        _ = await _context.SaveChangesAsync();

        var filterModel = new ProductCaseInsensitiveFilterModel
        {
            PageIndex = 0,
            PageSize = 10
        };

        // Act
        var result = await _service.GetProductByNameWithSortAsync(filterModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal("Alpha Product", result.Items[0].Name);
        Assert.Equal("Beta Product", result.Items[1].Name);
        Assert.Equal("Zebra Product", result.Items[2].Name);
    }

    #endregion

    #region GetProductByNameWithMultipleSortAsync Tests

    [Fact]
    public async Task GetProductByNameWithMultipleSortAsync_ShouldSortByMultipleFields()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var products = new List<Product>
        {
            new Product
            {
                Name = "Product A",
                Description = "Desc A",
                Category = "Cat1",
                Price = 100m,
                Stock = 10,
                IsActive = true,
                CreatedAt = now.AddDays(-10),
                LastUpdate = now.AddDays(-1)
            },
            new Product
            {
                Name = "Product B",
                Description = "Desc B",
                Category = "Cat1",
                Price = 200m,
                Stock = 20,
                IsActive = true,
                CreatedAt = now.AddDays(-5),
                LastUpdate = now.AddDays(-2)
            },
            new Product
            {
                Name = "Product C",
                Description = "Desc C",
                Category = "Cat1",
                Price = 150m,
                Stock = 15,
                IsActive = false,
                CreatedAt = now.AddDays(-3),
                LastUpdate = now
            }
        };

        _context.Products.AddRange(products);
        _ = await _context.SaveChangesAsync();

        var filterModel = new ProductCaseInsensitiveFilterModel
        {
            PageIndex = 0,
            PageSize = 10
        };

        // Act
        var result = await _service.GetProductByNameWithMultipleSortAsync(filterModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal("Product A", result.Items[0].Name);
    }

    #endregion

    #region GetProductsWithComplexFilterAsync Tests

    [Fact]
    public async Task GetProductsWithComplexFilterAsync_WithPriceRange_ShouldFilterCorrectly()
    {
        // Arrange
        var products = new List<Product>
        {
            ProductFaker.GenerateWithSpecificData("Product 1", "Eletrônicos", 50m, 10),
            ProductFaker.GenerateWithSpecificData("Product 2", "Eletrônicos", 150m, 20),
            ProductFaker.GenerateWithSpecificData("Product 3", "Eletrônicos", 250m, 30),
            ProductFaker.GenerateWithSpecificData("Product 4", "Eletrônicos", 350m, 40)
        };

        _context.Products.AddRange(products);
        _ = await _context.SaveChangesAsync();

        var filterModel = new ComplexProductFilterModel
        {
            MinPrice = 100m,
            MaxPrice = 300m,
            PageIndex = 0,
            PageSize = 10
        };

        // Act
        var result = await _service.GetProductsWithComplexFilterAsync(filterModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item => Assert.InRange(item.Price, 100m, 300m));
    }

    [Fact]
    public async Task GetProductsWithComplexFilterAsync_WithGlobalSearch_ShouldMatchAnyTerm()
    {
        // Arrange
        var products = new List<Product>
        {
            ProductFaker.GenerateWithSpecificData("iPhone 15", "Eletrônicos", 5000m, 10),
            ProductFaker.GenerateWithSpecificData("Samsung Galaxy", "Eletrônicos", 3500m, 20),
            ProductFaker.GenerateWithSpecificData("Xiaomi Redmi", "Eletrônicos", 1500m, 30),
            ProductFaker.GenerateWithSpecificData("Nokia Classic", "Eletrônicos", 500m, 5)
        };

        _context.Products.AddRange(products);
        _ = await _context.SaveChangesAsync();

        var filterModel = new ComplexProductFilterModel
        {
            GlobalTerms = new List<string> { "iPhone", "Samsung" },
            PageIndex = 0,
            PageSize = 10
        };

        // Act
        var result = await _service.GetProductsWithComplexFilterAsync(filterModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, p => p.Name.Contains("iPhone"));
        Assert.Contains(result.Items, p => p.Name.Contains("Samsung"));
    }

    [Fact]
    public async Task GetProductsWithComplexFilterAsync_WithCategory_ShouldFilterByCategory()
    {
        // Arrange
        var products = new List<Product>
        {
            ProductFaker.GenerateWithSpecificData("Product 1", "Eletrônicos", 100m, 10),
            ProductFaker.GenerateWithSpecificData("Product 2", "Roupas", 50m, 20),
            ProductFaker.GenerateWithSpecificData("Product 3", "Eletrônicos", 150m, 30),
            ProductFaker.GenerateWithSpecificData("Product 4", "Livros", 30m, 15)
        };

        _context.Products.AddRange(products);
        _ = await _context.SaveChangesAsync();

        var filterModel = new ComplexProductFilterModel
        {
            PrimaryCategory = "Eletrônicos",
            PageIndex = 0,
            PageSize = 10
        };

        // Act
        var result = await _service.GetProductsWithComplexFilterAsync(filterModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item => Assert.Equal("Eletrônicos", item.Category));
    }

    [Fact]
    public async Task GetProductsWithComplexFilterAsync_WithMinStock_ShouldFilterByStock()
    {
        // Arrange
        var products = new List<Product>
        {
            ProductFaker.GenerateWithSpecificData("Product 1", "Eletrônicos", 100m, 5),
            ProductFaker.GenerateWithSpecificData("Product 2", "Eletrônicos", 150m, 15),
            ProductFaker.GenerateWithSpecificData("Product 3", "Eletrônicos", 200m, 25),
            ProductFaker.GenerateWithSpecificData("Product 4", "Eletrônicos", 250m, 35)
        };

        _context.Products.AddRange(products);
        _ = await _context.SaveChangesAsync();

        var filterModel = new ComplexProductFilterModel
        {
            MinStock = 20,
            PageIndex = 0,
            PageSize = 10
        };

        // Act
        var result = await _service.GetProductsWithComplexFilterAsync(filterModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item => Assert.True(item.Stock >= 20));
    }

    [Fact]
    public async Task GetProductsWithComplexFilterAsync_ShouldOrderByPriceDescAndNameAsc()
    {
        // Arrange
        var products = new List<Product>
        {
            ProductFaker.GenerateWithSpecificData("Alpha", "Eletrônicos", 100m, 10),
            ProductFaker.GenerateWithSpecificData("Zeta", "Eletrônicos", 100m, 10),
            ProductFaker.GenerateWithSpecificData("Beta", "Eletrônicos", 200m, 20)
        };

        _context.Products.AddRange(products);
        _ = await _context.SaveChangesAsync();

        var filterModel = new ComplexProductFilterModel
        {
            PageIndex = 0,
            PageSize = 10
        };

        // Act
        var result = await _service.GetProductsWithComplexFilterAsync(filterModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal("Beta", result.Items[0].Name);
        Assert.Equal("Alpha", result.Items[1].Name);
        Assert.Equal("Zeta", result.Items[2].Name);
    }

    [Fact]
    public async Task GetProductsWithComplexFilterAsync_WithMultipleFilters_ShouldApplyAllFilters()
    {
        // Arrange
        var products = new List<Product>
        {
            ProductFaker.GenerateWithSpecificData("iPhone 15", "Eletrônicos", 5000m, 25),
            ProductFaker.GenerateWithSpecificData("iPhone 14", "Eletrônicos", 4000m, 15),
            ProductFaker.GenerateWithSpecificData("Samsung S23", "Eletrônicos", 4500m, 30),
            ProductFaker.GenerateWithSpecificData("Xiaomi 13", "Eletrônicos", 2500m, 40)
        };

        _context.Products.AddRange(products);
        _ = await _context.SaveChangesAsync();

        var filterModel = new ComplexProductFilterModel
        {
            GlobalTerms = new List<string> { "iPhone", "Samsung" },
            MinPrice = 3500m,
            MaxPrice = 5500m,
            MinStock = 20,
            PageIndex = 0,
            PageSize = 10
        };

        // Act
        var result = await _service.GetProductsWithComplexFilterAsync(filterModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, p => p.Name == "iPhone 15");
        Assert.Contains(result.Items, p => p.Name == "Samsung S23");
    }

    #endregion

    #region Pagination Metadata Tests

    [Fact]
    public async Task AllMethods_ShouldReturnCorrectPaginationMetadata()
    {
        // Arrange
        var products = ProductFaker.Generate(35);
        _context.Products.AddRange(products);
        _ = await _context.SaveChangesAsync();

        var filterModel = new ProductCaseInsensitiveFilterModel
        {
            PageIndex = 2,
            PageSize = 10
        };

        // Act
        var result = await _service.GetProductByNameAsync(filterModel);

        // Assert
        Assert.Equal(2, result.PageIndex);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(35, result.TotalCount);
        Assert.Equal(4, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
        Assert.Equal(10, result.Items.Count);
    }

    #endregion

    public void Dispose()
    {
        var allProducts = _context.Products.ToList();
        if (allProducts.Count > 0)
        {
            _context.Products.RemoveRange(allProducts);
            _ = _context.SaveChanges();
        }
        _context.Dispose();
    }
}
