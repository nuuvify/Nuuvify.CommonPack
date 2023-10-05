using Nuuvify.CommonPack.Domain.ValueObjects;
using Nuuvify.CommonPack.Extensions.Implementation;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects
{
    public class CodigoSistemaTests
    {
        [Theory]
        [Trait("CommonApi.Domain-ValueObjects", nameof(CodigoSistemaVo))]
        [InlineData("x", false)]
        [InlineData("0", false)]
        [InlineData("1", false)]
        [InlineData("Reinf", true)]
        [InlineData("Sap", true)]
        [InlineData(null, false)]
        [InlineData("reinf", true)]
        [InlineData("sap", true)]
        [InlineData("REINF", true)]
        [InlineData("SAP", true)]
        public void CodigoSistema(string codigo, bool result)
        {

            var situacao = new CodigoSistemaVo(codigo);
            Assert.Equal(result, situacao.IsValid());
        }


        [Theory]
        [Trait("CommonApi.Domain-ValueObjects", nameof(CodigoSistemaVo))]
        [InlineData("reinf", "REINF", 1)]
        [InlineData("sap", "SAP", 2)]
        [InlineData("REINF", "REINF", 1)]
        [InlineData("SAP", "SAP", 2)]
        public void CodigoSistemaCodigoValido(string codigo, string result, int hashCodigo)
        {

            var situacao = new CodigoSistemaVo(codigo);
            Assert.Equal(result, situacao.ToString());
            Assert.Equal(hashCodigo, situacao.GetHashCode());
        }


        [Theory]
        [Trait("CommonApi.Domain-ValueObjects", nameof(CodigoSistemaVo))]
        [InlineData("reinf", "REINF", 1)]
        [InlineData("sap", "SAP", 2)]
        [InlineData("REINF", "REINF", 1)]
        [InlineData("SAP", "SAP", 2)]
        public void TransformaCodigoEmEnum(string codigo, string result, int hashCodigo)
        {

            var codigoLiteral = new CodigoSistemaVo(codigo);
            var codigoNumerico = codigoLiteral.Codigo.ToEnumNumero<CodigoSistema>();
            var codigoNumericoOutraForma = codigoLiteral.GetHashCode();


            Assert.Equal(result, codigoLiteral.ToString());
            Assert.Equal(hashCodigo, codigoNumerico);
            Assert.Equal(hashCodigo, codigoNumericoOutraForma);
        }
    }
}
