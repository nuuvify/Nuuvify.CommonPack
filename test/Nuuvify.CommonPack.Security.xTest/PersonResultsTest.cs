using Nuuvify.CommonPack.Security.Abstraction;
using Xunit;

namespace Nuuvify.CommonPack.Security.xTest
{
    public class PersonResultTest
    {


        [Fact]
        [Trait("Nuuvify.CommonPack.Extensions.Implementation", "PersonResult")]
        public void PersonQueryResult_DeveRetornarEmailInformado()
        {
            const string Email = "teste@zzz.com";

            var personResult = new PersonQueryResult()
            {
                Email = Email
            };


            Assert.Equal(personResult.Email, Email);

        }

        [Fact]
        [Trait("Nuuvify.CommonPack.Extensions.Implementation", "PersonResult")]
        public void PersonQueryResult_DeveRetornarEmailNullCasoNaoForInformado()
        {

            var personResult = new PersonQueryResult()
            {
                Login = "lalalala",
                Name = "Giropopis"
            };


            Assert.Null(personResult.Email);

        }

    }

}