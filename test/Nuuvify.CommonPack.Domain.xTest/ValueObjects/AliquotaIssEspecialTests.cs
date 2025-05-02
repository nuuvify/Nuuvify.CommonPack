using Nuuvify.CommonPack.Domain.ValueObjects;
using Nuuvify.CommonPack.Extensions.Brazil;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects;

public class AliquotaIssEspecialTests
{
    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(AliquotaIssEspecial))]
    [InlineData(10.11, EnumSimNao.Sim, "S", true)]
    [InlineData(10.11, EnumSimNao.Nao, "N", true)]
    public void AliquotaIssEspecialTest(double aliquota, EnumSimNao temAliquota, string temAliquotaRetorno, bool retorno)
    {
        var _retorno = new AliquotaIssEspecial(aliquota, temAliquota);

        Assert.Equal(retorno, _retorno.IsValid());
        Assert.Equal(temAliquotaRetorno, _retorno.TemAliquotaIssEspecialParaContribuinteOptantePeloSimples);

    }
}
