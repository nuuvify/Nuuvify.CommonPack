using Nuuvify.CommonPack.Domain.ValueObjects;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects;

public class NacionalidadeFiscalTests
{
    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(NacionalidadeFiscal))]
    [InlineData("x", null, int.MaxValue, false)]
    [InlineData("0", null, int.MaxValue, false)]
    [InlineData("kkk", null, int.MaxValue, false)]
    [InlineData(null, null, int.MaxValue, false)]
    [InlineData("nacional", null, int.MaxValue, false)]
    [InlineData("NACIONAL", null, int.MaxValue, false)]
    [InlineData("estrangeirosemcpfcnpj", null, int.MaxValue, false)]
    [InlineData("estrangeirocomcpfcnpj", null, int.MaxValue, false)]
    [InlineData("N", "N", 0, true)]
    [InlineData("S", "S", 1, true)]
    [InlineData("C", "C", 2, true)]
    [InlineData("n", "N", 0, true)]
    [InlineData("s", "S", 1, true)]
    [InlineData("c", "C", 2, true)]

    public void NacionalidadeFiscalTest(string situacao, string situacaoRetorno, int hashCodigo, bool retorno)
    {

        var _teste = new NacionalidadeFiscal(situacao);

        Assert.Equal(retorno, _teste.IsValid());
        Assert.Equal(situacaoRetorno, _teste.ToString());
        Assert.Equal(hashCodigo, _teste.GetHashCode());

    }
}
