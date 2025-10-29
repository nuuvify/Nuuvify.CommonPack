using Nuuvify.CommonPack.UnitOfWork.Abstraction.Enums;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Extensions;
using Xunit;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Tests;

[Trait("Category", "Unit")]
public class MethodNameExtensionsTest
{
    [Theory]
    [InlineData(MethodName.ToUpper, "ToUpper")]
    [InlineData(MethodName.StartsWith, "StartsWith")]
    [InlineData(MethodName.Contains, "Contains")]
    public void ToMethodString_WithValidEnumValues_ShouldReturnCorrectString(MethodName methodName, string expected)
    {
        // Act
        var result = methodName.ToMethodString();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToMethodString_WithInvalidEnum_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var invalidMethodName = (MethodName)999;

        // Act & Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => invalidMethodName.ToMethodString());
    }
}