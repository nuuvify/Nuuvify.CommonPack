using Nuuvify.CommonPack.Extensions.Implementation;
using Xunit;

namespace Nuuvify.CommonPack.Extensions.xTest;

public class StringExtensionMethodsFixedTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveSpecialChars_ShouldKeepAsciiRange()
    {
        // Arrange - The method keeps ASCII chars x20-x5F and x61-x7E
        var input = "Hello@#$%World!&*()123 ABC abc";

        // Act
        var result = input.RemoveSpecialChars();

        // Assert - All characters in the input are within the allowed ASCII ranges
        Assert.Equal(input, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveSpecialChars_ShouldRemoveUnicodeChars()
    {
        // Arrange - Unicode characters outside ASCII range
        var input = "Helloñáéíóú";
        var expected = "Hello";

        // Act
        var result = input.RemoveSpecialChars();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetReturnMessageWithoutRn_ShouldFormatSucessoCorrectly()
    {
        // Arrange
        var jsonMessage = "{\"sucesso\": true, \"data\": \"test\"}";

        // Act
        var result = jsonMessage.GetReturnMessageWithoutRn();

        // Assert
        // The method only removes spaces before the colon in the success property
        Assert.Equal("{\"sucesso\":true, \"data\": \"test\"}", result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetReturnMessageWithoutRn_ShouldFormatSuccessCorrectly()
    {
        // Arrange
        var jsonMessage = "{\"success\": true, \"data\": \"test\"}";

        // Act
        var result = jsonMessage.GetReturnMessageWithoutRn();

        // Assert
        Assert.Equal("{\"success\":true, \"data\": \"test\"}", result);
    }
}
