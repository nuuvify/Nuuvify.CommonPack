using Nuuvify.CommonPack.Mediator.Implementation;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Nuuvify.CommonPack.Domain.xTest.FluentValidator;

[Order(6)]
public class AssertIsNullTests : NotifiableR
{

    public AssertIsNullTests()
    {
        RemoveNotifications();
    }

    [Fact]
    public void Para_AssertIsNull_ObjetoNuloConsultandoOutrasPropriedadesDeveGerarError()
    {
        Customer customer = null;

        var valido = new ValidationConcernR<Customer>(customer)
            .AssertIsNull(customer)
            .AssertIsTelephone(x => x.Telephone);

        Assert.Null(customer);
        Assert.True(valido.Errors.Count > 0);
    }

    [Fact]
    public void QuandoClasseForNulaRetornaErroParaVariavel_e_ConverteParaAddNotificationR()
    {
        Customer customer = null;

        var valido = new ValidationConcernR<AssertIsNullTests>(this)
            .AssertNotIsNull<Customer>(customer)
            .AssertIsTelephone(x => customer.Telephone);

        AddNotifications(valido.Errors);

        Assert.Null(customer);
        Assert.True(Notifications.Count > 0);
    }

    [Fact]
    public void Para_AssertIsNull_ObjetoNuloNaoDeveGerarError()
    {
        Customer customer = null;

        var valido = new ValidationConcernR<Customer>(customer)
            .AssertIsNull(customer);

        Assert.Null(customer);
        Assert.True(valido.Errors.Count == 0);
    }

    [Fact]
    public void Para_AssertIsNull_ObjetoNaoNuloConsultandoOutrasPropriedadesDeveGerarError()
    {
        Customer customer = new Customer();
        customer.Telephone = "19-98199-9977";

        _ = new ValidationConcernR<Customer>(customer)
            .AssertIsNull(customer)
            .AssertIsTelephone(x => x.Telephone);

        Assert.NotNull(customer);
        Assert.True(customer.Notifications.Count > 0);
    }

    [Fact]
    public void Para_AssertIsNull_ObjectoNaoNuloDeveGerarError()
    {
        Customer customer = new Customer();
        customer.Telephone = "19-98199-9977";

        _ = new ValidationConcernR<Customer>(customer)
           .AssertIsTelephone(x => x.Telephone)
           .AssertIsNull(customer);

        Assert.NotNull(customer);
        Assert.True(customer.Notifications.Count > 0);
    }

    [Fact]
    public void Para_AssertNotIsNull_QuandoOutraClasseForNulaDeveRetornarErrors()
    {

        Customer customer = new Customer();
        customer.Telephone = "19-98199-9977";

        OtherClass otherClass = null;

        _ = new ValidationConcernR<Customer>(customer)
            .AssertIsTelephone(x => x.Telephone)
            .AssertNotIsNull(otherClass);

        Assert.Null(otherClass);
        Assert.True(customer.Notifications.Count == 1);
    }

    [Fact]
    public void Para_AssertNotIsNull_QuandoOutraClasseForNulaDeveRetornarErrorsEmThis()
    {

        Customer customer = new Customer();
        customer.Telephone = "19-98199-9977";

        OtherClass otherClass = null;

        _ = new ValidationConcernR<AssertIsNullTests>(this)
            .AssertIsTelephone(x => customer.Telephone)
            .AssertNotIsNull(otherClass);

        Assert.Null(otherClass);
        Assert.True(customer.Notifications.Count == 0);
        Assert.True(Notifications.Count == 1);
    }

    [Fact]
    public void Para_AssertNotIsNull_QuandoClasseForNulaDeveRetornarErroPersonalizadoEmThis()
    {

        var messageExpected = "Pedido não cadastrado";
        OtherClass otherClass = null;

        _ = new ValidationConcernR<AssertIsNullTests>(this)
            .AssertNotIsNull(otherClass, messageExpected);

        Assert.Null(otherClass);
        Assert.True(Notifications.Count == 1);
        Assert.Equal(messageExpected, Notifications.FirstOrDefault().Message);
    }

