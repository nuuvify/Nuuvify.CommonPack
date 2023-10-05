using Nuuvify.CommonPack.Domain.ValueObjects;
using System.Linq;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects
{
    public class CnpjTestes
    {
        [Theory]
        [Trait("CommonApi.Domain-ValueObjects", nameof(Cnpj))]
        [InlineData("123456", false)]
        [InlineData("11.111.111/1111-11", false)]
        [InlineData(null, false)]
        [InlineData("71.266.534/0001-02", true)]
        public void CnpjTest(string cnpj, bool resultado)
        {
            var _cnpj = new Cnpj(cnpj);
            Assert.Equal(resultado, _cnpj.IsValid());
        }

        [Theory]
        [Trait("CommonApi.Domain-ValueObjects", nameof(Cnpj))]
        [InlineData("123456", "Codigo invalido")]
        public void CNPJMensagemInvalido(string cnpj, string mensagem)
        {
            var _cnpj = new Cnpj(cnpj);
            Assert.Equal(mensagem, _cnpj.Notifications.FirstOrDefault(x => x.Message.Equals(mensagem)).Message);
            Assert.Equal(nameof(Cnpj), _cnpj.Notifications.FirstOrDefault(x => x.Property.Equals($"{nameof(Cnpj)}")).Property);
        }

        [Theory]
        [Trait("CommonApi.Domain-ValueObjects", nameof(Cnpj))]
        [InlineData("71.266.534/0001-02", "71266534000102")]
        [InlineData("71266534000102", "71266534000102")]
        [InlineData("71.266.534000102", "71266534000102")]
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
        public void CNPJComMascara(string cnpj, string cnpjFormatado)
        {

            var _cnpj = new Cnpj(cnpj);
            Assert.Equal(cnpjFormatado, _cnpj.Mascara());

        }

    }
}
