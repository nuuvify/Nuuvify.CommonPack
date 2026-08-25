using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Nuuvify.CommonPack.Security.Abstraction;
using Nuuvify.CommonPack.Security.Jwt;
using Xunit;

namespace Nuuvify.CommonPack.Security.xTest;

[Trait("Category", "Unit")]
public class JwtCoverageTests
{
    private const string SecretKey = "0123456789012345678901234567890123456789";

    [Fact]
    public void JwtTokenOptions_ValidFor_WhenNonPositive_UsesDefaultDuration()
    {
        var options = new JwtTokenOptions
        {
            ValidFor = TimeSpan.Zero
        };

        Assert.Equal(TimeSpan.FromHours(8), options.ValidFor);
    }

    [Fact]
    public void JwtTokenOptions_Expiration_AddsValidForToNotBefore()
    {
        var notBefore = new DateTimeOffset(2026, 8, 25, 10, 0, 0, TimeSpan.Zero);
        var options = new JwtTokenOptions
        {
            NotBefore = notBefore,
            ValidFor = TimeSpan.FromMinutes(30)
        };

        Assert.Equal(notBefore.AddMinutes(30), options.Expiration);
    }

    [Fact]
    public void JwtTokenOptions_KeyEncoding_WhenSecretIsMissing_Throws()
    {
        var options = new JwtTokenOptions();

        Assert.Throws<ArgumentNullException>(() => options.KeyEncoding());
        Assert.Throws<ArgumentNullException>(() => options.GetSymmetricSecurityKey());
    }

    [Fact]
    public void JwtTokenOptions_SigningCredentials_WhenKeyIsTooShort_Throws()
    {
        var options = new JwtTokenOptions { SecretKey = "short-key" };

        Assert.Throws<SecurityTokenInvalidSigningKeyException>(() => options.SigningCredentials());
    }

    [Fact]
    public void JwtTokenOptions_SigningCredentials_WithValidKey_ReturnsHmacCredentials()
    {
        var options = new JwtTokenOptions { SecretKey = SecretKey };

        var credentials = options.SigningCredentials();
        var symmetricKey = Assert.IsType<SymmetricSecurityKey>(credentials.Key);

        Assert.Equal(SecurityAlgorithms.HmacSha256, credentials.Algorithm);
        Assert.Equal(SecretKey.Length, symmetricKey.KeySize / 8);
        Assert.Equal(SecretKey, System.Text.Encoding.ASCII.GetString(options.KeyEncoding()));
        Assert.Equal(SecretKey, System.Text.Encoding.ASCII.GetString(options.GetSymmetricSecurityKey().Key));
    }

    [Fact]
    public void JwtTokenOptions_NewInstance_RefreshesNotBefore()
    {
        var options = new JwtTokenOptions
        {
            NotBefore = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)
        };

        options.NewInstance();

        Assert.True(options.NotBefore > new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void JwtBuilder_WithJwtOptions_WhenNull_Throws()
    {
        var builder = new JwtBuilder();

        Assert.Throws<ArgumentNullException>(() => builder.WithJwtOptions(null));
    }

    [Fact]
    public void JwtBuilder_WithJwtUserClaims_WhenLoginIsMissing_Throws()
    {
        var builder = new JwtBuilder();

        Assert.Throws<ArgumentNullException>(() => builder.WithJwtUserClaims(new PersonWithRolesQueryResult()));
    }

    [Fact]
    public void JwtBuilder_WithClaimsAndUserClaims_BuildsExpectedIdentity()
    {
        var options = CreateOptions();
        var builder = new JwtBuilder();
        var person = new PersonWithRolesQueryResult
        {
            Login = "user-1",
            Name = "Test User"
        };
        person.Groups = new[]
        {
            new PersonRoleQueryResult { Group = "admin" },
            new PersonRoleQueryResult { Group = "reader" }
        };

        builder.WithJwtOptions(options)
            .WithJwtUserClaims(person)
            .WithJwtClaims();

        var identity = builder.GetClaimsIdentity();

        Assert.Equal("Test User", identity.FindFirst(ClaimTypes.Name)?.Value);
        Assert.Equal("user-1", identity.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal(new[] { "0", "1" }, identity.FindClaims("admin", "reader"));
        Assert.Contains(identity.Claims, claim => claim.Type == JwtRegisteredClaimNames.Jti);
        Assert.Contains(identity.Claims, claim => claim.Type == JwtRegisteredClaimNames.Nbf);
        Assert.Contains(identity.Claims, claim => claim.Type == JwtRegisteredClaimNames.Iat);
    }

    [Fact]
    public void JwtBuilder_GetUserToken_ProducesTokenAndMetadata()
    {
        var options = CreateOptions();
        var builder = new JwtBuilder();
        var person = new PersonWithRolesQueryResult { Login = "user-1", Name = "Test User" };

        builder.WithJwtOptions(options).WithJwtUserClaims(person);
        var result = builder.GetUserToken();

        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.Equal(options.NotBefore, result.Created);
        Assert.Equal(options.Expiration, result.Expires);
        Assert.True(builder.CheckTokenIsValid(result.Token));
    }

    [Fact]
    public void JwtBuilder_CheckTokenIsValid_WhenTokenIsMissing_Throws()
    {
        var builder = new JwtBuilder();

        Assert.Throws<SecurityTokenException>(() => builder.CheckTokenIsValid(""));
    }

    [Fact]
    public void JwtBuilder_CheckTokenIsValid_WhenTokenIsInvalid_ReturnsFalse()
    {
        var options = CreateOptions();
        var builder = new JwtBuilder();

        builder.WithJwtOptions(options);

        Assert.False(builder.CheckTokenIsValid("invalid-token"));
    }

    [Fact]
    public void JwtBuilder_ToUnixEpochDate_ReturnsExpectedValue()
    {
        var builder = new JwtBuilder();
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 10, DateTimeKind.Utc);

        Assert.Equal(10, builder.ToUnixEpochDate(dateTime));
    }

    [Fact]
    public void ControllerCustomAuthorizationRequirement_StoresClaimConfiguration()
    {
        var requirement = new ControllerCustomAuthorizationRequirement("permission", "read", "write");

        Assert.Equal("permission", requirement.ClaimType);
        Assert.Equal(new[] { "read", "write" }, requirement.ClaimValues);
    }

    private static JwtTokenOptions CreateOptions()
    {
        return new JwtTokenOptions
        {
            Issuer = "issuer",
            Audience = "audience",
            SecretKey = SecretKey,
            ValidFor = TimeSpan.FromMinutes(30)
        };
    }
}

internal static class ClaimsIdentityTestExtensions
{
    public static string[] FindClaims(this ClaimsIdentity identity, params string[] claimTypes)
    {
        return identity.Claims
            .Where(claim => claimTypes.Contains(claim.Type))
            .Select(claim => claim.Value)
            .ToArray();
    }
}
