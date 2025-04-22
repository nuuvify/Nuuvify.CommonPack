using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects;

public class DocumentoPessoaEstrangeiroTests
{
    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(DocumentoPessoaEstrangeiro))]
    [InlineData("fulano", "", null, "n", null, "", null, "N", false)]
    [InlineData(null, null, null, null, null, null, null, null, false)]
    [InlineData("fulano", "1234", "sp", "xxxx", null, null, null, null, false)]
    [InlineData("abc", "1234", "sp", "n", "abc", "1234", "SP", "N", true)]
    [InlineData("abc", "1234", "sp", "s", "abc", "1234", "SP", "S", true)]
    [InlineData("abc", "1234", "sp", "c", "abc", "1234", "SP", "C", true)]
    public void DocumentoPessoaEstrangeiroTest(string beneficiario, string identificacao, string provincia, string nacionalidade,
                                           string beneficiarioRetorno, string identificacaoRetorno, string provinciaRetorno, string nacionalidadeRetorno,
                                           bool retorno)
    {

        var nacionalidadeFiscal = new NacionalidadeFiscal(nacionalidade);
        Assert.Equal(nacionalidadeRetorno, nacionalidadeFiscal.Codigo);

        var _retorno = new DocumentoPessoaEstrangeiro(beneficiario, identificacao, provincia, nacionalidadeFiscal);

        Assert.Equal(retorno, _retorno.IsValid());
        Assert.Equal(beneficiarioRetorno, _retorno.CodigoInfBeneficiarioRendimento);
        Assert.Equal(identificacaoRetorno, _retorno.NumeroIdentificacaoFiscal);
        Assert.Equal(provinciaRetorno, _retorno.Provincia);
        Assert.Equal(nacionalidadeRetorno, _retorno.Nacionalidade);

    }
}
