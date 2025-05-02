using Nuuvify.CommonPack.Mediator.Implementation;

namespace Nuuvify.CommonPack.Extensions.Brazil;


public class Endereco : NotifiableR
{

    protected Endereco() { }

    public Endereco(
        string tipoLogradouro,
        string logradouro,
        string codigoMunicipio,
        string nomeMunicipio,
        string uf,
        string bairro,
        string cep,
        string numero,
        string complemento,
        string siglaPais)
    {

        DefinirTipoLogradouro(tipoLogradouro);
        DefinirLogradouro(logradouro);
        DefinirCodigoMunicipio(codigoMunicipio);
        DefinirNomeMunicipio(nomeMunicipio);
        DefinirUF(uf);
        DefinirBairro(bairro);
        DefinirCep(cep);
        DefinirNumero(numero);
        DefinirComplemento(complemento);
        DefinirSiglaPais(siglaPais);

    }

    public string TipoLogradouro { get; private set; }
    public string Logradouro { get; private set; }
    public string CodigoMunicipio { get; private set; }
    public string NomeMunicipio { get; private set; }
    public string UF { get; private set; }
    public string Bairro { get; private set; }
    public string Cep { get; private set; }
    public string Numero { get; private set; }
    public string Complemento { get; private set; }
    public string SiglaPais { get; private set; }

    private void DefinirTipoLogradouro(string tipoLogradouro)
    {
        var validacao = Notifications.Count;

        _ = new ValidationConcernR<Endereco>(this)
            .AssertHasMaxLength(x => tipoLogradouro, MaxTipoLogradouro);

        if (validacao.Equals(Notifications.Count))
            TipoLogradouro = tipoLogradouro?.ToUpper();
    }

    private void DefinirLogradouro(string logradouro)
    {
        var validacao = Notifications.Count;

        _ = new ValidationConcernR<Endereco>(this)
            .AssertHasMinLength(x => logradouro, MinLogradouro)
            .AssertHasMaxLength(x => logradouro, MaxLogradouro);

        if (validacao.Equals(Notifications.Count))
            Logradouro = StringExtensionMethods.ToTitleCase(logradouro);
    }

    private void DefinirCodigoMunicipio(string codigo)
    {
        var validacao = Notifications.Count;

        var codigoMunicipio = codigo.GetNumbers();

        _ = new ValidationConcernR<Endereco>(this)
            .AssertHasMinLength(x => codigoMunicipio, MinCodigoMunicipio)
            .AssertHasMaxLength(x => codigoMunicipio, MaxCodigoMunicipio);

        if (validacao.Equals(Notifications.Count))
            CodigoMunicipio = codigoMunicipio;
    }

    private void DefinirNomeMunicipio(string nomeMunicipio)
    {
        var validacao = Notifications.Count;

        _ = new ValidationConcernR<Endereco>(this)
            .AssertHasMinLength(x => nomeMunicipio, MinNomeMunicipio)
            .AssertHasMaxLength(x => nomeMunicipio, MaxNomeMunicipio);

        if (validacao.Equals(Notifications.Count))
            NomeMunicipio = StringExtensionMethods.ToTitleCase(nomeMunicipio);
    }

    private void DefinirUF(string uf)
    {
        var validacao = Notifications.Count;

        _ = new ValidationConcernR<Endereco>(this)
            .AssertFixedLength(x => uf, MaxUF);

        if (validacao.Equals(Notifications.Count))
            UF = uf.ToUpper();
    }

    private void DefinirBairro(string bairro)
    {
        var validacao = Notifications.Count;

        _ = new ValidationConcernR<Endereco>(this)
            .AssertHasMaxLength(x => bairro, MaxBairro);

        if (validacao.Equals(Notifications.Count))
            Bairro = StringExtensionMethods.ToTitleCase(bairro);
    }

    private void DefinirCep(string cep)
    {
        var validacao = Notifications.Count;
        cep = StringExtensionMethods.GetNumbers(cep);

        _ = new ValidationConcernR<Endereco>(this)
            .AssertFixedLength(x => cep, MaxCep);

        if (validacao.Equals(Notifications.Count))
            Cep = cep;
    }
    private void DefinirNumero(string numeroLogradouro)
    {
        var validacao = Notifications.Count;

        _ = new ValidationConcernR<Endereco>(this)
            .AssertHasMinLength(x => numeroLogradouro, MinNumero)
            .AssertHasMaxLength(x => numeroLogradouro, MaxNumero);

        if (validacao.Equals(Notifications.Count))
            Numero = numeroLogradouro;
    }

    private void DefinirComplemento(string complemento)
    {
        var validacao = Notifications.Count;

        _ = new ValidationConcernR<Endereco>(this)
            .AssertHasMaxLength(x => complemento, MaxComplemento);

        if (validacao.Equals(Notifications.Count) && (!(complemento is null)))
        {
            Complemento = StringExtensionMethods.ToTitleCase(complemento);
        }
    }

    private void DefinirSiglaPais(string siglaPais)
    {
        var validacao = Notifications.Count;

        _ = new ValidationConcernR<Endereco>(this)
            .AssertHasMinLength(x => siglaPais, MinSiglaPais)
            .AssertHasMaxLength(x => siglaPais, MaxSiglaPais);

        if (validacao.Equals(Notifications.Count))
            SiglaPais = siglaPais.ToUpper();
    }

    public static int MinTipoLogradouro { get; set; } = 0;
    public static int MaxTipoLogradouro { get; set; } = 10;

    public static int MinLogradouro { get; set; } = 3;
    public static int MaxLogradouro { get; set; } = 80;

    public static int MinCodigoMunicipio { get; set; } = 3;
    public static int MaxCodigoMunicipio { get; set; } = 8;

    public static int MinNomeMunicipio { get; set; } = 3;
    public static int MaxNomeMunicipio { get; set; } = 30;

    public static int MinBairro { get; set; } = 0;
    public static int MaxBairro { get; set; } = 20;

    public static int MinNumero { get; set; } = 1;
    public static int MaxNumero { get; set; } = 8;

    public static int MinComplemento { get; set; } = 0;
    public static int MaxComplemento { get; set; } = 20;

    public static int MinSiglaPais { get; set; } = 2;
    public static int MaxSiglaPais { get; set; } = 3;

    public static int MaxUF { get; set; } = 2;
    public static int MaxCep { get; set; } = 8;

}
