using Nuuvify.CommonPack.Domain.ValueObjects;
using System.Linq;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects
{
    public class CpfTests
    {

        [Theory]
        [Trait("CommonApi.Domain-ValueObjects", nameof(Cpf))]
        [InlineData("123456", false)]
        [InlineData("111.111.111-11", false)]
        [InlineData(null, false)]
        [InlineData("419.514.167-24", true)]
        public void CpfTest(string cpf, bool resultado)
        {
            var _cpf = new Cpf(cpf);
            Assert.Equal(resultado, _cpf.IsValid());
        }

        [Theory]
        [Trait("CommonApi.Domain-ValueObjects", nameof(Cpf))]
        [InlineData("123456", "Codigo invalido")]
        public void CPFMensagemInvalido(string cpf, string mensagem)
        {
            var _cpf = new Cpf(cpf);
            Assert.Equal(mensagem, _cpf.Notifications.FirstOrDefault(x => x.Message.Equals(mensagem)).Message);
            Assert.Equal(nameof(Cpf), _cpf.Notifications.FirstOrDefault(x => x.Property.Equals(nameof(Cpf))).Property);

        }


        [Theory]
        [Trait("CommonApi.Domain-ValueObjects", nameof(Cpf))]
        [InlineData("419.514.167-24", "41951416724")]
        [InlineData("41951416724", "41951416724")]
        [InlineData("419.51416724", "41951416724")]
        public void CPFFormatado(string cpf, string cpfFormatado)
        {

            var _cpf = new Cpf(cpf);
            Assert.Equal(cpfFormatado, _cpf.Codigo);
            Assert.Equal(cpfFormatado, _cpf.ToString());
        }


        [Theory]
        [Trait("CommonApi.Domain-ValueObjects", nameof(Cpf))]
        [InlineData("419.514.167-24", "419.514.167-24")]
        [InlineData("41951416724", "419.514.167-24")]
        [InlineData("419.51416724", "419.514.167-24")]
        [InlineData("XXX.51416724", null)]
        public void CPFComMascara(string cpf, string cpfFormatado)
        {

            var _cpf = new Cpf(cpf);
            Assert.Equal(cpfFormatado, _cpf.Mascara());
        }
    }
}
