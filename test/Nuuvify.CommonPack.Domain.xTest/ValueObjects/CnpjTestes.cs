using Nuuvify.CommonPack.Domain.ValueObjects;
using System.Linq;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects;

public class CnpjTestes
{
    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(Cnpj))]
    [InlineData("123456", false)]
    [InlineData("11.111.111/1111-11", false)]
    [InlineData(null, false)]
    [InlineData("71.266.534/0001-02", true)]
    // CNPJ alfanumerico
    [InlineData("12ABC345000188", true)]
    [InlineData("A1B2C3D4E5F668", true)]
    [InlineData("00ABC000000145", true)]
    [InlineData("ABCDEF00000160", true)]
    [InlineData("Z9Y8X7W6V5U429", true)]
    // CNPJ alfanumerico com mascara
    [InlineData("12.ABC.345/0001-88", true)]
    [InlineData("A1.B2C.3D4/E5F6-68", true)]
    // CNPJ alfanumerico minusculo (deve converter)
    [InlineData("12abc345000188", true)]
    [InlineData("a1b2c3d4e5f668", true)]
    // CNPJ alfanumerico invalido (DV errado)
    [InlineData("12ABC345000199", false)]
    [InlineData("A1B2C3D4E5F600", false)]
    // Caracteres especiais invalidos
    [InlineData("12@BC345000188", false)]
    [InlineData("12#BC345000188", false)]
    public void CnpjTest(string cnpj, bool resultado)
    {
        var _cnpj = new Cnpj(cnpj);
        Assert.Equal(resultado, _cnpj.IsValid());
    }

    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(Cnpj))]
    [InlineData("123456", "Codigo invalido")]
    [InlineData("12ABC345000199", "Codigo invalido")]
    public void CNPJMensagemInvalido(string cnpj, string mensagem)
    {
        var _cnpj = new Cnpj(cnpj);
        Assert.Equal(mensagem, _cnpj.Notifications.FirstOrDefault(x => x.Message.Equals(mensagem, StringComparison.Ordinal)).Message);
        Assert.Equal(nameof(Cnpj), _cnpj.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Cnpj)}", StringComparison.Ordinal)).Property);
    }

    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(Cnpj))]
    [InlineData("71.266.534/0001-02", "71266534000102")]
    [InlineData("71266534000102", "71266534000102")]
    [InlineData("71.266.534000102", "71266534000102")]
    // CNPJ alfanumerico
    [InlineData("12.ABC.345/0001-88", "12ABC345000188")]
    [InlineData("12ABC345000188", "12ABC345000188")]
    [InlineData("12abc345000188", "12ABC345000188")]
    [InlineData("A1.B2C.3D4/E5F6-68", "A1B2C3D4E5F668")]
    public void CNPJFormatado(string cnpj, string cnpjFormatado)
    {

        var _cnpj = new Cnpj(cnpj);
        Assert.Equal(cnpjFormatado, _cnpj.Codigo);
        Assert.Equal(cnpjFormatado, _cnpj.ToString());
    }

    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(Cnpj))]
    [InlineData("71.266.534/0001-02", "71.266.534/0001-02")]
    [InlineData("71266534000102", "71.266.534/0001-02")]
    [InlineData("71.266.534000102", "71.266.534/0001-02")]
    [InlineData("XX.266.534000102", null)]
    // CNPJ alfanumerico
    [InlineData("12ABC345000188", "12.ABC.345/0001-88")]
    [InlineData("12.ABC.345/0001-88", "12.ABC.345/0001-88")]
    [InlineData("A1B2C3D4E5F668", "A1.B2C.3D4/E5F6-68")]
    [InlineData("ABCDEF00000160", "AB.CDE.F00/0001-60")]
    [InlineData("12abc345000188", "12.ABC.345/0001-88")]
    public void CNPJComMascara(string cnpj, string cnpjFormatado)
    {

        var _cnpj = new Cnpj(cnpj);
        Assert.Equal(cnpjFormatado, _cnpj.Mascara());

    }

    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(Cnpj))]
    [InlineData("00000000000000")]
    [InlineData("11111111111111")]
    [InlineData("AAAAAAAAAAAAAA")]
    public void CnpjTodosCaracteresIguaisDeveSerInvalido(string cnpj)
    {
        var _cnpj = new Cnpj(cnpj);
        Assert.False(_cnpj.IsValid());
        Assert.Null(_cnpj.Codigo);
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(Cnpj))]
    public void CnpjNumericoExistenteDeveManterCompatibilidade()
    {
        var _cnpj = new Cnpj("71266534000102");
        Assert.True(_cnpj.IsValid());
        Assert.Equal("71266534000102", _cnpj.Codigo);
        Assert.Equal("71.266.534/0001-02", _cnpj.Mascara());
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(Cnpj))]
    public void CnpjAlfanumericoValidoDeveRetornarCodigoUpperCase()
    {
        var _cnpj = new Cnpj("12abc345000188");
        Assert.True(_cnpj.IsValid());
        Assert.Equal("12ABC345000188", _cnpj.Codigo);
    }

    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(Cnpj))]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CnpjVazioOuNuloDeveSerInvalido(string cnpj)
    {
        var _cnpj = new Cnpj(cnpj);
        Assert.False(_cnpj.IsValid());
        Assert.Null(_cnpj.Codigo);
        Assert.Null(_cnpj.Mascara());
    }
}
