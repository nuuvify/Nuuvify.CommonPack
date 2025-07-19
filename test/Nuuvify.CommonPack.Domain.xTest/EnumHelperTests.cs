using Nuuvify.CommonPack.Domain.ValueObjects;
using Nuuvify.CommonPack.Extensions.Implementation;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest
{

    public class EnumHelperTests
    {
        [Fact]
        [Trait("CommonPack.Extensions", nameof(EnumExtensionMethods))]
        public void EnumHelperAtivo()
        {

            var _situacao = EnumAtivoInativo.Ativo;

            var descricao = _situacao.ToString().ToEnumDescricao<EnumAtivoInativo>();
            var codigo = descricao.GetCodeEnumByDescription<EnumAtivoInativo>();
            var numero = codigo.ToEnumNumero<EnumAtivoInativo>();
            var literal = numero.ToEnumCodigo<EnumAtivoInativo>();


            Assert.NotEqual(codigo, descricao);
            Assert.NotEqual(numero.ToString(), codigo);
            Assert.NotEqual(descricao, literal);

            Assert.Equal("A", descricao);
            Assert.Equal("Ativo", codigo);
            Assert.Equal(0, numero);
            Assert.Equal("Ativo", literal);

        }

        [Fact]
        [Trait("CommonPack.Extensions", nameof(EnumExtensionMethods))]
        public void EnumHelperInativo()
        {

            var _situacao = EnumAtivoInativo.Inativo;

            var descricao = _situacao.ToString().ToEnumDescricao<EnumAtivoInativo>();
            var codigo = descricao.GetCodeEnumByDescription<EnumAtivoInativo>();
            var numero = codigo.ToEnumNumero<EnumAtivoInativo>();
            var literal = numero.ToEnumCodigo<EnumAtivoInativo>();


            Assert.NotEqual(codigo, descricao);
            Assert.NotEqual(numero.ToString(), codigo);
            Assert.NotEqual(descricao, literal);

            Assert.Equal("I", descricao);
            Assert.Equal("Inativo", codigo);
            Assert.Equal(1, numero);
            Assert.Equal("Inativo", literal);

        }


        [Fact]
        [Trait("CommonPack.Extensions", nameof(EnumExtensionMethods))]
        public void EnumHelperAmbos()
        {

            var _situacao = EnumAtivoInativo.Ambos;

            var descricao = _situacao.ToString().ToEnumDescricao<EnumAtivoInativo>();
            var codigo = descricao.GetCodeEnumByDescription<EnumAtivoInativo>();
            var numero = codigo.ToEnumNumero<EnumAtivoInativo>();
            var literal = numero.ToEnumCodigo<EnumAtivoInativo>();


            Assert.NotEqual(codigo, descricao);
            Assert.NotEqual(numero.ToString(), codigo);
            Assert.NotEqual(descricao, literal);

            Assert.Equal("N", descricao);
            Assert.Equal("Ambos", codigo);
            Assert.Equal(2, numero);
            Assert.Equal("Ambos", literal);

        }


    }
}
