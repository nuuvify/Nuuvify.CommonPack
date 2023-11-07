using Nuuvify.CommonPack.Domain.ValueObjects;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects
{
    public class EnderecoTests
    {

        [Fact]
        public void EnderecoComTipoLogradouroComMinZero()
        {
            var endereco = new Endereco(
                null,
                "Padre Capelli",
                "1112223",
                "Porto Ferreira",
                "sp",
                "centro",
                "13660-000",
                "921",
                "casa",
                "BR");


            Assert.True(endereco.IsValid());

        }
    }
}
