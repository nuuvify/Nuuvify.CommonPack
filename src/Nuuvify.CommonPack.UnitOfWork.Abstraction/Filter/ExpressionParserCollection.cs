using System.Linq.Expressions;

namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Filter;

internal class ExpressionParserCollection : List<ExpressionParser>
{
    public ParameterExpression ParameterExpression { get; set; }

    public List<ExpressionParser> Ordered()
    {
        return [.. this.OrderBy(b => b.Criteria.UseOr)];
    }
}
