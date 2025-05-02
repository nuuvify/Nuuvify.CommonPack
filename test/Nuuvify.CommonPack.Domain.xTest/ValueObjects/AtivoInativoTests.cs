using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects;

public class AtivoInativoTests
{
    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(AtivoInativo))]
    [InlineData("x", false)]
    [InlineData("0", false)]
    [InlineData("1", false)]
    [InlineData("2", false)]
    [InlineData(null, false)]
    [InlineData("a", true)]
    [InlineData("i", true)]
    [InlineData("n", true)]
    [InlineData("Ativo", true)]
    [InlineData("Inativo", true)]
    [InlineData("Ambos", true)]
    public void AtivoInativo(string codigo, bool result)
    {

        var situacao = new AtivoInativo(codigo);
        Assert.Equal(result, situacao.IsValid());
    }

    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(AtivoInativo))]
    [InlineData("a", "A", 0)]
    [InlineData("i", "I", 1)]
    [InlineData("n", "N", 2)]
    [InlineData("Ativo", "A", 0)]
    [InlineData("Inativo", "I", 1)]
    [InlineData("Ambos", "N", 2)]
    public void AtivoInativoCodigoValido(string codigo, string result, int hashCodigo)
    {

        var situacao = new AtivoInativo(codigo);
        Assert.Equal(result, situacao.ToString());
        Assert.Equal(hashCodigo, situacao.GetHashCode());
    }

}
