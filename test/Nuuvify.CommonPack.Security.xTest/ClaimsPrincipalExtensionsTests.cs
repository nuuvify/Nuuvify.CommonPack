using System;
using System.Security.Claims;
using Nuuvify.CommonPack.Security.Abstraction;
using Xunit;

namespace Nuuvify.CommonPack.Security.xTest;

[Trait("Category", "Unit")]
public class ClaimsPrincipalExtensionsTests
{
    private static ClaimsPrincipal BuildPrincipal(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    [Trait("CommonApi.Security-Extensions", nameof(ClaimsPrincipalExtensions))]
    public void GetLogin_ClaimPresente_RetornaValor()
    {
        var principal = BuildPrincipal(new Claim(ClaimTypes.NameIdentifier, "usuario123"));
        Assert.Equal("usuario123", principal.GetLogin());
    }

    [Fact]
    [Trait("CommonApi.Security-Extensions", nameof(ClaimsPrincipalExtensions))]
    public void GetLogin_ClaimAusente_RetornaNull()
    {
        var principal = BuildPrincipal();
        Assert.Null(principal.GetLogin());
    }

    [Fact]
    [Trait("CommonApi.Security-Extensions", nameof(ClaimsPrincipalExtensions))]
    public void GetLogin_PrincipalNulo_LancaArgumentNullException()
    {
        ClaimsPrincipal principal = null;
        Assert.Throws<ArgumentNullException>(() => principal.GetLogin());
    }

    [Fact]
    [Trait("CommonApi.Security-Extensions", nameof(ClaimsPrincipalExtensions))]
    public void GetName_ClaimPresente_RetornaValor()
    {
        var principal = BuildPrincipal(new Claim(ClaimTypes.Name, "João Silva"));
        Assert.Equal("João Silva", principal.GetName());
    }

    [Fact]
    [Trait("CommonApi.Security-Extensions", nameof(ClaimsPrincipalExtensions))]
    public void GetName_ClaimAusente_RetornaNull()
    {
        var principal = BuildPrincipal();
        Assert.Null(principal.GetName());
    }

    [Fact]
    [Trait("CommonApi.Security-Extensions", nameof(ClaimsPrincipalExtensions))]
    public void GetName_PrincipalNulo_LancaArgumentNullException()
    {
        ClaimsPrincipal principal = null;
        Assert.Throws<ArgumentNullException>(() => principal.GetName());
    }

    [Fact]
    [Trait("CommonApi.Security-Extensions", nameof(ClaimsPrincipalExtensions))]
    public void GetEmail_ClaimPresente_RetornaValor()
    {
        var principal = BuildPrincipal(new Claim(ClaimTypes.Email, "joao@empresa.com"));
        Assert.Equal("joao@empresa.com", principal.GetEmail());
    }

    [Fact]
    [Trait("CommonApi.Security-Extensions", nameof(ClaimsPrincipalExtensions))]
    public void GetEmail_ClaimAusente_RetornaNull()
    {
        var principal = BuildPrincipal();
        Assert.Null(principal.GetEmail());
    }

    [Fact]
    [Trait("CommonApi.Security-Extensions", nameof(ClaimsPrincipalExtensions))]
    public void GetEmail_PrincipalNulo_LancaArgumentNullException()
    {
        ClaimsPrincipal principal = null;
        Assert.Throws<ArgumentNullException>(() => principal.GetEmail());
    }
}
