using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace Nuuvify.CommonPack.UnitOfWork.Migrations;

public static class TypeRowOperationBuilder
{
    public static OperationBuilder<SqlOperation> CreateTypeRow<TObject>(
        this MigrationBuilder migrationBuilder)
        where TObject : CustomObjectSqlOperation, new()
    {
        Console.WriteLine($"******* Migration: {nameof(TypeRowOperationBuilder.CreateTypeRow)} {typeof(TObject).Name} *******");

        var databaseObject = new TObject();

        switch (migrationBuilder.ActiveProvider)
        {
            case "Oracle.EntityFrameworkCore":
                {
                    return migrationBuilder.Sql(databaseObject.GetObjectBuilder().ToString());
                }
            case "Microsoft.EntityFrameworkCore.SqlServer":
                {
                    return migrationBuilder.Sql(databaseObject.GetObjectBuilder().ToString());
                }
        }

        throw new TypeLoadException($"Unexpected provider: {migrationBuilder.ActiveProvider}");
    }

    public static OperationBuilder<SqlOperation> DropTypeRow<TObject>(
        this MigrationBuilder migrationBuilder)
        where TObject : CustomObjectSqlOperation, new()
    {

        Console.WriteLine($"******* Migration: {nameof(TypeRowOperationBuilder.DropTypeRow)} {typeof(TObject).Name} *******");

        var databaseObject = new TObject();

        switch (migrationBuilder.ActiveProvider)
        {
            case "Oracle.EntityFrameworkCore":
                {
                    return migrationBuilder
                        .Sql($"DROP TYPE {databaseObject};");
                }
            case "Microsoft.EntityFrameworkCore.SqlServer":
                {
                    return migrationBuilder
                        .Sql($"DROP TYPE {databaseObject};");
                }
        }

        throw new TypeLoadException($"Unexpected provider: {migrationBuilder.ActiveProvider}");
    }

}
