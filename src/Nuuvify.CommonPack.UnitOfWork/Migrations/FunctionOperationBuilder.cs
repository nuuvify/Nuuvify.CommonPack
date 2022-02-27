using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace Nuuvify.CommonPack.UnitOfWork.Migrations
{
    public static class FunctionOperationBuilder
    {
        public static OperationBuilder<SqlOperation> CreateFunction<TFunction>(
            this MigrationBuilder migrationBuilder)
            where TFunction : CustomFunctionSqlOperation, new()
        {
            Console.WriteLine($"******* Migration: {nameof(FunctionOperationBuilder.CreateFunction)} {typeof(TFunction).Name} *******");

            var function = new TFunction();

            var sql = function.GetFunctionBuilder();
            sql.Append(function.Grant());

            return migrationBuilder.Sql(sql.ToString());
        }

        public static OperationBuilder<SqlOperation> DropFunction<TFunction>(
            this MigrationBuilder migrationBuilder)
            where TFunction : CustomFunctionSqlOperation, new()
        {

            Console.WriteLine($"******* Migration: {nameof(FunctionOperationBuilder.DropFunction)} {typeof(TFunction).Name} *******");

            var function = new TFunction();

            switch (migrationBuilder.ActiveProvider)
            {
                case "Oracle.EntityFrameworkCore":
                    {
                        return migrationBuilder
                            .Sql($"DROP FUNCTION {function};");
                    }
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    {
                        return migrationBuilder
                            .Sql($"DROP FUNCTION {function};");
                    }
            }

            throw new TypeLoadException($"Unexpected provider: {migrationBuilder.ActiveProvider}");
        }

    }
}