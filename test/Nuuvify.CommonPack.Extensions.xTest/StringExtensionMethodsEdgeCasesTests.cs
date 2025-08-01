using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.xTest.Fixtures;
using Xunit;

namespace Nuuvify.CommonPack.Extensions.xTest;

public class StringExtensionMethodsEdgeCasesTests
{
    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void RemoveSpecialChars_ShouldHandleEmptyOrNullInput(string input)
    {
        // Act
        var result = input?.RemoveSpecialChars() ?? string.Empty;

        // Assert
        Assert.True(string.IsNullOrEmpty(result));
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GetLettersAndNumbersOnly_ShouldHandleEmptyOrNullInput(string input)
    {
        // Act
        var result = input?.GetLettersAndNumbersOnly() ?? string.Empty;

        // Assert
        Assert.True(string.IsNullOrEmpty(result));
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GetUnicodeChars_ShouldHandleEmptyOrNullInput(string input)
    {
        // Act
        var result = input?.GetUnicodeChars() ?? string.Empty;

        // Assert
        Assert.True(string.IsNullOrEmpty(result));
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void RemoveCharsKeepDiacritics_ShouldHandleEmptyOrNullInput(string input)
    {
        // Act
        var result = input?.RemoveCharsKeepDiacritics() ?? string.Empty;

        // Assert
        Assert.True(string.IsNullOrEmpty(result));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveSpecialChars_ShouldWorkWithGeneratedData()
    {
        // Arrange
        var testData = StringTestDataFixture.GenerateTextsWithSpecialChars(3);

        foreach (var text in testData)
        {
            // Act
            var result = text.RemoveSpecialChars();

            // Assert
            Assert.DoesNotContain("@", result);
            Assert.DoesNotContain("#", result);
            Assert.DoesNotContain("$", result);
            Assert.DoesNotContain("%", result);
            Assert.DoesNotContain("!", result);
            Assert.DoesNotContain("&", result);
            Assert.DoesNotContain("*", result);
            Assert.DoesNotContain("(", result);
            Assert.DoesNotContain(")", result);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetLettersAndNumbersOnly_ShouldWorkWithGeneratedData()
    {
        // Arrange
        var testData = StringTestDataFixture.GenerateTextsWithNumbers(3);

        foreach (var text in testData)
        {
            // Act
            var result = text.GetLettersAndNumbersOnly();

            // Assert
            Assert.True(result.All(char.IsLetterOrDigit));
            Assert.True(result.Length <= text.Length);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveAccent_ShouldWorkWithGeneratedAccentedData()
    {
        // Arrange
        var testData = StringTestDataFixture.GenerateTextsWithAccents(3);

        foreach (var text in testData)
        {
            // Act
            var result = text.RemoveAccent();

            // Assert
            Assert.DoesNotContain("ç", result);
            Assert.DoesNotContain("ã", result);
            Assert.DoesNotContain("á", result);
            Assert.DoesNotContain("à", result);
            Assert.DoesNotContain("â", result);
            Assert.DoesNotContain("é", result);
            Assert.DoesNotContain("ê", result);
            Assert.DoesNotContain("í", result);
            Assert.DoesNotContain("ó", result);
            Assert.DoesNotContain("ô", result);
            Assert.DoesNotContain("õ", result);
            Assert.DoesNotContain("ú", result);
            Assert.DoesNotContain("ü", result);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetNumbers_ShouldWorkWithGeneratedNumberData()
    {
        // Arrange
        var testData = StringTestDataFixture.GenerateOnlyNumbersCollection(3);

        foreach (var numberText in testData)
        {
            // Act
            var result = numberText.GetNumbers();

            // Assert
            Assert.Equal(numberText, result);
            Assert.True(result.All(char.IsDigit));
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ToTitleCase_ShouldWorkWithGeneratedMixedCaseData()
    {
        // Arrange
        var testData = StringTestDataFixture.GenerateTextsWithMixedCase(3);

        foreach (var text in testData)
        {
            // Act
            var result = text.ToTitleCase();

            // Assert
            Assert.NotNull(result);
            Assert.True(char.IsUpper(result[0]));
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetReturnMessageWithoutRn_ShouldWorkWithGeneratedControlCharData()
    {
        // Arrange
        var testData = StringTestDataFixture.GenerateTextsWithControlChars(3);

        foreach (var text in testData)
        {
            // Act
            var result = text.GetReturnMessageWithoutRn();

            // Assert
            Assert.DoesNotContain("\r", result);
            Assert.DoesNotContain("\n", result);
            Assert.DoesNotContain("\t", result);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetReturnMessageWithoutRn_ShouldWorkWithGeneratedJsonMessages()
    {
        // Arrange
        var testData = StringTestDataFixture.GenerateJsonMessages(3);

        foreach (var jsonMessage in testData)
        {
            // Act
            var result = jsonMessage.GetReturnMessageWithoutRn();

            // Assert
            Assert.DoesNotContain("\r", result);
            Assert.DoesNotContain("\n", result);
            Assert.DoesNotContain("\t", result);
            Assert.Contains("success", result, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("Test", 0, 2, "Te")]
    [InlineData("Test", 2, 2, "st")]
    [InlineData("Test", 0, 10, "Test")]
    [InlineData("Test", 10, 2, "")]
    [InlineData("Test", 2, 10, "st")]
    public void SubstringNotNull_ShouldHandleVariousParameters(string input, int start, int length, string expected)
    {
        // Act
        var result = input.SubstringNotNull(start, length);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("test", "Test")]
    [InlineData("TEST", "Test")]
    [InlineData("tEST", "Test")]
    [InlineData("hello world", "Hello World")]
    [InlineData("HELLO WORLD", "Hello World")]
    public void ToTitleCase_ShouldConvertCorrectly(string input, string expected)
    {
        // Act
        var result = input.ToTitleCase();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("ABC123DEF456", "123456")]
    [InlineData("ABCDEF", "")]
    [InlineData("123456", "123456")]
    [InlineData("1A2B3C", "123")]
    public void GetNumbers_ShouldExtractCorrectNumbers(string input, string expected)
    {
        // Act
        var result = input.GetNumbers();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("São Paulo", "Sao Paulo")]
    [InlineData("José da Silva", "Jose da Silva")]
    [InlineData("Coração", "Coracao")]
    [InlineData("Você", "Voce")]
    [InlineData("Ação", "Acao")]
    public void RemoveAccent_ShouldRemoveAccentsCorrectly(string input, string expected)
    {
        // Act
        var result = input.RemoveAccent();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveCharsKeepChars_ShouldEscapeRegexChars()
    {
        // Arrange
        var textWithRegexChars = "Test.123$Text^456";
        var keepChars = new[] { ".", "$", "^" };
        var expected = "Test.123$Text^456";

        // Act
        var result = textWithRegexChars.RemoveCharsKeepChars(keepChars);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void AllMethods_ShouldHandleUnicodeCharacters()
    {
        // Arrange
        var unicodeText = "Hello αβγ 123 δεζ World";

        // Act & Assert - Should not throw exceptions
        Assert.DoesNotContain("null", unicodeText.RemoveSpecialChars() ?? "null");
        Assert.DoesNotContain("null", unicodeText.GetLettersAndNumbersOnly() ?? "null");
        Assert.DoesNotContain("null", unicodeText.GetUnicodeChars() ?? "null");
        Assert.DoesNotContain("null", unicodeText.RemoveCharsKeepDiacritics() ?? "null");
        Assert.DoesNotContain("null", unicodeText.RemoveAccent() ?? "null");
        Assert.DoesNotContain("null", unicodeText.GetNumbers() ?? "null");
        Assert.DoesNotContain("null", unicodeText.SubstringNotNull(0, 5) ?? "null");
        Assert.DoesNotContain("null", unicodeText.ToTitleCase() ?? "null");
        Assert.DoesNotContain("null", unicodeText.GetReturnMessageWithoutRn() ?? "null");
    }
}
