using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace Nuuvify.CommonPack.UnitOfWork.Migrations
{
    public static class PkgOperationBuilder
    {
        public static OperationBuilder<SqlOperation> CreatePkg<TObject>(
            this MigrationBuilder migrationBuilder) 
            where TObject : CustomPkgSqlOperation, new() 
        {
            Console.WriteLine($"******* Migration: {nameof(PkgOperationBuilder.CreatePkg)} {typeof(TObject).Name} *******");

            var pkg = new TObject();

            switch (migrationBuilder.ActiveProvider)
            {
                case "Oracle.EntityFrameworkCore":
                    {
                        var sql = pkg.GetPkgBuilder();
                        sql.Append(pkg.Grant());

                        return migrationBuilder.Sql(sql.ToString());

                    }
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    {
                        throw new NotImplementedException("CreatePkg to SqlServer not Implemented");
                    }
            }

            throw new TypeLoadException($"Unexpected provider: {migrationBuilder.ActiveProvider}");
        }
        
        public static OperationBuilder<SqlOperation> DropPkg<TObject>(
            this MigrationBuilder migrationBuilder) 
            where TObject : CustomPkgSqlOperation, new() 
        {

            Console.WriteLine($"******* Migration: {nameof(PkgOperationBuilder.DropPkg)} {typeof(TObject).Name} *******");

            var pkg = new TObject();

            switch (migrationBuilder.ActiveProvider)
            {
                case "Oracle.EntityFrameworkCore":
                    {
                        return migrationBuilder
                            .Sql($"DROP PACKAGE {pkg};");
                    }
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    {
                        return migrationBuilder
                            .Sql($"DROP PACKAGE {pkg};");
                    }
            }

            throw new TypeLoadException($"Unexpected provider: {migrationBuilder.ActiveProvider}");
        }

    }
}