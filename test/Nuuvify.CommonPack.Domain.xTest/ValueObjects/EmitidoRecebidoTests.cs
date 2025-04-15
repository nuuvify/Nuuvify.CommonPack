using Nuuvify.CommonPack.Domain.ValueObjects;
using Nuuvify.CommonPack.Extensions.Implementation;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects;


public class EmitidoRecebidoTests
{
    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(EnumEmitidoRecebido))]
    [InlineData("x", false, int.MaxValue)]
    [InlineData(null, false, int.MaxValue)]
    [InlineData("e", false, int.MaxValue)]
    [InlineData("r", false, int.MaxValue)]
    [InlineData("0", true, 0)]
    [InlineData("1", true, 1)]
    [InlineData("Emitido", true, 0)]
    [InlineData("Recebido", true, 1)]
    public void EmitidoRecebidoTest(string textCode, bool enumIsTrue, int enumCode)
    {

        var testResult = textCode.IsEnum<EnumEmitidoRecebido>(out int resultEnum);

        Assert.Equal(testResult, enumIsTrue);
        Assert.Equal(enumCode, resultEnum);
    }

}
