using System.Collections.Generic;
using System.Linq;
using Nuuvify.CommonPack.Extensions.Implementation;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest
{

    public class DistinctExtensionTests
    {
        [Fact]
        [Trait("CommonPack.Extensions", nameof(DistinctExtension))]
        public void Distinct_ComPropriedadesSelecionadas_DeveRetornarDistintos()
        {

            var customers = new List<Customer>()
            {
                new Customer() { Id = "AA1", Nome = "Fritz", Codigo = 3 },
                new Customer() { Id = "XX1", Nome = "Giropopis", Codigo = 1 },
                new Customer() { Id = "XA2", Nome = "Stradivarius", Codigo = 2 },
                new Customer() { Id = "XB1", Nome = "Giropopis", Codigo = 1 },
                new Customer() { Id = "DDD", Nome = "Fritz", Codigo = 3 },
                new Customer() { Id = "XC3", Nome = "Fulano", Codigo = 1 },
            }; 

            const int DistinctExpected = 4;

            var distinct = customers.Distinct((p1, p2) => 
                p1.Codigo == p2.Codigo &&
                p1.Nome == p2.Nome, 
                p1 => p1.GetHashCode()).ToList();


            Assert.Equal(expected: DistinctExpected , distinct.Count());

        }
    }

    public class Customer
    {

        public string Id { get; set; }
        public string Nome { get; set; }
        public int Codigo { get; set; }
    }
}