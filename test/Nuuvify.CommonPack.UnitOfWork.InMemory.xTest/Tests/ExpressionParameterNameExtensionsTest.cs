using Nuuvify.CommonPack.UnitOfWork.Abstraction.Enums;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Extensions;
using Xunit;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Tests;

[Trait("Category", "Unit")]
public class ExpressionParameterNameExtensionsTest
{
    [Theory]
    [InlineData(ExpressionParameterName.Model, "model")]
    [InlineData(ExpressionParameterName.Entity, "entity")]
    [InlineData(ExpressionParameterName.Item, "item")]
    [InlineData(ExpressionParameterName.P, "p")]
    [InlineData(ExpressionParameterName.Param, "param")]
    [InlineData(ExpressionParameterName.X, "x")]
    public void ToParameterString_WithValidEnumValues_ShouldReturnCorrectString(ExpressionParameterName parameterName, string expected)
    {
        // Act
        var result = parameterName.ToParameterString();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToParameterString_WithInvalidEnum_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var invalidParameterName = (ExpressionParameterName)999;

        // Act & Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => invalidParameterName.ToParameterString());
    }
}