using Nuuvify.CommonPack.Domain.ValueObjects;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects;

[Trait("Category", "Unit")]
public class NomeCompletoTests
{
    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(NomeCompleto))]
    [InlineData("João", "Silva", true)]
    [InlineData("Ana", "Pereira", true)]
    [InlineData(null, "Silva", false)]
    [InlineData("Jo", "Silva", false)]
    [InlineData("João", "Si", false)]
    public void NomeCompleto_Validade_DeveRetornarEsperado(string nome, string sobrenome, bool esperado)
    {
        var nomeCompleto = new NomeCompleto(nome, sobrenome);
        Assert.Equal(esperado, nomeCompleto.IsValid());
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(NomeCompleto))]
    public void NomeCompleto_Valido_PropriedadesPreenchidas()
    {
        var nomeCompleto = new NomeCompleto("João", "Silva");
        Assert.True(nomeCompleto.IsValid());
        Assert.NotNull(nomeCompleto.Nome);
        Assert.NotNull(nomeCompleto.SobreNome);
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(NomeCompleto))]
    public void NomeCompleto_Invalido_PropriedadesNulas()
    {
        var nomeCompleto = new NomeCompleto("Jo", "Silva");
        Assert.False(nomeCompleto.IsValid());
        Assert.Null(nomeCompleto.Nome);
        Assert.Null(nomeCompleto.SobreNome);
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(NomeCompleto))]
    public void NomeCompleto_ToString_RetornaNomeSobrenome()
    {
        var nomeCompleto = new NomeCompleto("João", "Silva");
        var resultado = nomeCompleto.ToString();
        Assert.Contains("Silva", resultado);
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(NomeCompleto))]
    public void NomeCompleto_NomeExcedeMaxLength_EhInvalido()
    {
        var nomeLongo = new string('a', NomeCompleto.maxNome + 1);
        var nomeCompleto = new NomeCompleto(nomeLongo, "Silva");
        Assert.False(nomeCompleto.IsValid());
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(NomeCompleto))]
    public void NomeCompleto_SobrenomeExcedeMaxLength_EhInvalido()
    {
        var sobrenomeLongo = new string('a', NomeCompleto.maxSobreNome + 1);
        var nomeCompleto = new NomeCompleto("João", sobrenomeLongo);
        Assert.False(nomeCompleto.IsValid());
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(NomeCompleto))]
    public void NomeCompleto_Constantes_TemValoresEsperados()
    {
        Assert.Equal(3, NomeCompleto.minNome);
        Assert.Equal(60, NomeCompleto.maxNome);
        Assert.Equal(3, NomeCompleto.minSobreNome);
        Assert.Equal(60, NomeCompleto.maxSobreNome);
    }
}
