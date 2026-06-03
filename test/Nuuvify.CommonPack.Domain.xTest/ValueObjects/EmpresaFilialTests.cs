using Nuuvify.CommonPack.Domain.ValueObjects;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects;

[Trait("Category", "Unit")]
public class EmpresaFilialTests
{
    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(EmpresaFilial))]
    [InlineData("EMP01", "F001", true)]
    [InlineData("EMPR01", "F001", false)]
    [InlineData("EMP01", "FIL01", false)]
    public void EmpresaFilial_Validade_DeveRetornarEsperado(string empresa, string filial, bool esperado)
    {
        var ef = new EmpresaFilial(empresa, filial);
        Assert.Equal(esperado, ef.IsValid());
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(EmpresaFilial))]
    public void EmpresaFilial_Valido_PropriedadesPreenchidas()
    {
        var ef = new EmpresaFilial("EMP01", "F001");
        Assert.True(ef.IsValid());
        Assert.Equal("EMP01", ef.CodigoEmpresa);
        Assert.Equal("F001", ef.CodigoFilial);
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(EmpresaFilial))]
    public void EmpresaFilial_EmpresaExcedeMaxLength_CodigoEmpresaNulo()
    {
        var ef = new EmpresaFilial("EMPR01", "F001");
        Assert.False(ef.IsValid());
        Assert.Null(ef.CodigoEmpresa);
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(EmpresaFilial))]
    public void EmpresaFilial_FilialExcedeMaxLength_CodigoFilialNulo()
    {
        var ef = new EmpresaFilial("EMP01", "FIL01");
        Assert.False(ef.IsValid());
        Assert.Null(ef.CodigoFilial);
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(EmpresaFilial))]
    public void EmpresaFilial_Constantes_TemValoresEsperados()
    {
        Assert.Equal(0, EmpresaFilial.minEmpresa);
        Assert.Equal(5, EmpresaFilial.maxEmpresa);
        Assert.Equal(0, EmpresaFilial.minFilial);
        Assert.Equal(4, EmpresaFilial.maxFilial);
    }
}
