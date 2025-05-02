using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Internal;
using Moq;
using Nuuvify.CommonPack.Security.Abstraction;
using Nuuvify.CommonPack.Security.Helpers;
using Nuuvify.CommonPack.Security.Jwt;

namespace Nuuvify.CommonPack.Security.xTest;

public class UserAuthenticatedTests
{

    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly DefaultHttpContext _context;
    private readonly IConfiguration _config;
    private static readonly string[] separator = new String[] { @"bin\" };

    public UserAuthenticatedTests()
    {
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        _context = new DefaultHttpContext();

        var cwsComGrupo = new PersonWithRolesQueryResult
        {
            Email = "fulano@zzz.com",
            Login = "charopinho",
            Name = "Fulano de tal"
        };

        string projectPath = AppDomain.CurrentDomain.BaseDirectory.Split(separator, StringSplitOptions.None)[0];

        _config = new ConfigurationBuilder()
           .SetBasePath(projectPath)
           .AddJsonFile(Path.Combine(projectPath, "configTest.json"))
           .Build();

        var _jwtOptions = new JwtTokenOptions
        {
            SecretKey = _config.GetSection("JwtTokenOptions:SECRET_KEY")?.Value,
            Issuer = _config.GetSection("JwtTokenOptions:Issuer")?.Value,
            Audience = _config.GetSection("JwtTokenOptions:Audience")?.Value
        };

        var jwtClass = new JwtBuilder()
            .WithJwtOptions(_jwtOptions)
            .WithJwtUserClaims(cwsComGrupo);

        _context.User.AddIdentity(jwtClass.GetClaimsIdentity());

    }

    private SqlServerCacheOptions GetCacheOptions(ISystemClock testClock = null)
    {
        return new SqlServerCacheOptions()
        {
            ConnectionString = _config.GetConnectionString("SqlCacheTest"),
            SchemaName = "cache",
            TableName = "Tokens",
            SystemClock = testClock ?? new TestClock().Add(TimeSpan.FromMinutes(60)),
            ExpiredItemsDeletionInterval = TimeSpan.FromMinutes(60)
        };
    }

}

public class UserAuthenticatedCustom : UserAuthenticated
{
    public UserAuthenticatedCustom(IHttpContextAccessor accessor)
        : base(accessor)
    {
    }
}
