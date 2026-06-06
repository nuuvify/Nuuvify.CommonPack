using System;
using System.Collections.Generic;
using System.Linq;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Collections;
using Xunit;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Tests;

[Trait("Category", "Unit")]
public sealed class PagedListGenericTest
{
    private static IEnumerable<int> Items(int count) => Enumerable.Range(1, count);

    [Fact]
    [Trait("CommonApi.UnitOfWork-Collections", nameof(PagedList<int>))]
    public void PagedList_Ctor_CalculaTotalPagesCorretamente()
    {
        var lista = new PagedList<int>(Items(100), 1, 10, 1);
        Assert.Equal(10, lista.TotalPages);
    }

    [Fact]
    [Trait("CommonApi.UnitOfWork-Collections", nameof(PagedList<int>))]
    public void PagedList_Ctor_TotalCountCorreto()
    {
        var lista = new PagedList<int>(Items(50), 1, 10, 1);
        Assert.Equal(50, lista.TotalCount);
    }

    [Fact]
    [Trait("CommonApi.UnitOfWork-Collections", nameof(PagedList<int>))]
    public void PagedList_PrimeiraPagina_NaoTemPaginaAnterior()
    {
        var lista = new PagedList<int>(Items(30), 1, 10, 1);
        Assert.False(lista.HasPreviousPage);
    }

    [Fact]
    [Trait("CommonApi.UnitOfWork-Collections", nameof(PagedList<int>))]
    public void PagedList_PrimeiraPagina_TemProximaPagina()
    {
        var lista = new PagedList<int>(Items(30), 1, 10, 1);
        Assert.True(lista.HasNextPage);
    }

    [Fact]
    [Trait("CommonApi.UnitOfWork-Collections", nameof(PagedList<int>))]
    public void PagedList_UltimaPagina_NaoTemProximaPagina()
    {
        var lista = new PagedList<int>(Items(30), 3, 10, 1);
        Assert.False(lista.HasNextPage);
    }

    [Fact]
    [Trait("CommonApi.UnitOfWork-Collections", nameof(PagedList<int>))]
    public void PagedList_SegundaPagina_TemPaginaAnterior()
    {
        var lista = new PagedList<int>(Items(30), 2, 10, 1);
        Assert.True(lista.HasPreviousPage);
    }

    [Fact]
    [Trait("CommonApi.UnitOfWork-Collections", nameof(PagedList<int>))]
    public void PagedList_Skip_CalculadoCorretamente()
    {
        // PageIndex=2, PageSize=10, IndexFrom=1 → Skip=(2-1)*10=10
        var lista = new PagedList<int>(Items(30), 2, 10, 1);
        Assert.Equal(10, lista.Skip);
    }

    [Fact]
    [Trait("CommonApi.UnitOfWork-Collections", nameof(PagedList<int>))]
    public void PagedList_Take_IgualAPageSize()
    {
        var lista = new PagedList<int>(Items(30), 1, 10, 1);
        Assert.Equal(10, lista.Take);
    }

    [Fact]
    [Trait("CommonApi.UnitOfWork-Collections", nameof(PagedList<int>))]
    public void PagedList_IndexFromMaiorQuePageIndex_LancaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new PagedList<int>(Items(10), pageIndex: 1, pageSize: 5, indexFrom: 2));
    }

    [Fact]
    [Trait("CommonApi.UnitOfWork-Collections", nameof(PagedList<int>))]
    public void PagedList_Ctor_ItensNaPrimeiraPagina()
    {
        var lista = new PagedList<int>(Items(25), 1, 10, 1);
        Assert.Equal(10, lista.Items.Count);
        Assert.Equal(1, lista.Items.First());
    }
}
