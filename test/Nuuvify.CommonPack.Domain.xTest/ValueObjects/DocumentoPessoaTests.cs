using Nuuvify.CommonPack.Domain.ValueObjects;
using System;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects
{
    public class DocumentoPessoaTests
    {
        [Theory]
        [Trait("CommonApi.Domain-ValueObjects", nameof(DocumentoPessoa))]
        [InlineData("123456", "71.266.534/0001-02")]
        public void DocumentoPessoaComCnpjCorreto(string cpf, string cnpj)
        {
            var _cpf = new Cpf(cpf);
            var _cnpj = new Cnpj(cnpj);
            var _tipoPessoa = new TipoPessoa("J", null);

            var documento = new DocumentoPessoa(_cpf, _cnpj, _tipoPessoa);

            Assert.Equal(documento.Codigo, _cnpj.Codigo);
            Assert.Equal(documento.TipoDaPessoa, _tipoPessoa.Codigo);
            Assert.True(documento.IsValid());
        }

        [Theory]
        [Trait("CommonApi.Domain-ValueObjects", nameof(DocumentoPessoa))]
        [InlineData("419.514.167-24", "71.266.534/0000-00")]
        public void DocumentoPessoaComCpfCorretoNascimentoInvalido(string cpf, string cnpj)
        {
            var _cpf = new Cpf(cpf);
            var _cnpj = new Cnpj(cnpj);
            var _tipoPessoa = new TipoPessoa("F", null);

            var documento = new DocumentoPessoa(_cpf, _cnpj, _tipoPessoa);

            Assert.Null(_cnpj.Codigo);
            Assert.NotEqual(documento.Codigo, _cpf.Codigo);
            Assert.False(documento.IsValid());
        }

        [Theory]
        [Trait("CommonApi.Domain-ValueObjects", nameof(DocumentoPessoa))]
        [InlineData("419.514.167-24", "71.266.534/0000-00")]
        public void DocumentoPessoaComCpfCorreto(string cpf, string cnpj)
        {
            var _cpf = new Cpf(cpf);
            var _cnpj = new Cnpj(cnpj);
            var _tipoPessoa = new TipoPessoa("F", DateTime.Now.Date);

            var documento = new DocumentoPessoa(_cpf, _cnpj, _tipoPessoa);

            Assert.Null(_cnpj.Codigo);
            Assert.Equal(documento.Codigo, _cpf.Codigo);
            Assert.True(documento.IsValid());
        }

        [Theory]
        [Trait("CommonApi.Domain-ValueObjects", nameof(DocumentoPessoa))]
        [InlineData("419.514.167-24", "71.266.534/0001-02")]
        public void DocumentoPessoaComCpfCnpjCorreto(string cpf, string cnpj)
        {
            var _cpf = new Cpf(cpf);
            var _cnpj = new Cnpj(cnpj);
            var _tipoPessoa = new TipoPessoa("J", null);

            var documento = new DocumentoPessoa(_cpf, _cnpj, _tipoPessoa);

            Assert.Equal(documento.Codigo, _cnpj.Codigo);
            Assert.NotEqual(documento.Codigo, _cpf.Codigo);
            Assert.Equal(documento.TipoDaPessoa, _tipoPessoa.Codigo);
            Assert.True(documento.IsValid());
        }

        [Theory]
        [Trait("CommonApi.Domain-ValueObjects", nameof(DocumentoPessoa))]
        [InlineData("419.514.167-20", "71.266.534/0001-00")]
        public void DocumentoPessoaComCnpjIncorreto(string cpf, string cnpj)
        {
            var _cpf = new Cpf(cpf);
            var _cnpj = new Cnpj(cnpj);
            var _tipoPessoa = new TipoPessoa("J", null);

            var documento = new DocumentoPessoa(_cpf, _cnpj, _tipoPessoa);

            Assert.Null(documento.Codigo);
            Assert.False(documento.IsValid());
        }

    }
}
