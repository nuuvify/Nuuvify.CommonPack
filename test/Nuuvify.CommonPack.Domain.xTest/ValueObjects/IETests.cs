using Nuuvify.CommonPack.Domain.ValueObjects;
using Nuuvify.CommonPack.Extensions.Implementation;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects;

public class IETests
{
    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(IE))]
    [InlineData("123456", "sp", false)]
    [InlineData("111.111.111.111", "sp", false)]
    [InlineData(null, null, false)]
    [InlineData("077.247.437.513", "mg", false)]
    [InlineData("077.247.437.513", "sp", true)]
    [InlineData("isento", "sp", true)]
    public void IETest(string ie, string uf, bool resultado)
    {
        var _ie = new IE(ie, uf);
        Assert.Equal(resultado, _ie.IsValid());
    }

    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(IE))]
    [InlineData("077.247.437.513", "sp", "077247437513")]
    [InlineData("077247437513", "sp", "077247437513")]
    [InlineData("077.247437513", "sp", "077247437513")]
    [InlineData("isento", "Sp", "ISENTO")]
    public void IEFormatado(string ie, string uf, string ieFormatado)
    {

        var _ie = new IE(ie, uf);
        Assert.Equal(ieFormatado, _ie.Codigo);
        Assert.Equal(ieFormatado, _ie.ToString());
        Assert.Equal(uf.ToUpper(), _ie.UF);
    }

    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(IE))]
    [InlineData("01133113333311", "sp")]
    public void ValidaMensagemParaIEIncorreto(string ie, string uf)
    {

        var _ie = new IE(ie, uf);
        Assert.Null(_ie.Codigo);
        Assert.NotNull(_ie.Notifications.FirstOrDefault(x => x.Message != null));
        Assert.Null(_ie.UF);
    }

    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(IE))]
    [InlineData("07988303001-77", "df")]
    [InlineData("0798830300177", "df")]
    public void IEParaDFDeveSerValido(string ie, string uf)
    {

        var _ie = new IE(ie, uf);

        Assert.Equal(_ie.Codigo, ie.GetNumbers());
        Assert.True(_ie.IsValid());
        Assert.Equal(_ie.UF, uf.ToUpper());
    }
}
