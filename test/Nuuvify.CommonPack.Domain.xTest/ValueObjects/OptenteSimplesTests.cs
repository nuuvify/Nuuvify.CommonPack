using Nuuvify.CommonPack.Domain.ValueObjects;
using Nuuvify.CommonPack.Extensions.Implementation;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects;

public class OptenteSimplesTests
{
    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(OptanteSimples))]
    [InlineData(EnumSimNao.Nao, "2017-09-13", true)]
    [InlineData(EnumSimNao.Sim, "2017-09-13", true)]
    [InlineData(EnumSimNao.Nao, null, false)]
    public void OptanteSimplesInvalido(EnumSimNao simNao, string data, bool retorno)
    {
        _ = DateTime.TryParse(data, out DateTime dataEntradaOuSaida);
        var optante = new OptanteSimples(simNao, dataEntradaOuSaida);

        var literal = simNao.GetDescription();

        Assert.Equal(retorno, optante.IsValid());
        Assert.True(literal.Equals(optante.OptantePeloSimples, StringComparison.Ordinal));
        Assert.Equal(dataEntradaOuSaida, optante.DataOpcaoPeloSimples);

    }

}
