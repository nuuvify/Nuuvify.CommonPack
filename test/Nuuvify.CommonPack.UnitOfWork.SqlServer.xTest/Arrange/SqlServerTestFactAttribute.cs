using Xunit;

namespace Nuuvify.CommonPack.UnitOfWork.SqlServer.xTest;

public sealed class SqlServerTestFactAttribute : FactAttribute
{
    public SqlServerTestFactAttribute()
    {
        if (!IsSqlServerContext())
        {
            Skip = "Ignore test Database SqlServer";
        }
    }

    private static bool IsSqlServerContext()
    {
        var config = AppSettingsConfig.GetConfig();
        var machine = Environment.GetEnvironmentVariable("MACHINE");
        string database = "false";

        if (machine.StartsWith("B8", StringComparison.InvariantCultureIgnoreCase))
        {
            database = config.GetSection("TestOptions:DataBaseTesteSqlServer")?.Value;
        }
        return database.Equals("true", StringComparison.OrdinalIgnoreCase);
    }
}
