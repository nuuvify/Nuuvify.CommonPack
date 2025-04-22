using System.Globalization;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Nuuvify.CommonPack.Domain.xTest.FluentValidator;

[Order(5)]
public class AssertNotAreEqualsTests : NotifiableR
{

    public AssertNotAreEqualsTests()
    {
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");

        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
        CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");

        RemoveNotifications();
    }

    [Fact]
    public void AssertNotAreEqualsObjectNull()
    {
        Customer customer = null;

        var valido = new ValidationConcernR<Customer>(customer)
            .AssertNotAreEquals(x => x.Name == "João", true);

        Assert.Null(customer);
        Assert.True(valido.Errors.Count > 0);
    }

    [Fact]
    public void AssertNotAreEqualsObjectNotNull()
    {
        Customer customer = new Customer
        {
            Name = "João"
        };

        _ = new ValidationConcernR<Customer>(customer)
            .AssertNotAreEquals(x => x.Name == "João", true);

        Assert.NotNull(customer);
        Assert.True(customer.Notifications.Count > 0);
    }

    [Fact]
    public void ObjectNaoNuloComPropriedadeNaoIgualDeveRetornarErros()
    {
        Customer customer = new Customer
        {
            Name = "Pedro"
        };

        _ = new ValidationConcernR<Customer>(customer)
            .AssertNotAreEquals(x => x.Name == "João", false)
            .AssertNotDateTimeNull(x => customer.Birthdate);

        Assert.NotNull(customer);
        Assert.True(customer.Notifications.Count > 0);
    }

}
