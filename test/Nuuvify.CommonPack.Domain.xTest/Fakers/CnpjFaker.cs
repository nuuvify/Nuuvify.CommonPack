using Bogus;
using Nuuvify.CommonPack.Domain.ValueObjects;

namespace Nuuvify.CommonPack.Domain.xTest.Fakers;

public static class CnpjFaker
{
    private static readonly int[] Pesos1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
    private static readonly int[] Pesos2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
    private static readonly Faker s_faker = new("pt_BR");

    public static Cnpj Generate()
    {
        var cnpjString = GerarCnpjAlfanumericoValido();
        return new Cnpj(cnpjString);
    }

    public static IList<Cnpj> Generate(int count)
    {
        return Enumerable.Range(0, count).Select(_ => Generate()).ToList();
    }

    public static string GerarCnpjAlfanumericoValido()
    {
        const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var base12 = new char[12];

        for (var i = 0; i < 12; i++)
        {
            base12[i] = s_faker.PickRandom(chars.AsEnumerable());
        }

        var soma1 = 0;
        for (var i = 0; i < 12; i++)
            soma1 += (base12[i] - 48) * Pesos1[i];

        var resto1 = soma1 % 11;
        var dv1 = resto1 < 2 ? 0 : 11 - resto1;

        var soma2 = 0;
        for (var i = 0; i < 12; i++)
            soma2 += (base12[i] - 48) * Pesos2[i];
        soma2 += dv1 * Pesos2[12];

        var resto2 = soma2 % 11;
        var dv2 = resto2 < 2 ? 0 : 11 - resto2;

        return new string(base12) + dv1.ToString() + dv2.ToString();
    }

    public static string GerarCnpjNumericoValido()
    {
        var base12 = new char[12];

        for (var i = 0; i < 12; i++)
        {
            base12[i] = (char)('0' + s_faker.Random.Int(0, 9));
        }

        // Garantir que nao sao todos iguais
        if (base12.All(c => c == base12[0]))
            base12[11] = base12[0] == '0' ? '1' : '0';

        var soma1 = 0;
        for (var i = 0; i < 12; i++)
            soma1 += (base12[i] - 48) * Pesos1[i];

        var resto1 = soma1 % 11;
        var dv1 = resto1 < 2 ? 0 : 11 - resto1;

        var soma2 = 0;
        for (var i = 0; i < 12; i++)
            soma2 += (base12[i] - 48) * Pesos2[i];
        soma2 += dv1 * Pesos2[12];

        var resto2 = soma2 % 11;
        var dv2 = resto2 < 2 ? 0 : 11 - resto2;

        return new string(base12) + dv1.ToString() + dv2.ToString();
    }
}
