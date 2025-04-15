using Xunit;

namespace Nuuvify.CommonPack.UnitOfWork.PostgreSQL.xTest;

public sealed class PostgreSQLTestFactAttribute : FactAttribute
{
    public PostgreSQLTestFactAttribute()
    {
        if (!IsPostgreSQLContext())
        {
            Skip = "Ignore test Database PostgreSQL";
        }
    }

    private static bool IsPostgreSQLContext()
    {
        var config = AppSettingsConfig.GetConfig();
        var database = config.GetSection("TestOptions:DataBaseTestePostgreSQL")?.Value;

        return database.Equals("true", StringComparison.OrdinalIgnoreCase);
    }
}
