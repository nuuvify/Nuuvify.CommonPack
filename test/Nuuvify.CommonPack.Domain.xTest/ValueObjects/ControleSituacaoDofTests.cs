using Nuuvify.CommonPack.Domain.ValueObjects;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects;

[Trait("Category", "Unit")]
public class ControleSituacaoDofTests
{
    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(ControleSituacaoDof))]
    [InlineData("S", true)]
    [InlineData("N", true)]
    [InlineData("I", true)]
    [InlineData("D", true)]
    [InlineData("s", true)]
    [InlineData("n", true)]
    [InlineData("X", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void ControleSituacaoDof_Validade_DeveRetornarEsperado(string situacao, bool esperado)
    {
        var controle = new ControleSituacaoDof(situacao);
        Assert.Equal(esperado, controle.IsValid());
    }

    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(ControleSituacaoDof))]
    [InlineData("S")]
    [InlineData("N")]
    [InlineData("I")]
    [InlineData("D")]
    public void ControleSituacaoDof_Valido_CodigoPreenchido(string situacao)
    {
        var controle = new ControleSituacaoDof(situacao);
        Assert.True(controle.IsValid());
        Assert.NotNull(controle.Codigo);
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(ControleSituacaoDof))]
    public void ControleSituacaoDof_Invalido_AdicionaNotificacao()
    {
        var controle = new ControleSituacaoDof("X");
        Assert.False(controle.IsValid());
        Assert.NotEmpty(controle.Notifications);
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(ControleSituacaoDof))]
    public void ControleSituacaoDof_ToString_RetornaCodigo()
    {
        var controle = new ControleSituacaoDof("S");
        Assert.Equal("S", controle.ToString());
    }
}
