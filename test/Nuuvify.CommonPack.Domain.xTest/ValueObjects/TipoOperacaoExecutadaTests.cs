using Nuuvify.CommonPack.Domain.ValueObjects;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects;

[Trait("Category", "Unit")]
public class TipoOperacaoExecutadaTests
{
    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(TipoOperacaoExecutadaVo))]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(99, false)]
    [InlineData(-1, false)]
    public void TipoOperacaoExecutada_Validade_DeveRetornarEsperado(int numero, bool esperado)
    {
        var tipo = new TipoOperacaoExecutadaVo(numero);
        Assert.Equal(esperado, tipo.IsValid());
    }

    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(TipoOperacaoExecutadaVo))]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void TipoOperacaoExecutada_Valido_CodigoPreenchido(int numero)
    {
        var tipo = new TipoOperacaoExecutadaVo(numero);
        Assert.True(tipo.IsValid());
        Assert.Equal(numero, tipo.Codigo);
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(TipoOperacaoExecutadaVo))]
    public void TipoOperacaoExecutada_Invalido_AdicionaNotificacao()
    {
        var tipo = new TipoOperacaoExecutadaVo(99);
        Assert.False(tipo.IsValid());
        Assert.NotEmpty(tipo.Notifications);
    }
}
