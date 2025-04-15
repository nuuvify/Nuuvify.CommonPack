using System.Text;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Nuuvify.CommonPack.UnitOfWork.Migrations;

public abstract class CustomViewSqlOperation : SqlOperation
{

    /// <summary>
    /// Crie uma constante para usar em:
    /// <para>
    /// <c>
    /// builder.ToView($"{SuaViewOperation.ViewName}");
    /// </c>
    /// </para>
    /// </summary>
    /// <value></value>
    public abstract string ViewName { get; }
    public abstract string Schema { get; }
    public abstract string UserGrant { get; }

    public override string ToString()
    {
        if (string.IsNullOrWhiteSpace(Schema))
        {
            return ViewName;
        }
        else
        {
            return $"{Schema}.{ViewName}";
        }
    }

    public abstract StringBuilder GetViewBuilder();

    public abstract StringBuilder Grant();

}
