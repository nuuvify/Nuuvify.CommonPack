using Nuuvify.CommonPack.Mediator.Implementation;

namespace Nuuvify.CommonPack.Extensions.Brazil;

public class Cpf : NotifiableR
{

    protected Cpf() { }

    public Cpf(string numero)
    {
        Validar(numero);
    }

    public string Codigo { get; private set; }

    private void Validar(string numero)
    {

        if (!ValidarCodigo(numero))
        {
            AddNotification(nameof(Cpf), "Codigo invalido");
        }

        _ = new ValidationConcernR<Cpf>(this)
            .AssertFixedLength(x => x.Codigo, MaxCpf);

        if (!IsValid())
            Codigo = null;

    }

    private bool ValidarCodigo(string cpf)
    {

        if (string.IsNullOrWhiteSpace(cpf))
        {
            Codigo = null;
            return false;
        }

        var multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        var multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        string tempCpf;
        string digito;

        int soma;
        int resto;

        cpf = cpf.GetNumbers();

        if (cpf.Length != MaxCpf)
        {
            Codigo = null;
            return false;
        }

        if (cpf.Substring(0, 1).Equals(cpf.Substring(1, 1)) && cpf.Substring(1, 1).Equals(cpf.Substring(2, 1)) &&
           cpf.Substring(2, 1).Equals(cpf.Substring(3, 1)) && cpf.Substring(3, 1).Equals(cpf.Substring(4, 1)) &&
           cpf.Substring(3, 1).Equals(cpf.Substring(5, 1)) && cpf.Substring(5, 1).Equals(cpf.Substring(6, 1)) &&
           cpf.Substring(6, 1).Equals(cpf.Substring(7, 1)) && cpf.Substring(7, 1).Equals(cpf.Substring(8, 1)) &&
           cpf.Substring(8, 1).Equals(cpf.Substring(9, 1)) && cpf.Substring(9, 1).Equals(cpf.Substring(10, 1)))
        {
            return false;
        }

        tempCpf = cpf.Substring(0, 9);
        soma = 0;

        for (int i = 0; i < 9; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
        resto = soma % 11;
        resto = resto < 2 ? 0 : 11 - resto;
        digito = resto.ToString();
        tempCpf += digito;
        soma = 0;
        for (int i = 0; i < 10; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
        resto = soma % 11;
        resto = resto < 2 ? 0 : 11 - resto;
        digito += resto.ToString();

        var valido = cpf.EndsWith(digito);

        if (valido)
            Codigo = cpf;

        return valido;
    }

    public const int MaxCpf = 11;

    /// <summary>
    /// Formatar uma string CPF
    /// </summary>
    /// <returns>string CPF formatada</returns>
    /// <example>Recebe '99999999999' Devolve '999.999.999-99'</example>
    public string Mascara()
    {
        return Codigo == null
            ? null
            : Convert.ToUInt64(Codigo).ToString(@"000\.000\.000\-00");
    }

    public override string ToString()
    {
        return Codigo?.ToString();
    }
}
