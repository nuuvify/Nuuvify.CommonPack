using System.Collections;
using Nuuvify.CommonPack.Extensions.Implementation;
using Xunit;

namespace Nuuvify.CommonPack.Domain.xTest;

public class CollectionsGenericsIsNullTests
{
    [Fact]
    [Trait("CommonPack.Extensions", nameof(ValidatedNotNullExtensionMethods))]
    public void IEnumerableIsnull()
    {

        IEnumerable ienumerable = null;

        var isNotNull = ienumerable.NotNullOrZero();

        Assert.False(isNotNull, "IEnumerable deveria ser null, mas retornou NotNull");

    }

    [Fact]
    [Trait("CommonPack.Extensions", nameof(ValidatedNotNullExtensionMethods))]
    public void IEnumerableIsNull_1()
    {

        IEnumerable ienumerable = Enumerable.Empty<string>();

        var isNotNull = ienumerable.NotNullOrZero();

        Assert.False(isNotNull, "IEnumerable deveria ser null, mas renornou NotNulll");

    }

    [Fact]
    [Trait("CommonPack.Extensions", nameof(ValidatedNotNullExtensionMethods))]
    public void IEnumerableIsZero()
    {

        IEnumerable ienumerable = Enumerable.Empty<string>();

        var isNotZero = ienumerable.NotNullOrZero();

        Assert.False(isNotZero, "IEnumerable deveria ser Zero, mas retornou NotZero");

    }

    [Fact]
    [Trait("CommonPack.Extensions", nameof(ValidatedNotNullExtensionMethods))]
    public void IEnumerableListIsZero()
    {

        IEnumerable ienumerable = new List<string>();

        var isNotZero = ienumerable.NotNullOrZero();

        Assert.False(isNotZero, "IEnumerable deveria ser Zero, mas retornou NotZero");

    }

    [Fact]
    [Trait("CommonPack.Extensions", nameof(ValidatedNotNullExtensionMethods))]
    public void ListIsZero()
    {

        var list = new List<string>();

        var isNotZero = list.NotNullOrZero();

        Assert.False(isNotZero, "List deveria ser Zero, mas retornou NotZero");

    }

    [Fact]
    [Trait("CommonPack.Extensions", nameof(ValidatedNotNullExtensionMethods))]
    public void IEnumerableNotIsNull()
    {

        IEnumerable ienumerable = new List<string> { "aaa", "bbb", "ccc" };

        var isNotNull = ienumerable.NotNullOrZero();

        Assert.True(isNotNull, "IEnumerable não deveria ser NotNull, mas retornou Null");

    }

    [Fact]
    [Trait("CommonPack.Extensions", nameof(ValidatedNotNullExtensionMethods))]
    public void AnyoneListNotIsZero()
    {
        var list = new List<string> { "aaa", "bbb", "ccc" };

        var anyone = list.ToHashSet();

        var isNotZero = anyone.NotNullOrZero();

        Assert.True(isNotZero, "Lista não deveria ser Zero, mas retornou Zero");

    }

    [Fact]
    [Trait("CommonPack.Extensions", nameof(ValidatedNotNullExtensionMethods))]
    public void AnyoneListIsZero()
    {
        var list = new List<string>();

        var anyone = list.ToHashSet();

        var isNotZero = anyone.NotNullOrZero();

        Assert.False(isNotZero, "Lista deveria ser Zero, mas retornou NotZero");

    }

    [Fact]
    [Trait("CommonPack.Extensions", nameof(ValidatedNotNullExtensionMethods))]
    public void ArrayIsNull()
    {
        object[] arrayNull = null;

        var isNotZero = arrayNull.NotNullOrZero();

        Assert.False(isNotZero, "Array deveria ser null, mas retornou NotNull");

    }

    [Fact]
    [Trait("CommonPack.Extensions", nameof(ValidatedNotNullExtensionMethods))]
    public void ArrayNotIsNull()
    {
        object[] array = new object[] { "a", "b" };

        var isNotZero = array.NotNullOrZero();

        Assert.True(isNotZero, "Array não deveria ser null, mas retornou Null");

    }

}
