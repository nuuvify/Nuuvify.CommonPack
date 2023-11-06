using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace Nuuvify.CommonPack.UnitOfWork.Migrations
{
    public static class ViewOperationBuilder
    {
        public static OperationBuilder<SqlOperation> CreateView<TView>(
            this MigrationBuilder migrationBuilder)
            where TView : CustomViewSqlOperation, new()
        {
            Console.WriteLine($"******* Migration: {nameof(ViewOperationBuilder.CreateView)} {typeof(TView).Name} *******");

            var view = new TView();

            var sql = view.GetViewBuilder();
            sql.Append(view.Grant());

            return migrationBuilder.Sql(sql.ToString());

        }

        public static OperationBuilder<SqlOperation> DropView<TView>(
            this MigrationBuilder migrationBuilder) 
            where TView : CustomViewSqlOperation, new()
        {

            Console.WriteLine($"******* Migration: {nameof(ViewOperationBuilder.DropView)} {typeof(TView).Name} *******");

            var view = new TView();

            switch (migrationBuilder.ActiveProvider)
            {
                case "Oracle.EntityFrameworkCore":
                    {
                        return migrationBuilder
                            .Sql($"DROP VIEW {view} cascade constraints;");
                    }
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    {
                        return migrationBuilder
                            .Sql($"DROP VIEW {view};");
                    }
            }

            throw new TypeLoadException($"Unexpected provider: {migrationBuilder.ActiveProvider}");
        }

    }
}