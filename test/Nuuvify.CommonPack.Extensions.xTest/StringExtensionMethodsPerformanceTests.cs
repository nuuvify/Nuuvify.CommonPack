using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.xTest.Fixtures;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace Nuuvify.CommonPack.Extensions.xTest;

public class StringExtensionMethodsPerformanceTests
{
    private readonly ITestOutputHelper _output;

    public StringExtensionMethodsPerformanceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveSpecialChars_ShouldPerformWellWithLargeText()
    {
        // Arrange
        var largeText = string.Join("", StringTestDataFixture.GenerateTextsWithSpecialChars(1000));
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = largeText.RemoveSpecialChars();
        stopwatch.Stop();

        // Assert
        Assert.NotNull(result);
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Performance issue: took {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"RemoveSpecialChars took {stopwatch.ElapsedMilliseconds}ms for {largeText.Length} characters");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetLettersAndNumbersOnly_ShouldPerformWellWithLargeText()
    {
        // Arrange
        var largeText = string.Join("", StringTestDataFixture.GenerateTextsWithNumbers(1000));
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = largeText.GetLettersAndNumbersOnly();
        stopwatch.Stop();

        // Assert
        Assert.NotNull(result);
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Performance issue: took {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"GetLettersAndNumbersOnly took {stopwatch.ElapsedMilliseconds}ms for {largeText.Length} characters");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveAccent_ShouldPerformWellWithLargeText()
    {
        // Arrange
        var largeText = string.Join(" ", StringTestDataFixture.GenerateTextsWithAccents(1000));
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = largeText.RemoveAccent();
        stopwatch.Stop();

        // Assert
        Assert.NotNull(result);
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Performance issue: took {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"RemoveAccent took {stopwatch.ElapsedMilliseconds}ms for {largeText.Length} characters");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetNumbers_ShouldPerformWellWithLargeText()
    {
        // Arrange
        var largeText = string.Join("", StringTestDataFixture.GenerateTextsWithNumbers(1000));
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = largeText.GetNumbers();
        stopwatch.Stop();

        // Assert
        Assert.NotNull(result);
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Performance issue: took {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"GetNumbers took {stopwatch.ElapsedMilliseconds}ms for {largeText.Length} characters");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RemoveCharsKeepDiacritics_ShouldPerformWellWithLargeText()
    {
        // Arrange
        var largeText = string.Join(" ", StringTestDataFixture.GenerateTextsWithAccents(1000));
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = largeText.RemoveCharsKeepDiacritics();
        stopwatch.Stop();

        // Assert
        Assert.NotNull(result);
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Performance issue: took {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"RemoveCharsKeepDiacritics took {stopwatch.ElapsedMilliseconds}ms for {largeText.Length} characters");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetReturnMessageWithoutRn_ShouldPerformWellWithLargeJsonText()
    {
        // Arrange
        var largeJsonText = string.Join("\r\n", StringTestDataFixture.GenerateJsonMessages(1000));
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = largeJsonText.GetReturnMessageWithoutRn();
        stopwatch.Stop();

        // Assert
        Assert.NotNull(result);
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Performance issue: took {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"GetReturnMessageWithoutRn took {stopwatch.ElapsedMilliseconds}ms for {largeJsonText.Length} characters");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void AllMethodsCombined_ShouldMaintainConsistency()
    {
        // Arrange
        var testTexts = StringTestDataFixture.GenerateTextsWithSpecialChars(10).ToList();

        foreach (var text in testTexts)
        {
            // Act
            var removedSpecial = text.RemoveSpecialChars();
            var lettersNumbers = text.GetLettersAndNumbersOnly();
            var unicode = text.GetUnicodeChars();
            var withDiacritics = text.RemoveCharsKeepDiacritics();
            var withoutAccents = text.RemoveAccent();
            var numbersOnly = text.GetNumbers();
            var titleCase = text.ToTitleCase();
            var withoutRn = text.GetReturnMessageWithoutRn();

            // Assert - All should execute without exceptions
            Assert.NotNull(removedSpecial);
            Assert.NotNull(lettersNumbers);
            Assert.NotNull(unicode);
            Assert.NotNull(withDiacritics);
            Assert.NotNull(withoutAccents);
            Assert.NotNull(numbersOnly);
            Assert.NotNull(withoutRn);

            // Unicode and LettersNumbers should have same effect
            Assert.Equal(lettersNumbers, unicode);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ConsistencyTest_RemoveCharsKeepChars_WithVariousInputs()
    {
        // Arrange
        var testData = new[]
        {
            ("Test@123#World", new[] { "@", "#" }, "Test@123#World"),
            ("Test@123#World", new[] { "@" }, "Test@123World"),
            ("Test@123#World", new string[0], "Test123World"),
            ("Test-123_World", new[] { "-", "_" }, "Test-123_World"),
            ("Test+123=World", new[] { "+", "=" }, "Test123=World") // + should be ignored
        };

        foreach (var (text, keepChars, expected) in testData)
        {
            // Act
            var result = text.RemoveCharsKeepChars(keepChars);

            // Assert
            if (keepChars.Contains("+"))
            {
                // Special case: + should be ignored
                Assert.DoesNotContain("+", result);
            }
            else
            {
                Assert.Equal(expected, result);
            }
        }
    }
}
