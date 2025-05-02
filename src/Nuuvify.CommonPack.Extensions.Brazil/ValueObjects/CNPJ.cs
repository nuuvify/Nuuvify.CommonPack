
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Mediator.Implementation;

namespace Nuuvify.CommonPack.Extensions.Brazil;


public class Cnpj : NotifiableR
{

    protected Cnpj() { }

    public Cnpj(string numero)
    {
        Validar(numero);
    }

    /// <summary>
    /// Cnpj da empresa sem mascara, apenas numeros
    /// </summary>
    /// <example>61064911000177</example>
    public string Codigo { get; private set; }

    private void Validar(string numero)
    {

        if (!ValidarCodigo(numero))
        {
            AddNotification(nameof(Cnpj), "Codigo invalido");
        }

        if (Codigo.Length != MaxCnpj)
        {
            AddNotification(nameof(Cnpj), $"Codigo deve ter {MaxCnpj} digitos");
        }

        if (!IsValid())
            Codigo = null;

    }

    private bool ValidarCodigo(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
        {
            this.Codigo = null;
            return false;
        }

        var multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int soma;
        int resto;
        string digito;
        string tempCnpj;

        cnpj = cnpj.Trim();
        cnpj = cnpj.GetNumbers();
        if (cnpj.Length != MaxCnpj)
        {
            this.Codigo = null;
            return false;
        }

        tempCnpj = cnpj.Substring(0, 12);
        soma = 0;

        for (int i = 0; i < 12; i++)
            soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];
        resto = soma % 11;

        resto = resto < 2 ? 0 : 11 - resto;

        digito = resto.ToString();
        tempCnpj += digito;
        soma = 0;

        for (int i = 0; i < 13; i++)
            soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

        resto = soma % 11;

        resto = resto < 2 ? 0 : 11 - resto;

        digito += resto.ToString();

        var valido = cnpj.EndsWith(digito);

        if (valido)
            this.Codigo = cnpj;

        return valido;
    }

    public const int MaxCnpj = 14;

    /// <summary>
    /// Formatar uma string Cnpj
    /// </summary>
    /// <returns>string Cnpj formatada</returns>
    /// <example>Recebe '99999999999999' Devolve '99.999.999/9999-99'</example>
    public string Mascara()
    {
        return Codigo == null ? null : Convert.ToUInt64(Codigo).ToString(@"00\.000\.000\/0000\-00");
    }

    public override string ToString()
    {
        return Codigo?.ToString();
    }
}
