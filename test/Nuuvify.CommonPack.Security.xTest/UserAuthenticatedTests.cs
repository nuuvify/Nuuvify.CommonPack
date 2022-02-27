using System;
using Nuuvify.CommonPack.Security.Abstraction;
using Nuuvify.CommonPack.Security.Helpers;
using Nuuvify.CommonPack.Security.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Internal;
using Moq;
using System.IO;

namespace Nuuvify.CommonPack.Security.xTest
{
    public class UserAuthenticatedTests
    {

        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly DefaultHttpContext _context;
        private readonly IConfiguration _config;

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


            string projectPath = AppDomain.CurrentDomain.BaseDirectory.Split(new String[] { @"bin\" }, StringSplitOptions.None)[0];

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


        private IDistributedCache GetSqlServerCache(SqlServerCacheOptions options = null)
        {
            if (options == null)
            {
                options = GetCacheOptions();
            }

            return new SqlServerCache(options);
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
}
