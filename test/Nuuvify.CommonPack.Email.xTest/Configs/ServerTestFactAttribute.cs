namespace Nuuvify.CommonPack.Email.xTest.Configs;

public sealed class ServerTestFactAttribute : FactAttribute
{
    public ServerTestFactAttribute()
    {
        if (!IsServerMachine())
        {
            Skip = "Ignore test in Local Machine";
        }
    }

    private static bool IsServerMachine()
    {
        var machineName = Environment.MachineName;
        return !machineName.StartsWith("B8", StringComparison.OrdinalIgnoreCase);
    }
}
