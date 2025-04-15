using Xunit;

namespace Nuuvify.CommonPack.UnitOfWork.Oracle.xTest;

public sealed class OracleTestFactAttribute : FactAttribute
{
    public OracleTestFactAttribute()
    {
        if (!IsOracleContext())
        {
            Skip = "Ignore test Database Oracle";
        }
    }

    private static bool IsOracleContext()
    {
        var config = AppSettingsConfig.GetConfig();

        var machine = Environment.GetEnvironmentVariable("MACHINE");
        string database = "false";

        if (machine.StartsWith("B8", StringComparison.InvariantCultureIgnoreCase))
        {
            database = config.GetSection("TestOptions:DataBaseTesteOracle")?.Value;
        }

        return database.Equals("true", StringComparison.OrdinalIgnoreCase);
    }
}
