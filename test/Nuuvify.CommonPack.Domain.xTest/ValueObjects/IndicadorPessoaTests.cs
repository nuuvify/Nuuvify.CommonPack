namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects;

// public class IndicadorPessoaTests
// {
//     [Theory]
//     [Trait("CommonApi.Domain-ValueObjects", nameof(IndicadorPessoa))]
//     [InlineData("cliente", null, false)]
//     [InlineData("fornecedor", null, false)]
//     [InlineData("transportador", null, false)]
//     [InlineData("estabelecimento", null, false)]
//     [InlineData("kkk", null, false)]
//     [InlineData(null, null, false)]
//     [InlineData("c", "C", true)]
//     [InlineData("f", "F", true)]
//     [InlineData("t", "T", true)]
//     [InlineData("e", "E", true)]
//     public void IndicadorDePessoa(string pessoa, string codigoRetorno, bool retorno)
//     {

//         var testResult = pessoa.IsEnum<IndicadorPessoa>(out int resultEnum);
//         var returnDescription = pessoa.ToEnumDescricao<IndicadorPessoa>();

//         Assert.Equal(retorno, testResult);
//         Assert.Equal(codigoRetorno, returnDescription);

//     }

//     [Theory]
//     [Trait("CommonApi.Domain-ValueObjects", nameof(IndicadorPessoa))]
//     [InlineData("cliente", "", int.MaxValue, false)]
//     [InlineData("fornecedor", "", int.MaxValue, false)]
//     [InlineData("transportador", "", int.MaxValue, false)]
//     [InlineData("estabelecimento", "", int.MaxValue, false)]
//     [InlineData("kkk", "", int.MaxValue, false)]
//     [InlineData(null, "", int.MaxValue, false)]
//     [InlineData("c", "Cliente", 0, true)]
//     [InlineData("f", "Fornecedor", 1, true)]
//     [InlineData("t", "Transportador", 2, true)]
//     [InlineData("e", "Estabelecimento", 3, true)]
//     public void IndicadorDePessoaObterDescricao(string pessoa, string descricao, int codigoRetorno, bool retorno)
//     {

//         var codigoLiteral = pessoa;
//         var retornoDescricao = codigoLiteral.GetCodeEnumByDescription<IndicadorPessoa>();
//         var codigoNumerico = retornoDescricao.ToEnumNumero<IndicadorPessoa>();

//         Assert.Equal(descricao, retornoDescricao);
//         Assert.Equal(codigoNumerico, codigoRetorno);

//     }
// }
