using System.Text;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Nuuvify.CommonPack.UnitOfWork.Migrations
{
    public abstract class CustomObjectSqlOperation : SqlOperation
    {

        /// <summary>
        /// Objects são utilizados dentro das functions ou procedures
        /// </summary>
        /// <value></value>
        public abstract string ObjectName { get; }
        public abstract string Schema { get; }
        public abstract string UserGrant { get; }


        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Schema))
            {
                return ObjectName;
            }
            else
            {
                return $"{Schema}.{ObjectName}";
            }
        }

        public abstract StringBuilder GetObjectBuilder();

    }
}