using Nuuvify.CommonPack.UnitOfWork.Abstraction.Enums;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Extensions;
using Xunit;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Tests;

[Trait("Category", "Unit")]
public sealed class SortingExtensionsTest
{
    [Fact]
    public void SortCommandExtensions_ToCommandString_ShouldReturnCorrectStrings()
    {
        // Act & Assert
        Assert.Equal("OrderBy", SortCommand.OrderBy.ToCommandString());
        Assert.Equal("OrderByDescending", SortCommand.OrderByDescending.ToCommandString());
        Assert.Equal("ThenBy", SortCommand.ThenBy.ToCommandString());
        Assert.Equal("ThenByDescending", SortCommand.ThenByDescending.ToCommandString());
    }

    [Fact]
    public void SortCommandExtensions_ToCommandString_WithInvalidEnum_ShouldThrow()
    {
        // Arrange
        var invalidCommand = (SortCommand)999;

        // Act & Assert
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => invalidCommand.ToCommandString());
    }
}