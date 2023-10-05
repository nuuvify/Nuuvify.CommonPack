using Nuuvify.CommonPack.Extensions.Implementation;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest
{
    public class TextoHelperTests
    {
        [Theory]
        [Trait("CommonPack.Extensions", nameof(StringExtensionMethods))]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData("LINCOLN ZOCATELI", "Lincoln Zocateli")]
        [InlineData("SOENERGY  SISTEMAS INTERNACIONAIS DE ENERGIA SA", "Soenergy  Sistemas Internacionais De Energia Sa")]
        public void ToTitleCase(string texto, string retorno)
        {

            var textoRetorno = texto.ToTitleCase();


            Assert.Equal(retorno, textoRetorno);

        }


        [Theory]
        [InlineData("Texto maiúsculo com char especial #$%", "TEXTO MAIÚSCULO COM CHAR ESPECIAL #$%")]
        [InlineData(null, null)]
        public void TextoComToUpperInvariant_Nulo_ou_NaoNulo(string textActual, string textExpected)
        {

            var newText = textActual.ToUpperInvariantNotNull();


            Assert.Equal(textExpected, newText);
        }

        [Fact]
        public void TextoComToUpperInvariant_Retorna_Nulo_Caso_Entrada_For_Nulo()
        {
            var textExpected = "TEXTO MAIÚSCULO COM CHAR ESPECIAL #$%";
            string textActual = null;


            var newText = textActual.ToUpperInvariantNotNull();

            Assert.Null(newText);
            Assert.NotEqual(textExpected, newText);
        }

        [Fact]
        public void TextoSemCharEspecialDeveRetornarTextoSemDriatico()
        {

            var text = "Isso é um teste, sem chars especiais !?#";

            var newText = text.RemoveSpecialChars();

            var textExpected = "Isso  um teste, sem chars especiais !?#";

            Assert.Equal(textExpected, newText);

        }

        [Fact]
        public void TextoComCharsEspeciaisDeveRetornarTabelaAscii_entre_H20_a_7E_SemDriaticos()
        {
            var text = "França-Brasil!?!#@$%^&*()_+\\|}{○<>??/ €$¥£¢ \\^$.|?*+()[{ 0123456789";

            var newText = text.RemoveSpecialChars();

            var textExpected = "Frana-Brasil!?!#@$%^&*()_+\\|}{<>??/ $ \\^$.|?*+()[{ 0123456789";

            Assert.Equal(textExpected, newText);

        }

        [Fact]
        public void TextoComCharsEspeciaisTabelaAscii_entre_H0_a_H1F_DeveRemoverEssesCaracteres()
        {
            var charNull = char.ConvertFromUtf32(0);
            var charEndOfText = char.ConvertFromUtf32(3);
            var charVerticalTab = char.ConvertFromUtf32(9);
            var charEnter = char.ConvertFromUtf32(13);

            var text = "França-Brasil!?!#@$%^&*()_+\\|}{○<>??/ €$¥£¢ \\^$.|?*+()[{ 0123456789"
                + charNull
                + charEndOfText
                + charVerticalTab
                + charEnter;

            var newText = text.RemoveSpecialChars();

            var textExpected = "Frana-Brasil!?!#@$%^&*()_+\\|}{<>??/ $ \\^$.|?*+()[{ 0123456789";

            Assert.Equal(textExpected, newText);

        }
        [Fact]
        public void TextoComCharsEspeciaisTabelaAscii_entre_H0_a_H1F_DeveRemoverEssesCaracteres_e_ManterDriaticos()
        {
            var charNull = char.ConvertFromUtf32(0);
            var charEndOfText = char.ConvertFromUtf32(3);
            var charVerticalTab = char.ConvertFromUtf32(9);
            var charEnter = char.ConvertFromUtf32(13);

            var text = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇçFrança-Brasil!?!#@$%^&*()_+\\|}{○<>??/ €$¥£¢ \\^$.|?*+()[{ 0123456789"
                + charNull
                + charEndOfText
                + charVerticalTab
                + charEnter;

            var newText = text.RemoveCharsKeepDiacritics();

            var textExpected = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇçFrançaBrasil0123456789";

            Assert.Equal(textExpected, newText);

        }

        [Fact]
        public void RemoveCharEspeciaisMantemDriaticos_TabelaAsciiExtendida()
        {
            var text = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇçFrança-Brasil-Isso é um teste ação avô avó!?!#@$%^&*()_+\\|}{○<>??/ €$¥£¢ \\^$.|?*+()[{ 0123456789";

            var newText = text.RemoveCharsKeepDiacritics();

            var textExpected = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇçFrançaBrasilIssoéumtesteaçãoavôavó0123456789";

            Assert.Equal(textExpected, newText);

        }
        [Fact]
        public void RemoveCharEspeciaisInclusiveDriaticos()
        {
            var text = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇçFrança-Brasil-Isso é um teste ação avô avó!?!#@$%^&*()_+\\|}{○<>??/ €$¥£¢ \\^$.|?*+()[{ 0123456789";

            var newText = text.RemoveSpecialChars();

            var textExpected = "Frana-Brasil-Isso  um teste ao av av!?!#@$%^&*()_+\\|}{<>??/ $ \\^$.|?*+()[{ 0123456789";

            Assert.Equal(textExpected, newText);

        }

        [Fact]
        public void RemoveCharEspeciaisMantemCharsEspecificadosForaCharAdicao()
        {
            var text = "França-Brasil-Isso é um teste ação avô avó!?!#@$%^&*()_+\\|}{○<>??/ €$¥£¢ \\^$.|?*+()[{ 0123456789";


            var newText = text.RemoveCharsKeepChars("(", ")", "¥", "-", "+");

            var textExpected = "França-Brasil-Issoéumtesteaçãoavôavó()¥()0123456789";

            Assert.Equal(textExpected, newText);

        }

        [Fact]
        public void RemoveQualquerCharQueNaoForLetra_e_Numero()
        {
            var charNull = char.ConvertFromUtf32(0);
            var charEndOfText = char.ConvertFromUtf32(3);
            var charVerticalTab = char.ConvertFromUtf32(9);
            var charEnter = char.ConvertFromUtf32(13);

            var text = "França-Brasil-Isso é um teste ação avô avó!?!#@$%^&*()_+\\|}{○<>??/ €$¥£¢ \\^$.|?*+()[{ 0123456789"
                + charNull
                + charEndOfText
                + charVerticalTab
                + charEnter;

            var newText = text.GetLettersAndNumbersOnly();

            var textExpected = "FranaBrasilIssoumtesteaoavav0123456789";

            Assert.Equal(textExpected, newText);
        }

        [Fact]
        public void RemoveUnicode()
        {
            var charNull = char.ConvertFromUtf32(0);
            var charEndOfText = char.ConvertFromUtf32(3);
            var charVerticalTab = char.ConvertFromUtf32(9);
            var charEnter = char.ConvertFromUtf32(13);


            var text = "França-Brasil-Isso é um teste ação avô avó!?!#@$%^&*()_+\\|}{○<>??/ €$¥£¢ \\^$.|?*+()[{ 0123456789"
                + charNull
                + charEndOfText
                + charVerticalTab
                + charEnter;


            var newText = text.GetUnicodeChars();

            var textExpected = "FranaBrasilIssoumtesteaoavav0123456789";

            Assert.Equal(textExpected, newText);

        }

        [Fact]
        public void SubstringNotNull_Texto_Menor_retorna_mesmo_texto()
        {

            var text = "Lincoln";

            var newText = text.SubstringNotNull(0, 10);

            var textExpected = "Lincoln";

            Assert.Equal(textExpected, newText);

        }
        [Fact]
        public void SubstringNotNull_Texto_Menor_com_start_e_length_igual_retorna_empty()
        {

            var text = "Lincoln";

            var newText = text.SubstringNotNull(10, 10);

            var textExpected = string.Empty;

            Assert.Equal(textExpected, newText);

        }
        [Fact]
        public void SubstringNotNull_Texto_Maior_com_start_e_length_igual_retorna_empty()
        {

            var text = "Lincoln Zocateli";

            var newText = text.SubstringNotNull(10, 10);

            var textExpected = "cateli";

            Assert.Equal(textExpected, newText);

        }
        [Fact]
        public void SubstringNotNull_Texto_com_mesmo_tamanho_do_corte_deve_retornar_empty()
        {

            var text = "Lincoln";

            var newText = text.SubstringNotNull(7, 10);

            var textExpected = string.Empty;

            Assert.Equal(textExpected, newText);

        }
        [Fact]
        public void SubstringNotNull_Texto_com_tamanho_menor_deve_retornar_o_resto_do_texto()
        {

            var text = "Lincoln";

            var newText = text.SubstringNotNull(6, 10);

            var textExpected = "n";

            Assert.Equal(textExpected, newText);

        }
        [Fact]
        public void SubstringNotNull_Texto_com_tamanho_inicio_maior_que_final_retorna_Empty()
        {

            var text = "Lincoln";

            var newText = text.SubstringNotNull(12, 10);

            var textExpected = string.Empty;

            Assert.Equal(textExpected, newText);

        }
        [Fact]
        public void SubstringNotNull_Texto_maior_que_final_com_inicio_maior_que_final_retorna_retante_do_texto()
        {

            var text = "Lincoln Zocateli";

            var newText = text.SubstringNotNull(12, 10);

            var textExpected = "teli";

            Assert.Equal(textExpected, newText);

        }
        [Fact]
        public void SubstringNotNull_Texto_maior_que_final_com_Retorna_texto_cortado()
        {

            var text = "Lincoln Zocateli";

            var newText = text.SubstringNotNull(12, 3);

            var textExpected = "tel";

            Assert.Equal(textExpected, newText);

        }
        [Fact]
        public void SubstringNotNull_ComTextoMaiorDeveRetornarComCorte()
        {

            var text = "Lincoln Zocateli";

            var newText = text.SubstringNotNull(0, 10);

            var textExpected = "Lincoln Zo";

            Assert.Equal(textExpected, newText);

        }
        [Fact]
        public void SubstringNotNull_ComTextoNuloNaoDeveRetornarErro()
        {

            var text = string.Empty;

            var newText = text.SubstringNotNull(0, 10);

            var textExpected = string.Empty;

            Assert.Equal(textExpected, newText);

        }
    }
}
