namespace System.Linq.Expressions;

public static class ExpressionExtension
{

    /// <summary>
    /// CombineExpressions deve ser utilizado para gerar WHERE dinamicamente para o Entity Framework,
    /// <p>Veja aqui um exemplo de uso:</p> 
    /// </summary>
    /// <example>
    /// <code>
    /// public Expression{Func{GfciFci, bool}} GetFilter()
    /// {
    ///     Expression{Func{GfciFci, bool}} filter = p => true;
    /// 
    ///     filter = p => p.Periodo == Periodo ee
    ///              p.MercCodigo == CodigoMercadoria ee
    ///              p.Estabelicimento == Estabelecimento;
    /// 
    ///     if (TipoMaterial.NotNullOrZero())
    ///     {
    ///         filter = filter.CombineExpressions{GfciFci}(
    ///             p => TipoMaterial.Contains(p.TipoMaterial));
    ///     }
    /// 
    ///     if (OM.NotNullOrZero())
    ///     {
    ///         filter = filter.CombineExpressions{GfciFci}(
    ///             p => OM.Contains(p.OmCodigo));
    ///     }
    /// 
    ///     return filter;
    /// }
    /// </code>
    /// No EFCore, usando o método acima no metodo WHERE poderia ser no LINQ também:
    /// <code> 
    ///  var query = _context.GfciFcis.AsNoTracking()
    ///      .Where(obtemMercadoriaFciQuery.GetFilter())
    ///      .OrderByDescending(p => p.Periodo)
    ///      .ThenBy(p => p.MercCodigo);
    /// 
    ///  var result = await query.ToListAsync();
    /// </code>
    /// </example>
    /// <param name="expr1"></param>
    /// <param name="expr2"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Expression<Func<T, bool>> CombineExpressions<T>(this
        Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var parameter = Expression.Parameter(typeof(T), "param");

        var parameterMap = new Dictionary<ParameterExpression, ParameterExpression>
        {
            { expr1.Parameters[0], parameter },
            { expr2.Parameters[0], parameter }
        };

        var body1 = ReplaceParameters(parameterMap, expr1.Body);
        var body2 = ReplaceParameters(parameterMap, expr2.Body);

        var body = Expression.AndAlso(body1, body2);

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static Expression ReplaceParameters(
        IDictionary<ParameterExpression, ParameterExpression> map,
        Expression exp)
    {
        return new ReplaceParameterVisitor(map).Visit(exp);
    }

    private class ReplaceParameterVisitor : ExpressionVisitor
    {
        private readonly IDictionary<ParameterExpression, ParameterExpression> _map;

        public ReplaceParameterVisitor(IDictionary<ParameterExpression, ParameterExpression> map)
        {
            _map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (_map.TryGetValue(node, out var replacement))
            {
                node = replacement;
            }
            return base.VisitParameter(node);
        }
    }
}
