using Nuuvify.CommonPack.Extensions.Brazil;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects;


public class AtivarFuncaoTests
{
    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(AtivarFuncaoVo))]
    [InlineData("x", false)]
    [InlineData("0", false)]
    [InlineData("1", false)]
    [InlineData(null, false)]
    [InlineData("s", true)]
    [InlineData("n", true)]
    [InlineData("Ativar", true)]
    [InlineData("Desativar", true)]
    public void AtivarFuncaoTest(string codigo, bool result)
    {

        var situacao = new AtivarFuncaoVo(codigo);
        Assert.Equal(result, situacao.IsValid());
    }

    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(AtivarFuncaoVo))]
    [InlineData("n", "N", 0)]
    [InlineData("s", "S", 1)]
    [InlineData("Desativar", "N", 0)]
    [InlineData("Ativar", "S", 1)]
    public void AtivarFuncaoCodigoValido(string codigo, string result, int hashCodigo)
    {

        var situacao = new AtivarFuncaoVo(codigo);
        Assert.Equal(result, situacao.ToString());
        Assert.Equal(hashCodigo, situacao.GetHashCode());
    }

}
