using Nuuvify.CommonPack.Domain.xTest.Entities;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest
{
    public class DomainEntityTests
    {

        [Fact]
        [Trait("CommonPack.Domain", nameof(DomainEntityTests))]
        public void DomainEntity_ComObjetosDeIdsDiferentes_DevemRetornarFalseParaEquals()
        {

            var cliente1 = new Cliente(987654, "Lincoln");
            var cliente2 = new Cliente(987654, "Lincoln");

            var isNotEqual = cliente1.Equals(cliente2);


            Assert.False(isNotEqual);

        }

        [Fact]
        [Trait("CommonPack.Domain", nameof(DomainEntityTests))]
        public void DomainEntity_ComObjetosNull_DeveRetornarFalseParaEquals()
        {

            var cliente1 = new Cliente(987654, "Lincoln");
            Cliente cliente2 = null;

            var isNotEqual = cliente1.Equals(cliente2);

            Assert.False(isNotEqual);

        }

        [Fact]
        [Trait("CommonPack.Domain", nameof(DomainEntityTests))]
        public void DomainEntity_ComObjetosComMesmoId_DeveRetornarTrueParaEquals()
        {

            var cliente1 = new Cliente(987654, "Lincoln");
            var cliente2 = cliente1;
            cliente2.DataUltimoPedido = System.DateTime.Now;

            var isEqual = cliente1.Equals(cliente2);

            Assert.True(isEqual);

        }

    
    }
}