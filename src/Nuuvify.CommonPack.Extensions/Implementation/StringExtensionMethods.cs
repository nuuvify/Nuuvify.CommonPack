using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nuuvify.CommonPack.Extensions.Implementation;

public static partial class StringExtensionMethods
{

    private static readonly CultureInfo s_cultureInfo = CultureInfo.InvariantCulture;

    /// <summary>
    /// Remove caracteres especiais, veja tabela asii coluna Hex
    /// <para>Inicia em 20 até 7E</para>
    /// See http://www.asciitable.com/
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string RemoveSpecialChars(this string text)
    {
        if (text is null)
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        return Regex.Replace(text, "[^\x20-\x5F\x61-\x7E]+", "");
    }

    public static string GetLettersAndNumbersOnly(this string text)
    {
        var newText = Regex.Replace(text, "[^a-zA-Z0-9]", "");
        return newText;
    }

    /// <summary>
    /// Mantens os caracteres unicode entre "[^\u0030-\u0039\u0041-\u005A\u0061-\u007A]+", veja tabela:
    /// See http://unicode-table.com/en/
    /// <para>Retorna apenas letras e numeros, tem o mesmo efeito que GetLettersAndNumbersOnly()</para>
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string GetUnicodeChars(this string text)
    {
        var newText = Regex.Replace(text, "[^\u0030-\u0039\u0041-\u005A\u0061-\u007A]+", "");
        return newText;
    }

    /// <summary>
    /// Remove caracteres diferentes de letras e numeros, mantendo acentos e marcas de letras (Diacritics)
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string RemoveCharsKeepDiacritics(this string text)
    {
        var newText = Regex.Replace(text, "[^\\p{L}\\p{Nd}]", "");
        return newText;
    }

    /// <summary>
    /// Remove caracteres diferentes de letras e numeros, mantendo acentos e marcas de letras (Diacritics), e também os chars passados no parametro
    /// <para>O caracter + não será mantido</para>
    /// </summary>
    /// <param name="text"></param>
    /// <param name="keepChars">Se não for informado nenhum caractere, o text retornara original</param>
    /// <returns></returns>
    public static string RemoveCharsKeepChars(this string text, params string[] keepChars)
    {
        if (keepChars is null) return text;

        var regex = new StringBuilder("[^\\p{L}\\p{Nd}");

        foreach (var character in keepChars)
        {

            if (character == "-")
            {
                _ = regex.Append("-");
            }
            else if (character != "+")
            {
                _ = regex.Append(Regex.Escape(character));
            }
        }

        _ = regex.Append("]+");
        var newRegex = regex.ToString();

        var newText = Regex.Replace(text, newRegex, "");
        return newText;
    }

    /// <summary>
    /// Remove todos os acentos de um text
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string RemoveAccent(this string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        const string comAcentos = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇç";
        const string semAcentos = "AAAAAAaaaaaEEEEeeeeIIIIiiiiOOOOOoooooUUUuuuuCc";

        for (var i = 0; i < comAcentos.Length; i++)
            text = text.Replace(comAcentos[i].ToString(), semAcentos[i].ToString());

        return text;
    }

    /// <summary>
    /// Obtem apenas os numeros contidos em uma string
    /// </summary>
    /// <param name="text">Testo contendo numeros</param>
    /// <returns>Retorna apenas os numeros do text informado</returns>
    public static string GetNumbers(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;
        else
        {
            var textTrim = text.Trim();
            return new String(textTrim.Where(Char.IsDigit).ToArray());
        }
    }

    /// <summary>
    /// Retorna a parte do text entre o start e length, se text for null, retorna Empty,
    /// se o text for menor que o start, retorna a diferença, nunca retornara null
    /// </summary>
    /// <param name="value"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string SubstringNotNull(this string value, int start, int length)
    {
        var newValue = string.Empty;
        int qtdCut;

        if (string.IsNullOrWhiteSpace(value))
            return newValue;

        if (start < 0 || length < 0)
            return newValue;

        qtdCut = start + length;

        if (qtdCut > value.Length && start <= value.Length)
        {
            newValue = value.Substring(start);
        }
        else if (qtdCut > value.Length && start > value.Length)
        {
            return newValue;
        }
        else
        {
            newValue = value.Substring(start, length);
        }

        return newValue;
    }

    public static string ToTitleCase(this string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        var _textLower = s_cultureInfo.TextInfo.ToLower(text);
        var _text = s_cultureInfo.TextInfo.ToTitleCase(_textLower);

        return _text;
    }

    /// <summary>
    /// Retorna uma string removendo \r e \n e \t
    /// Caracteres equivalentes: .Replace('\"', '\u200B');
    /// </summary>
    /// <param name="returnMessage"></param>
    /// <param name="otherCharToRemove">Informe qual caractere deseja retirar da mensagem</param>
    /// <returns></returns>
    public static string GetReturnMessageWithoutRn(this string returnMessage, string otherCharToRemove = null)
    {

        var message = returnMessage.Replace("\r", "")
                                   .Replace("\n", "")
                                   .Replace("\t", "");

        if (!string.IsNullOrWhiteSpace(otherCharToRemove))
        {
            message = message.Replace(otherCharToRemove, "");
        }

        var initSuccess = message.IndexOf(value: "\"success\":", startIndex: 0, comparisonType: StringComparison.InvariantCultureIgnoreCase);
        var colonIndex = -1;

        if (initSuccess < 0)
        {
            initSuccess = message.IndexOf(value: "\"sucesso\":", startIndex: 0, comparisonType: StringComparison.InvariantCultureIgnoreCase);
            if (initSuccess >= 0)
            {
                colonIndex = initSuccess + 9; // Position of colon after "sucesso"
            }
        }
        else
        {
            colonIndex = initSuccess + 9; // Position of colon after "success"
        }

        if (initSuccess < 0 || colonIndex < 0)
        {
            return message;
        }

        // Check if there's a space after the colon and remove it
        if (colonIndex + 1 < message.Length && message[colonIndex + 1] == ' ')
        {
            return message.Remove(colonIndex + 1, 1);
        }

        return message;
    }

}