    [Fact]
    public void Para_AssertNotIsNull_QuandoClasseForNulaDeveRetornarErroParaVariavel()
    {

        var messageExpected = "Pedido não cadastrado";
        OtherClass otherClass = null;

        var valid = new ValidationConcernR<OtherClass>(otherClass)
            .AssertNotIsNull(otherClass, messageExpected);

        Assert.Null(otherClass);
        Assert.True(Notifications.Count == 0);
        Assert.Equal(messageExpected, valid.Errors.FirstOrDefault().Message);
    }
    [Fact]
    public void Para_AssertIsNull_QuandoClasseNaoForNulaDeveRetornarErroParaThis()
    {

        var messageExpected = "Pedido não deveria existir";
        OtherClass otherClass = new OtherClass
        {
            Name = "Lincoln Zocateli"
        };

        _ = new ValidationConcernR<AssertIsNullTests>(this)
            .AssertIsNull(otherClass, messageExpected);

        Assert.NotNull(otherClass);
        Assert.True(Notifications.Count == 1);
        Assert.Equal(messageExpected, Notifications.FirstOrDefault().Message);
    }

    [Fact]
    public void Para_AssertIsNull_QuandoClasseNaoForNulaDeveRetornarErroParaPropriaClasse()
    {

        var messageExpected = "Customer não cadastrado";
        OtherClass otherClass = new OtherClass
        {
            Name = "Lincoln Zocateli"
        };

        Customer customer = null;

        new ValidationConcernR<OtherClass>(otherClass)
            .AssertIsNull(otherClass)
            .AssertNotIsNull(customer, messageExpected)
            .AssertHasMinLength(x => customer.Name, 10)
            .AssertIsGreaterOrEqualsThan(x => customer.Height, 10D)
            .RemoveSelectorMessage();

        var messageActual = otherClass.Notifications.FirstOrDefault(x => x.Message.Equals(messageExpected, StringComparison.Ordinal)).Message;

        Assert.NotNull(otherClass);
        Assert.True(otherClass.Notifications.Count == 2);
        Assert.Equal(messageExpected, messageActual);
    }
    [Fact]
    public void Para_AssertIsNull_QuandoOutraClasseForNulaNaoRetornarErrors()
    {

        Customer customer = new Customer();
        customer.Telephone = "19-98199-9977";

        OtherClass otherClass = null;

        _ = new ValidationConcernR<Customer>(customer)
            .AssertIsTelephone(x => x.Telephone)
            .AssertIsNull(otherClass);

        Assert.Null(otherClass);
        Assert.True(customer.Notifications.Count == 0);
    }

    [Fact]
    public void Para_AssertIsNull_QuandoOutraClasseNaoNulaRetornarErrors()
    {

        Customer customer = new Customer();
        customer.Telephone = "19-98199-9977";

        OtherClass otherClass = new OtherClass
        {
            Name = "Lincoln Zocateli"
        };

        _ = new ValidationConcernR<Customer>(customer)
            .AssertIsTelephone(x => x.Telephone)
            .AssertIsNull(otherClass);

        Assert.NotNull(otherClass);
        Assert.True(customer.Notifications.Count > 0);
    }

    [Fact]
    public void Para_AssertNotIsNull_QuandoOutraClasseNaoNulaNaoRetornarErrors()
    {

        Customer customer = new Customer();
        customer.Telephone = "19-98199-9977";

        OtherClass otherClass = new OtherClass
        {
            Name = "Lincoln Zocateli"
        };

        _ = new ValidationConcernR<Customer>(customer)
            .AssertIsTelephone(x => x.Telephone)
            .AssertNotIsNull(otherClass);

        Assert.NotNull(otherClass);
        Assert.True(customer.Notifications.Count == 0);
    }

}
public class OtherClass : NotifiableR
{
    public string Name { get; set; }
}
