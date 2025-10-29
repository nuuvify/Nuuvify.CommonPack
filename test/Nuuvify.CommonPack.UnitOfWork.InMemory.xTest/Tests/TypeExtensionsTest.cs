using Nuuvify.CommonPack.UnitOfWork.Abstraction.Extensions;
using Xunit;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Tests;

[Trait("Category", "Unit")]
public sealed class TypeExtensionsTest
{
    [Fact]
    public void FieldName_WithDescendingPrefix_ShouldRemovePrefix()
    {
        // Arrange
        var field = "D-NumeroFatura";

        // Act
        var result = field.FieldName();

        // Assert
        Assert.Equal("NumeroFatura", result);
    }

    [Fact]
    public void FieldName_WithAscendingPrefix_ShouldRemovePrefix()
    {
        // Arrange
        var field = "A-NumeroFatura";

        // Act
        var result = field.FieldName();

        // Assert
        Assert.Equal("NumeroFatura", result);
    }

    [Fact]
    public void FieldName_WithoutPrefix_ShouldReturnOriginal()
    {
        // Arrange
        var field = "NumeroFatura";

        // Act
        var result = field.FieldName();

        // Assert
        Assert.Equal("NumeroFatura", result);
    }

    [Fact]
    public void FieldName_WithWhitespace_ShouldTrim()
    {
        // Arrange
        var field1 = "  NumeroFatura  ";
        var field2 = "  A-NumeroFatura  ";
        var field3 = "  D-NumeroFatura  ";

        // Act
        var result1 = field1.FieldName();
        var result2 = field2.FieldName();
        var result3 = field3.FieldName();

        // Assert
        Assert.Equal("NumeroFatura", result1);
        Assert.Equal("NumeroFatura", result2);
        Assert.Equal("NumeroFatura", result3);
    }

    [Fact]
    public void IsDescending_WithDescendingPrefix_ShouldReturnTrue()
    {
        // Arrange
        var field = "D-NumeroFatura";

        // Act
        var result = field.IsDescending();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDescending_WithAscendingPrefix_ShouldReturnFalse()
    {
        // Arrange
        var field = "A-NumeroFatura";

        // Act
        var result = field.IsDescending();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsDescending_WithoutPrefix_ShouldReturnFalse()
    {
        // Arrange
        var field = "NumeroFatura";

        // Act
        var result = field.IsDescending();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Fields_WithCommaSeparatedString_ShouldSplitCorrectly()
    {
        // Arrange
        var fieldsString = "A-NumeroFatura,D-Observacao,Id";

        // Act
        var result = fieldsString.Fields();

        // Assert
        Assert.Equal(3, result.Length);
        Assert.Equal("A-NumeroFatura", result[0]);
        Assert.Equal("D-Observacao", result[1]);
        Assert.Equal("Id", result[2]);
    }

    [Fact]
    public void Fields_WithSingleField_ShouldReturnArray()
    {
        // Arrange
        var fieldsString = "NumeroFatura";

        // Act
        var result = fieldsString.Fields();

        // Assert
        _ = Assert.Single(result);
        Assert.Equal("NumeroFatura", result[0]);
    }

    [Fact]
    public void Fields_WithEmptyString_ShouldReturnArrayWithEmptyString()
    {
        // Arrange
        var fieldsString = "";

        // Act
        var result = fieldsString.Fields();

        // Assert
        _ = Assert.Single(result);
        Assert.Equal("", result[0]);
    }

    [Fact]
    public void StartsWith_WithMultiplePrefixes_ShouldReturnTrueForAnyMatch()
    {
        // Arrange
        var text = "D-NumeroFatura";

        // Act
        var result = text.StartsWith("A-", "D-", "X-");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void StartsWith_WithNoMatchingPrefixes_ShouldReturnFalse()
    {
        // Arrange
        var text = "NumeroFatura";

        // Act
        var result = text.StartsWith("A-", "D-", "X-");

        // Assert
        Assert.False(result);
    }
}