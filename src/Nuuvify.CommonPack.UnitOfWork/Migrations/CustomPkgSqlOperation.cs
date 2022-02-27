using System.Text;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Nuuvify.CommonPack.UnitOfWork.Migrations
{
    public abstract class CustomPkgSqlOperation : SqlOperation
    {

        /// <summary>
        /// Packages são utilizados dentro das functions ou procedures, ou, em alguns
        /// casos como uma Tabela ou View
        /// </summary>
        /// <value></value>
        public abstract string PkgName { get; }
        public abstract string Schema { get; }
        public abstract string UserGrant { get; }


        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Schema))
            {
                return PkgName;
            }
            else
            {
                return $"{Schema}.{PkgName}";
            }
        }

        public abstract StringBuilder GetPkgBuilder();
        public abstract StringBuilder Grant();

    }
}