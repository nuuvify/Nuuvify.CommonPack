using System.Text;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Nuuvify.CommonPack.UnitOfWork.Migrations
{
    public abstract class CustomFunctionSqlOperation : SqlOperation
    {

        public abstract string FunctionName { get; }
        public abstract string Schema { get; }
        public abstract string UserGrant { get; }


        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Schema))
            {
                return FunctionName;
            }
            else
            {
                return $"{Schema}.{FunctionName}";
            }
        }

        public abstract StringBuilder GetFunctionBuilder();
        public abstract StringBuilder Grant();

    }
}