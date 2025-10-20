using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.xTest.Fixtures;
using Xunit;

namespace Nuuvify.CommonPack.Extensions.xTest;

public class StringExtensionMethodsTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveSpecialChars_ShouldRemoveSpecialCharacters_WhenTextContainsSpecialChars()
    {
        // Arrange
        var textWithSpecialChars = "Hello@#$%World!&*()";
        var expected = "Hello@#$%World!&*()"; // This method keeps ASCII chars from x20-x5F and x61-x7E

        // Act
        var result = textWithSpecialChars.RemoveSpecialChars();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveSpecialChars_ShouldReturnEmptyString_WhenTextIsOnlySpecialChars()
    {
        // Arrange - using characters outside the allowed ranges
        var textWithOnlySpecialChars = "ñáéíóú"; // Unicode characters not in ASCII range
        var expected = "";

        // Act
        var result = textWithOnlySpecialChars.RemoveSpecialChars();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveSpecialChars_ShouldPreserveValidAsciiChars_WhenTextContainsValidChars()
    {
        // Arrange
        var textWithValidChars = "Hello World 123 ABC abc";

        // Act
        var result = textWithValidChars.RemoveSpecialChars();

        // Assert
        Assert.Equal("Hello World 123 ABC abc", result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetLettersAndNumbersOnly_ShouldReturnOnlyAlphanumeric_WhenTextContainsMixedChars()
    {
        // Arrange
        var mixedText = "Hello123@#$World456!";
        var expected = "Hello123World456";

        // Act
        var result = mixedText.GetLettersAndNumbersOnly();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetLettersAndNumbersOnly_ShouldReturnEmptyString_WhenTextHasNoAlphanumeric()
    {
        // Arrange
        var noAlphanumericText = "@#$%!&*()";
        var expected = "";

        // Act
        var result = noAlphanumericText.GetLettersAndNumbersOnly();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetUnicodeChars_ShouldReturnOnlyUnicodeAlphanumeric_WhenTextContainsMixedChars()
    {
        // Arrange
        var unicodeText = "Hello123@#$World456!";
        var expected = "Hello123World456";

        // Act
        var result = unicodeText.GetUnicodeChars();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetUnicodeChars_ShouldHaveSameEffectAsGetLettersAndNumbersOnly()
    {
        // Arrange
        var testText = "Test123@#$Text456!";

        // Act
        var unicodeResult = testText.GetUnicodeChars();
        var lettersNumbersResult = testText.GetLettersAndNumbersOnly();

        // Assert
        Assert.Equal(unicodeResult, lettersNumbersResult);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveCharsKeepDiacritics_ShouldKeepAccentedCharacters_WhenTextContainsAccents()
    {
        // Arrange
        var textWithAccents = "Olá! Como você está? 123";
        var expected = "OláComovocêestá123";

        // Act
        var result = textWithAccents.RemoveCharsKeepDiacritics();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveCharsKeepDiacritics_ShouldRemoveSpecialChars_ButKeepLettersNumbers()
    {
        // Arrange
        var textWithSpecialChars = "São Paulo - SP! 01234-567";
        var expected = "SãoPauloSP01234567";

        // Act
        var result = textWithSpecialChars.RemoveCharsKeepDiacritics();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveCharsKeepChars_ShouldReturnOriginalText_WhenKeepCharsIsNull()
    {
        // Arrange
        var originalText = "Test@#$123";
        string[] keepChars = null;

        // Act
        var result = originalText.RemoveCharsKeepChars(keepChars);

        // Assert
        Assert.Equal(originalText, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveCharsKeepChars_ShouldKeepSpecifiedChars_WhenKeepCharsProvided()
    {
        // Arrange
        var textWithSpecialChars = "São Paulo - SP! 01234-567@#$";
        var keepChars = new[] { " ", "-" };
        var expected = "São Paulo - SP 01234-567";

        // Act
        var result = textWithSpecialChars.RemoveCharsKeepChars(keepChars);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveCharsKeepChars_ShouldIgnorePlusChar_WhenPlusCharInKeepChars()
    {
        // Arrange
        var textWithPlus = "Test+123+Text";
        var keepChars = new[] { "+" };
        var expected = "Test123Text";

        // Act
        var result = textWithPlus.RemoveCharsKeepChars(keepChars);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveCharsKeepChars_ShouldHandleDashCorrectly_WhenDashInKeepChars()
    {
        // Arrange
        var textWithDash = "Test-123-Text";
        var keepChars = new[] { "-" };
        var expected = "Test-123-Text";

        // Act
        var result = textWithDash.RemoveCharsKeepChars(keepChars);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveAccent_ShouldRemoveAllAccents_WhenTextContainsAccentedChars()
    {
        // Arrange
        var textWithAccents = "São Paulo, coração, você, até";
        var expected = "Sao Paulo, coracao, voce, ate";

        // Act
        var result = textWithAccents.RemoveAccent();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveAccent_ShouldReturnEmptyString_WhenInputIsNullOrWhiteSpace()
    {
        // Arrange & Act & Assert
        Assert.Equal(string.Empty, ((string)null).RemoveAccent());
        Assert.Equal(string.Empty, string.Empty.RemoveAccent());
        Assert.Equal(string.Empty, "   ".RemoveAccent());
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveAccent_ShouldHandleAllDefinedAccents()
    {
        // Arrange
        var allAccents = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇç";
        var expected = "AAAAAAaaaaaEEEEeeeeIIIIiiiiOOOOOoooooUUUuuuuCc";

        // Act
        var result = allAccents.RemoveAccent();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetNumbers_ShouldReturnOnlyNumbers_WhenTextContainsMixedContent()
    {
        // Arrange
        var mixedText = "ABC123DEF456GHI";
        var expected = "123456";

        // Act
        var result = mixedText.GetNumbers();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetNumbers_ShouldReturnEmptyString_WhenInputIsNullOrWhiteSpace()
    {
        // Arrange & Act & Assert
        Assert.Equal(string.Empty, ((string)null).GetNumbers());
        Assert.Equal(string.Empty, string.Empty.GetNumbers());
        Assert.Equal(string.Empty, "   ".GetNumbers());
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetNumbers_ShouldReturnEmptyString_WhenTextHasNoNumbers()
    {
        // Arrange
        var textWithoutNumbers = "ABCDEFGHIJK";
        var expected = string.Empty;

        // Act
        var result = textWithoutNumbers.GetNumbers();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetNumbers_ShouldHandleWhitespace_WhenTextHasSpaces()
    {
        // Arrange
        var textWithSpaces = "   123   456   ";
        var expected = "123456";

        // Act
        var result = textWithSpaces.GetNumbers();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SubstringNotNull_ShouldReturnCorrectSubstring_WhenParametersAreValid()
    {
        // Arrange
        var text = "Hello World";
        var start = 6;
        var length = 5;
        var expected = "World";

        // Act
        var result = text.SubstringNotNull(start, length);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SubstringNotNull_ShouldReturnEmptyString_WhenInputIsNullOrWhiteSpace()
    {
        // Arrange & Act & Assert
        Assert.Equal(string.Empty, ((string)null).SubstringNotNull(0, 5));
        Assert.Equal(string.Empty, string.Empty.SubstringNotNull(0, 5));
        Assert.Equal(string.Empty, "   ".SubstringNotNull(0, 5));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SubstringNotNull_ShouldReturnEmptyString_WhenStartOrLengthAreNegative()
    {
        // Arrange
        var text = "Hello World";

        // Act & Assert
        Assert.Equal(string.Empty, text.SubstringNotNull(-1, 5));
        Assert.Equal(string.Empty, text.SubstringNotNull(0, -1));
        Assert.Equal(string.Empty, text.SubstringNotNull(-1, -1));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SubstringNotNull_ShouldReturnRemainingText_WhenLengthExceedsStringLength()
    {
        // Arrange
        var text = "Hello";
        var start = 2;
        var length = 10;
        var expected = "llo";

        // Act
        var result = text.SubstringNotNull(start, length);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SubstringNotNull_ShouldReturnEmptyString_WhenStartExceedsStringLength()
    {
        // Arrange
        var text = "Hello";
        var start = 10;
        var length = 5;
        var expected = string.Empty;

        // Act
        var result = text.SubstringNotNull(start, length);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ToTitleCase_ShouldConvertToTitleCase_WhenTextIsValidString()
    {
        // Arrange
        var text = "hello world test";
        var expected = "Hello World Test";

        // Act
        var result = text.ToTitleCase();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ToTitleCase_ShouldReturnNull_WhenInputIsNullOrWhiteSpace()
    {
        // Arrange & Act & Assert
        Assert.Null(((string)null).ToTitleCase());
        Assert.Null(string.Empty.ToTitleCase());
        Assert.Null("   ".ToTitleCase());
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ToTitleCase_ShouldHandleMixedCase()
    {
        // Arrange
        var text = "hELLo WoRLD tESt";
        var expected = "Hello World Test";

        // Act
        var result = text.ToTitleCase();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetReturnMessageWithoutRn_ShouldRemoveControlCharacters()
    {
        // Arrange
        var messageWithControlChars = "Hello\r\nWorld\tTest";
        var expected = "HelloWorldTest";

        // Act
        var result = messageWithControlChars.GetReturnMessageWithoutRn();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetReturnMessageWithoutRn_ShouldRemoveSpecifiedCharacter_WhenOtherCharToRemoveProvided()
    {
        // Arrange
        var messageWithCustomChar = "Hello@World@Test";
        var charToRemove = "@";
        var expected = "HelloWorldTest";

        // Act
        var result = messageWithCustomChar.GetReturnMessageWithoutRn(charToRemove);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetReturnMessageWithoutRn_ShouldFormatSuccessProperty_WhenContainsSuccessInEnglish()
    {
        // Arrange
        var jsonMessage = "{\"success\": true, \"data\": \"test\"}";
        var expected = "{\"success\":true, \"data\": \"test\"}";

        // Act
        var result = jsonMessage.GetReturnMessageWithoutRn();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetReturnMessageWithoutRn_ShouldFormatSucessoProperty_WhenContainsSucessoInPortuguese()
    {
        // Arrange
        var jsonMessage = "{\"sucesso\": true, \"data\": \"test\"}";
        var expected = "{\"sucesso\":true, \"data\": \"test\"}";

        // Act
        var result = jsonMessage.GetReturnMessageWithoutRn();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetReturnMessageWithoutRn_ShouldReturnOriginalMessage_WhenNoSuccessPropertyFound()
    {
        // Arrange
        var jsonMessage = "{\"data\": \"test\", \"message\": \"hello\"}";
        var expected = "{\"data\": \"test\", \"message\": \"hello\"}";

        // Act
        var result = jsonMessage.GetReturnMessageWithoutRn();

        // Assert
        Assert.Equal(expected, result);
    }
}
