using Nuuvify.CommonPack.Domain.ValueObjects;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest.ValueObjects;

[Trait("Category", "Unit")]
public class EmailPessoaTests
{
    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(EmailPessoa))]
    [InlineData("usuario@dominio.com", true)]
    [InlineData("nome.sobrenome@empresa.com.br", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("invalido", false)]
    [InlineData("@dominio.com", false)]
    public void EmailPessoa_Validade_DeveRetornarEsperado(string endereco, bool esperado)
    {
        var email = new EmailPessoa(endereco);
        Assert.Equal(esperado, email.IsValid());
    }

    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(EmailPessoa))]
    [InlineData("usuario@dominio.com")]
    [InlineData("nome.sobrenome@empresa.com.br")]
    public void EmailPessoa_Valido_EnderecoPreenchido(string endereco)
    {
        var email = new EmailPessoa(endereco);
        Assert.True(email.IsValid());
        Assert.Equal(endereco, email.Endereco);
    }

    [Theory]
    [Trait("CommonApi.Domain-ValueObjects", nameof(EmailPessoa))]
    [InlineData("usuario@dominio.com")]
    public void EmailPessoa_ToString_RetornaEndereco(string endereco)
    {
        var email = new EmailPessoa(endereco);
        Assert.Equal(endereco, email.ToString());
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(EmailPessoa))]
    public void EmailPessoa_ExcedeMaxLength_EhInvalido()
    {
        // 50 + 1 + 210 + 4 = 265 chars > 256 (maxEndereco)
        var enderecoLongo = new string('a', 50) + "@" + new string('b', 210) + ".com";
        var email = new EmailPessoa(enderecoLongo);
        Assert.False(email.IsValid());
    }

    [Fact]
    [Trait("CommonApi.Domain-ValueObjects", nameof(EmailPessoa))]
    public void EmailPessoa_MaxEndereco_EhDuzentosECinquentaESeis()
    {
        Assert.Equal(256, EmailPessoa.maxEndereco);
    }
}
