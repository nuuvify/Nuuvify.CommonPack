using System.Linq.Expressions;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Filter;
using Xunit;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Tests;

[Trait("Category", "Unit")]
public class ExpressionFactoryTest
{
    [Fact]
    public void GetClosureOverConstant_WithNullValue_ShouldReturnTypedNullConstant()
    {
        // Arrange
        string? nullValue = null;
        var targetType = typeof(string);

        // Act
        var result = ExpressionFactory.GetClosureOverConstant(nullValue, targetType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ExpressionType.Constant, result.NodeType);
        var constantExpression = (ConstantExpression)result;
        Assert.Null(constantExpression.Value);
        Assert.Equal(targetType, constantExpression.Type);
    }

    [Fact]
    public void GetClosureOverConstant_WithNonNullValue_ShouldReturnConstantExpression()
    {
        // Arrange
        var value = "test";
        var targetType = typeof(string);

        // Act
        var result = ExpressionFactory.GetClosureOverConstant(value, targetType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ExpressionType.Constant, result.NodeType);
        var constantExpression = (ConstantExpression)result;
        Assert.Equal(value, constantExpression.Value);
        Assert.Equal(targetType, constantExpression.Type);
    }

    [Fact]
    public void GetClosureOverConstant_WithTypeConversion_ShouldReturnConvertExpression()
    {
        // Arrange
        var value = 42;
        var targetType = typeof(long);

        // Act
        var result = ExpressionFactory.GetClosureOverConstant(value, targetType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ExpressionType.Convert, result.NodeType);
        var convertExpression = (UnaryExpression)result;
        Assert.Equal(targetType, convertExpression.Type);

        var operand = (ConstantExpression)convertExpression.Operand;
        Assert.Equal(value, operand.Value);
        Assert.Equal(typeof(int), operand.Type);
    }

    [Fact]
    public void GetClosureOverConstant_WithNullableDateTime_ShouldHandleCorrectly()
    {
        // Arrange
        var value = new DateTime(2023, 10, 28);
        var targetType = typeof(DateTime?);

        // Act
        var result = ExpressionFactory.GetClosureOverConstant(value, targetType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ExpressionType.Convert, result.NodeType);
        var convertExpression = (UnaryExpression)result;
        Assert.Equal(targetType, convertExpression.Type);
    }

    [Theory]
    [InlineData("string value", typeof(string))]
    [InlineData(123, typeof(int))]
    [InlineData(45.67, typeof(double))]
    [InlineData(true, typeof(bool))]
    public void GetClosureOverConstant_WithSameType_ShouldReturnDirectConstant(object value, Type expectedType)
    {
        // Act
        var result = ExpressionFactory.GetClosureOverConstant(value, expectedType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ExpressionType.Constant, result.NodeType);
        var constantExpression = (ConstantExpression)result;
        Assert.Equal(value, constantExpression.Value);
        Assert.Equal(expectedType, constantExpression.Type);
    }
}