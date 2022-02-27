using System;
using Nuuvify.CommonPack.Security.Jwt;
using Microsoft.Extensions.Configuration;
using Xunit;
using Nuuvify.CommonPack.Security.Abstraction;

namespace Nuuvify.CommonPack.Security.xTest
{
    public class GerarValidarTokenUsuarioTests
    {

        private JwtTokenOptions _jwtOptions;
        private readonly IConfiguration config;


        public GerarValidarTokenUsuarioTests()
        {
            string projectPath = AppDomain.CurrentDomain.BaseDirectory.Split(new String[] { @"bin\" }, StringSplitOptions.None)[0];

            config = new ConfigurationBuilder()
               .SetBasePath(projectPath)
               .AddJsonFile("configTest.json")
               .Build();


            _jwtOptions = new JwtTokenOptions
            {
                SecretKey = config.GetSection("JwtTokenOptions:SECRET_KEY")?.Value,
                Issuer = config.GetSection("JwtTokenOptions:Issuer")?.Value,
                Audience = config.GetSection("JwtTokenOptions:Audience")?.Value
            };

        }


        [Fact]
        [Trait("Nuuvify.CommonPack.Security", nameof(JwtBuilder))]
        public void DeveObterUmTokenValidoVerdadeiro()
        {

            var fakeCws = new PersonWithRolesQueryResult
            {
                Login = "zocatel",
                Email = "teste@zzz.com",
                Name = "Fulano de Tal"
            };


            var jwtBuilder = new JwtBuilder()
                .WithJwtOptions(_jwtOptions)
                .WithJwtUserClaims(fakeCws);
                
            var userToken = jwtBuilder.GetUserToken();

            Assert.NotNull(userToken);

        }

        

    }
}
