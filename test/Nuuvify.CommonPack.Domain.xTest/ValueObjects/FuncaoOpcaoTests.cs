using Nuuvify.CommonPack.Domain.ValueObjects;
using Nuuvify.CommonPack.Extensions.Implementation;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects;

public class FuncaoOpcaoTests
{
    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(FuncaoOpcao))]
    [InlineData("x", false)]
    [InlineData("0", false)]
    [InlineData("1", true)]
    [InlineData("2", true)]
    [InlineData("3", true)]
    [InlineData("4", true)]
    [InlineData("5", true)]
    [InlineData("6", true)]
    [InlineData(null, false)]
    public void FuncaoOpcaoTest(string codigo, bool result)
    {
        var testResult = codigo.IsEnum<FuncaoOpcao>(out int resultEnum);

        Assert.Equal(result, testResult);
        _ = Assert.IsType<int>(resultEnum);
    }

    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(FuncaoOpcao))]
    [InlineData("BLOCO", 1)]
    [InlineData("SEMACENTOS", 2)]
    [InlineData("TRACE", 3)]
    [InlineData("LIMPAR", 4)]
    [InlineData("SEMCOMMIT", 5)]
    [InlineData("SEMCONTROLE", 6)]
    public void FuncaoOpcaoCodigoValido(string codigo, int hashCodigo)
    {

        _ = codigo.IsEnum<FuncaoOpcao>(out int resultEnum);

        Assert.Equal(hashCodigo, resultEnum);
    }

}
