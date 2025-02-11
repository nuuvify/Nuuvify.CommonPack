using System.Linq.Expressions;
using Nuuvify.CommonPack.Extensions;
using Nuuvify.CommonPack.Extensions.Implementation;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest;


public class ExpressionExtensionTests
{

    [Fact]
    [Trait("CommonPack.Extensions", nameof(ExpressionExtension))]
    public void CombineExpressions_ComFiltro_DeveRetornarListaFiltrada()
    {

        var customers = new List<Customer>()
            {
                new Customer() { Id = "AA1", Nome = "Fritz", Codigo = 3, Tipo = "A" },
                new Customer() { Id = "XX1", Nome = "Giropopis", Codigo = 1, Tipo = "A" },
                new Customer() { Id = "XA2", Nome = "Stradivarius", Codigo = 2, Tipo = "B" },
                new Customer() { Id = "XB1", Nome = "Giropopis", Codigo = 1, Tipo = "C" },
                new Customer() { Id = "DDD", Nome = "Fritz", Codigo = 3, Tipo = "D" },
                new Customer() { Id = "XC3", Nome = "Fulano", Codigo = 1, Tipo = "E" },
            };

        Tipos = new string[] { "A", "E" };
        Codigos = new int[] { 1, 2 };

        var query = customers.AsQueryable()
            .Where(GetFilter())
            .OrderByDescending(p => p.Tipo)
            .ThenBy(p => p.Nome);

        var result = query.ToList();


        Assert.Equal(expected: 2, result.Count());
        Assert.Equal(expected: 1, result.Count(p => p.Tipo == "A"));
        Assert.Equal(expected: 1, result.Count(p => p.Tipo == "E"));

    }


    [Fact]
    [Trait("CommonPack.Extensions", nameof(CacheTimeService))]
    public void CacheTimeService_ComHoraComoString_DeveRetornarDateEHoraLocal()
    {
        const string time = "22:00:00";

        var dateTimeLocal = CacheTimeService.ExpireAt(time);
        var actual = dateTimeLocal.ToString("HH:mm:ss");


        Assert.Equal(expected: time, actual: actual);

    }



    public string Id { get; set; }
    public int[] Codigos { get; set; }
    public string Nome { get; set; }
    public string[] Tipos { get; set; }

    public Expression<Func<Customer, bool>> GetFilter()
    {
        Expression<Func<Customer, bool>> filter = p => true;


        filter = p => p.Id != null;


        if (Codigos.NotNullOrZero())
        {
            filter = filter.CombineExpressions<Customer>(
                p => Codigos.Contains(p.Codigo));
        }

        if (Tipos.NotNullOrZero())
        {
            filter = filter.CombineExpressions<Customer>(
                p => Tipos.Contains(p.Tipo));
        }


        return filter;
    }

}

