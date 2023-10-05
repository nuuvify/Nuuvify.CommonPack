using System.Security.Claims;

namespace Nuuvify.CommonPack.StandardHttpClient.xTest.Fixtures
{
    public class UserFixture
    {


        public ClaimsPrincipal GetUserPrincipalFake(string nameIdentifier = "cwsFake", string name = "Lincoln Zocateli")
        {

            var user = new ClaimsPrincipal(
                new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.NameIdentifier, nameIdentifier),
                    new Claim(ClaimTypes.Name, name)
                }, "TestAuthentication"));

            return user;

        }



    }
}