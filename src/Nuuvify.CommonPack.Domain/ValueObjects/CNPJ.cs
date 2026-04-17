using System.Runtime.CompilerServices;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain.ValueObjects;

public class Cnpj : NotifiableR
{

    protected Cnpj() { }

    public Cnpj(string numero)
    {
        Validar(numero);
    }

    /// <summary>
    /// Cnpj da empresa sem mascara, alfanumerico (12 caracteres [A-Z0-9] + 2 digitos verificadores numericos)
    /// </summary>
    /// <example>61064911000177</example>
    /// <example>12ABC3450001XX</example>
    public string Codigo { get; private set; }

    private void Validar(string numero)
    {

        if (!ValidarCodigo(numero))
        {
            AddNotification(nameof(Cnpj), "Codigo invalido");
        }

        _ = new ValidationConcernR<Cnpj>(this)
            .AssertFixedLength(x => x.Codigo, MaxCNPJ);

        if (!IsValid())
            Codigo = null;

    }

    /// <summary>
    /// Converte um caractere alfanumerico para seu valor no calculo do CNPJ.
    /// Digitos 0-9 retornam 0-9, letras A-Z retornam 17-42 (ASCII - 48).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CharParaValor(char c)
    {
        return c - 48;
    }

    /// <summary>
    /// Remove apenas os caracteres de pontuacao do CNPJ (ponto, barra, hifen),
    /// preservando letras. Converte para uppercase.
    /// </summary>
    private static string RemoverPontuacao(string cnpj)
    {
        Span<char> buffer = stackalloc char[cnpj.Length];
        var pos = 0;
        for (var i = 0; i < cnpj.Length; i++)
        {
            var c = cnpj[i];
            if (c != '.' && c != '/' && c != '-')
            {
                buffer[pos++] = char.ToUpperInvariant(c);
            }
        }
        return new string(buffer[..pos]);
    }

    /// <summary>
    /// Valida se todos os 14 caracteres sao iguais (CNPJ invalido por repeticao).
    /// </summary>
    private static bool TodosCaracteresIguais(string cnpj)
    {
        var primeiro = cnpj[0];
        for (var i = 1; i < cnpj.Length; i++)
        {
            if (cnpj[i] != primeiro)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Valida o formato dos caracteres do CNPJ:
    /// 12 primeiros devem ser [A-Z0-9], 2 ultimos devem ser digitos [0-9].
    /// </summary>
    private static bool ValidarFormatoCaracteres(string cnpj)
    {
        for (var i = 0; i < 12; i++)
        {
            var c = cnpj[i];
            if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z')))
                return false;
        }

        return cnpj[12] >= '0' && cnpj[12] <= '9'
            && cnpj[13] >= '0' && cnpj[13] <= '9';
    }

    /// <summary>
    /// Calcula os dois digitos verificadores usando Modulo 11 com conversao ASCII.
    /// </summary>
    private static string CalcularDigitosVerificadores(string cnpj)
    {
        var multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        var soma = 0;
        for (var i = 0; i < 12; i++)
            soma += CharParaValor(cnpj[i]) * multiplicador1[i];

        var resto = soma % 11;
        var primeiroDv = resto < 2 ? 0 : 11 - resto;

        soma = 0;
        for (var i = 0; i < 12; i++)
            soma += CharParaValor(cnpj[i]) * multiplicador2[i];
        soma += primeiroDv * multiplicador2[12];

        resto = soma % 11;
        var segundoDv = resto < 2 ? 0 : 11 - resto;

        return $"{primeiroDv}{segundoDv}";
    }

    private bool ValidarCodigo(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
        {
            this.Codigo = null;
            return false;
        }

        cnpj = cnpj.Trim();
        cnpj = RemoverPontuacao(cnpj);

        if (cnpj.Length != MaxCNPJ || !ValidarFormatoCaracteres(cnpj) || TodosCaracteresIguais(cnpj))
        {
            this.Codigo = null;
            return false;
        }

        var digito = CalcularDigitosVerificadores(cnpj);
        var valido = cnpj.EndsWith(digito, StringComparison.Ordinal);

        if (valido)
            this.Codigo = cnpj;

        return valido;
    }

    public const int MaxCNPJ = 14;

    /// <summary>
    /// Formatar uma string Cnpj com mascara
    /// </summary>
    /// <returns>string Cnpj formatada no padrao XX.XXX.XXX/XXXX-XX</returns>
    /// <example>Recebe '71266534000102' Devolve '71.266.534/0001-02'</example>
    /// <example>Recebe '12ABC3450001XX' Devolve '12.ABC.345/0001-XX'</example>
    public string Mascara()
    {
        if (Codigo == null)
            return null;

        return $"{Codigo[..2]}.{Codigo[2..5]}.{Codigo[5..8]}/{Codigo[8..12]}-{Codigo[12..14]}";
    }

    public override string ToString()
    {
        return Codigo?.ToString();
    }
}
