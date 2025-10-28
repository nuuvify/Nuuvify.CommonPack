using System.Linq.Expressions;

namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Filter;

internal class ExpressionParser
{
    public WhereClause Criteria { get; set; }
    public Expression FieldToFilter { get; set; }
    public Expression FilterBy { get; set; }
}
