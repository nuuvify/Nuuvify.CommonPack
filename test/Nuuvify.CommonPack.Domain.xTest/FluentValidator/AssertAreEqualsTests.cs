using Xunit;
using Xunit.Extensions.Ordering;

namespace Nuuvify.CommonPack.Domain.xTest.FluentValidator
{
    [Order(7)]
    public class AssertAreEqualsTests
    {


        [Fact]
        public void AssertAreEqualsObjectNull()
        {
            Customer customer = null;

            var valido = new ValidationConcernR<Customer>(customer)
                .AssertAreEquals(x => x.Name == "João", true);



            Assert.Null(customer);
            Assert.True(valido.Errors.Count > 0);
        }

        [Fact]
        public void AssertAreEqualsObjectNotNull()
        {
            Customer customer = new Customer
            {
                Name = "João",
                Age = 41
            };

            new ValidationConcernR<Customer>(customer)
                .AssertAreEquals(x => x.Name, "João")
                .AssertAreEquals(x => x.Age + 10 == 50, true);


            Assert.NotNull(customer);
            Assert.True(customer.Notifications.Count == 1);
        }
    }
}
