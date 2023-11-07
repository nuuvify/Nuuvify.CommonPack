using Nuuvify.CommonPack.Domain.ValueObjects;
using Nuuvify.CommonPack.Extensions.Implementation;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects
{
    public class DadosBancariosTests
    {
        [Theory]
        [Trait("CommonApi.Domain-ValueObjects", nameof(DadosBancarios))]
        [InlineData(null, null, null, null, TipoContaBancaria.ContaCorrente, false)]
        [InlineData(null, null, null, null, TipoContaBancaria.ContaConjunta, false)]
        [InlineData(null, null, null, null, TipoContaBancaria.ContaPoupanca, false)]
        [InlineData(null, null, null, null, TipoContaBancaria.NaoPossuiConta, true)]
        [InlineData("341", "02861-5", "itau pira", "123456-7", TipoContaBancaria.ContaCorrente, true)]
        [InlineData("341", "02861-5", "itau pira", "123456-7", TipoContaBancaria.ContaPoupanca, true)]
        [InlineData("341", "02861-5", "itau pira", "123456-7", TipoContaBancaria.ContaConjunta, true)]
        public void DadosBancariosTest(string bancoNumero, string agenciaNumero, string agenciaNome, string contaCorrente, TipoContaBancaria tipoConta, bool retorno)
        {


            var _agenciaNome = agenciaNome.ToTitleCase();


            var teste = new DadosBancarios(bancoNumero, agenciaNumero, agenciaNome, contaCorrente, tipoConta);

            Assert.Equal(bancoNumero, teste.BancoNumero);
            Assert.Equal(agenciaNumero, teste.AgenciaNumero);
            Assert.Equal(_agenciaNome, teste.AgenciaNome);
            Assert.Equal(contaCorrente, teste.ContaCorrente);
            Assert.Equal(retorno, teste.IsValid());
        }

    }
}
