using Nuuvify.CommonPack.Domain.ValueObjects;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects;

[Trait("Category", "Unit")]
public class ProdutorRuralTests
{
    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(ProdutorRural))]
    [InlineData(EnumSimNao.Sim, "123456789012", true)]
    [InlineData(EnumSimNao.Nao, null, true)]
    [InlineData(EnumSimNao.Nao, "", true)]
    [InlineData(EnumSimNao.Sim, "1234567890123", false)]
    public void ProdutorRural_Validade_DeveRetornarEsperado(EnumSimNao simNao, string cei, bool esperado)
    {
        var produtor = new ProdutorRural(simNao, cei);
        Assert.Equal(esperado, produtor.IsValid());
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(ProdutorRural))]
    public void ProdutorRural_Sim_IndicadorSim()
    {
        var produtor = new ProdutorRural(EnumSimNao.Sim, "123456");
        Assert.Equal("S", produtor.IndicadorProdutorRural);
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(ProdutorRural))]
    public void ProdutorRural_Nao_IndicadorNao()
    {
        var produtor = new ProdutorRural(EnumSimNao.Nao, null);
        Assert.Equal("N", produtor.IndicadorProdutorRural);
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(ProdutorRural))]
    public void ProdutorRural_Sim_CeiPreenchido()
    {
        var produtor = new ProdutorRural(EnumSimNao.Sim, "123456");
        Assert.True(produtor.IsValid());
        Assert.Equal("123456", produtor.CeiDoProdutorRural);
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(ProdutorRural))]
    public void ProdutorRural_Nao_CeiNulo()
    {
        var produtor = new ProdutorRural(EnumSimNao.Nao, "123456");
        Assert.True(produtor.IsValid());
        Assert.Null(produtor.CeiDoProdutorRural);
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(ProdutorRural))]
    public void ProdutorRural_Constantes_TemValoresEsperados()
    {
        Assert.Equal(0, ProdutorRural.minCei);
        Assert.Equal(12, ProdutorRural.maxCei);
    }
}
