using Nuuvify.CommonPack.Domain.ValueObjects;
using Nuuvify.CommonPack.Extensions.Implementation;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects;

public class NotaFiscalTests
{
    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(NotaFiscal))]
    [InlineData("x", null, "2017-09-13", EnumSimNao.Nao, false)]
    [InlineData("0", null, "2017-09-13", EnumSimNao.Nao, false)]
    [InlineData("kkk", null, null, EnumSimNao.Nao, false)]
    [InlineData(null, null, null, EnumSimNao.Nao, false)]
    public void NotaFiscalInvalida(string numero, string serie, string data, EnumSimNao simNao, bool retorno)
    {

        _ = DateTime.TryParse(data, out DateTime emissao);
        var nota = new NotaFiscal(numero, serie, emissao, simNao);

        Assert.Equal(retorno, nota.IsValid());
        Assert.Null(nota.Numero);
        Assert.Null(nota.Serie);
        Assert.Equal(DateTime.MinValue, nota.Emissao);
        Assert.Null(nota.NotaEletronica);

    }

    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(NotaFiscal))]
    [InlineData("1234", "25A", "2017-09-13", "N", true)]
    [InlineData("1234", "25A", "2017-09-13", "S", true)]
    public void NotaFiscalValida(string numero, string serie, string data, string situacao, bool retorno)
    {
        _ = DateTime.TryParse(data, out DateTime emissao);
        var retornoDescricao = situacao.GetCodeEnumByDescription<EnumSimNao>();
        var simNao = retornoDescricao.ToUpperInvariantNotNull() == "SIM"
            ? EnumSimNao.Sim
            : EnumSimNao.Nao;

        var nota = new NotaFiscal(numero, serie, emissao, simNao);

        Assert.Equal(retorno, nota.IsValid());
        Assert.Equal(numero, nota.Numero);
        Assert.Equal(serie, nota.Serie);
        Assert.Equal(emissao, nota.Emissao);
        Assert.Equal(situacao, nota.NotaEletronica);

    }

}
