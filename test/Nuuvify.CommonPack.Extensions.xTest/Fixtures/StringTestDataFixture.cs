using Bogus;
using System.Globalization;

namespace Nuuvify.CommonPack.Extensions.xTest.Fixtures;

public static class StringTestDataFixture
{
    private static readonly Faker s_faker = new("pt_BR");

    public static string GenerateTextWithSpecialChars()
    {
        return $"{s_faker.Lorem.Word()}@#$%{s_faker.Lorem.Word()}!&*()";
    }

    public static IEnumerable<string> GenerateTextsWithSpecialChars(int count = 5)
    {
        return Enumerable.Range(0, count).Select(_ => GenerateTextWithSpecialChars());
    }

    public static string GenerateTextWithAccents()
    {
        var accentedWords = new[]
        {
            "açúcar", "coração", "pão", "ação", "informação",
            "você", "até", "João", "São", "José"
        };
        return s_faker.PickRandom(accentedWords) + " " + s_faker.Lorem.Word();
    }

    public static IEnumerable<string> GenerateTextsWithAccents(int count = 5)
    {
        return Enumerable.Range(0, count).Select(_ => GenerateTextWithAccents());
    }

    public static string GenerateTextWithNumbers()
    {
        return $"{s_faker.Lorem.Word()}{s_faker.Random.Number(100, 999)}{s_faker.Lorem.Letter()}";
    }

    public static IEnumerable<string> GenerateTextsWithNumbers(int count = 5)
    {
        return Enumerable.Range(0, count).Select(_ => GenerateTextWithNumbers());
    }

    public static string GenerateOnlyNumbers()
    {
        return s_faker.Random.Number(100000, 999999).ToString(CultureInfo.InvariantCulture);
    }

    public static IEnumerable<string> GenerateOnlyNumbersCollection(int count = 5)
    {
        return Enumerable.Range(0, count).Select(_ => GenerateOnlyNumbers());
    }

    public static string GenerateTextWithMixedCase()
    {
        var word = s_faker.Lorem.Word();
        return $"{word.ToUpper(CultureInfo.InvariantCulture)}{word.ToLower(CultureInfo.InvariantCulture)}{word}";
    }

    public static IEnumerable<string> GenerateTextsWithMixedCase(int count = 5)
    {
        return Enumerable.Range(0, count).Select(_ => GenerateTextWithMixedCase());
    }

    public static string GenerateTextWithControlChars()
    {
        return $"{s_faker.Lorem.Word()}\r\n\t{s_faker.Lorem.Word()}";
    }

    public static IEnumerable<string> GenerateTextsWithControlChars(int count = 5)
    {
        return Enumerable.Range(0, count).Select(_ => GenerateTextWithControlChars());
    }

    public static string GenerateUnicodeText()
    {
        return $"{s_faker.Lorem.Word()}αβγδε{s_faker.Lorem.Word()}";
    }

    public static IEnumerable<string> GenerateUnicodeTexts(int count = 5)
    {
        return Enumerable.Range(0, count).Select(_ => GenerateUnicodeText());
    }

    public static string GenerateJsonMessage()
    {
        return $"{{\"success\": true, \"message\": \"{s_faker.Lorem.Sentence()}\", \"data\": {s_faker.Random.Number()}}}";
    }

    public static IEnumerable<string> GenerateJsonMessages(int count = 5)
    {
        return Enumerable.Range(0, count).Select(_ => GenerateJsonMessage());
    }
}
