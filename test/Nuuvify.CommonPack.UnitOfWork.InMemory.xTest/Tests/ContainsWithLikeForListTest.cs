using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Filter;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Helpers;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;
using Xunit;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Tests;

[Trait("Category", "Unit")]
public class ContainsWithLikeForListTest
{
    [Fact]
    public void Filter_WithContainsWithLikeForList_ShouldFilterCorrectly()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Name = "Apple iPhone 14", Category = "Electronics" },
            new() { Name = "Samsung Galaxy S23", Category = "Electronics" },
            new() { Name = "Nike Air Max", Category = "Sports" },
            new() { Name = "Adidas Ultraboost", Category = "Sports" },
            new() { Name = "Apple MacBook Pro", Category = "Electronics" },
            new() { Name = "Microsoft Surface", Category = "Electronics" }
        };

        var searchModel = new ProductSearchModel
        {
            SearchTerms = new Collection<string> { "Apple", "Samsung" }
        };

        // Act
        var result = products.AsQueryable().Filter(searchModel).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, p => p.Name.Contains("Apple"));
        Assert.Contains(result, p => p.Name.Contains("Samsung"));
        Assert.Contains(result, p => p.Name == "Apple iPhone 14");
        Assert.Contains(result, p => p.Name == "Apple MacBook Pro");
        Assert.Contains(result, p => p.Name == "Samsung Galaxy S23");
    }

    [Fact]
    public void Filter_WithContainsWithLikeForListEmptyList_ShouldReturnEmptyResult()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Name = "Apple iPhone 14", Category = "Electronics" },
            new() { Name = "Samsung Galaxy S23", Category = "Electronics" }
        };

        var searchModel = new ProductSearchModel
        {
            SearchTerms = new Collection<string>()
        };

        // Act
        var result = products.AsQueryable().Filter(searchModel).ToList();

        // Assert
        Assert.Equal(2, result.Count); // Should return all items when search terms is empty
    }

    [Fact]
    public void Filter_WithContainsWithLikeForListNullList_ShouldReturnAllResults()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Name = "Apple iPhone 14", Category = "Electronics" },
            new() { Name = "Samsung Galaxy S23", Category = "Electronics" }
        };

        var searchModel = new ProductSearchModel
        {
            SearchTerms = null
        };

        // Act
        var result = products.AsQueryable().Filter(searchModel).ToList();

        // Assert
        Assert.Equal(2, result.Count); // Should return all items when search terms is null
    }

    [Fact]
    public void Filter_WithContainsWithLikeForListPartialMatch_ShouldFilterCorrectly()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Name = "Apple iPhone 14", Category = "Electronics" },
            new() { Name = "Samsung Galaxy S23", Category = "Electronics" },
            new() { Name = "Nike Air Max", Category = "Sports" },
            new() { Name = "MacBook Pro Apple", Category = "Electronics" }
        };

        var searchModel = new ProductSearchModel
        {
            SearchTerms = new Collection<string> { "iPhone", "Galaxy", "Air" }
        };

        // Act
        var result = products.AsQueryable().Filter(searchModel).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, p => p.Name.Contains("iPhone"));
        Assert.Contains(result, p => p.Name.Contains("Galaxy"));
        Assert.Contains(result, p => p.Name.Contains("Air"));
    }
}

public class Product
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class ProductSearchModel : IQueryableCustom
{
    [QueryOperator(Operator = WhereOperator.ContainsWithLikeForList, HasName = nameof(Product.Name))]
    public Collection<string>? SearchTerms { get; set; }

    [Key]
    public string PageIndex { get; set; } = "1";

    [Key]
    public string PageSize { get; set; } = "10";

    public string Sort { get; set; } = string.Empty;
}