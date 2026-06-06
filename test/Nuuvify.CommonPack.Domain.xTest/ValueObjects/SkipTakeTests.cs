using Nuuvify.CommonPack.Domain.ValueObjects;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects;

[Trait("Category", "Unit")]
public class SkipTakeTests
{
    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(SkipTake))]
    public void SkipTake_HasPagination_TakePositivo_RetornaTrue()
    {
        var st = new SkipTake { Take = 10 };
        Assert.True(st.HasPagination());
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(SkipTake))]
    public void SkipTake_HasPagination_TakeZero_RetornaFalse()
    {
        var st = new SkipTake { Take = 0 };
        Assert.False(st.HasPagination());
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(SkipTake))]
    public void SkipTake_Skip_ValorNegativo_ZeraSkip()
    {
        var st = new SkipTake { Skip = -5 };
        Assert.Equal(0, st.Skip);
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(SkipTake))]
    public void SkipTake_Take_ValorNegativo_ZeraTake()
    {
        var st = new SkipTake { Take = -1 };
        Assert.Equal(0, st.Take);
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(SkipTake))]
    public void SkipTake_SetSkipTakeZero_ZeraAmbos()
    {
        var st = new SkipTake { Skip = 20, Take = 50 };
        st.SetSkipTakeZero();
        Assert.Equal(0, st.Skip);
        Assert.Equal(0, st.Take);
    }

    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(SkipTake))]
    [InlineData(0, 25)]
    [InlineData(-1, 25)]
    [InlineData(10, 10)]
    [InlineData(100, 100)]
    public void SkipTake_MinTake_RetornaValorEsperado(int take, int esperado)
    {
        var st = new SkipTake { Take = take };
        Assert.Equal(esperado, st.MinTake());
    }
}
