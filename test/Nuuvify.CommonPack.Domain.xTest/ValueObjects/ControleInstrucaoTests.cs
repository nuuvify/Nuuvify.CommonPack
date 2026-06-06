using Nuuvify.CommonPack.Domain.ValueObjects;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects;

[Trait("Category", "Unit")]
public class ControleInstrucaoTests
{
    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(ControleInstrucao))]
    [InlineData("I", true)]
    [InlineData("A", true)]
    [InlineData("E", true)]
    [InlineData("M", true)]
    [InlineData("i", true)]
    [InlineData("a", true)]
    [InlineData("X", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void ControleInstrucao_Validade_DeveRetornarEsperado(string situacao, bool esperado)
    {
        var controle = new ControleInstrucao(situacao);
        Assert.Equal(esperado, controle.IsValid());
    }

    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(ControleInstrucao))]
    [InlineData("I")]
    [InlineData("A")]
    [InlineData("E")]
    [InlineData("M")]
    public void ControleInstrucao_Valido_CodigoPreenchido(string situacao)
    {
        var controle = new ControleInstrucao(situacao);
        Assert.True(controle.IsValid());
        Assert.NotNull(controle.Codigo);
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(ControleInstrucao))]
    public void ControleInstrucao_Invalido_AdicionaNotificacao()
    {
        var controle = new ControleInstrucao("X");
        Assert.False(controle.IsValid());
        Assert.NotEmpty(controle.Notifications);
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(ControleInstrucao))]
    public void ControleInstrucao_ToString_RetornaCodigo()
    {
        var controle = new ControleInstrucao("I");
        Assert.Equal("I", controle.ToString());
    }
}
