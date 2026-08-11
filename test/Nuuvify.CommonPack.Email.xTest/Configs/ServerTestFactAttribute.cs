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
        if (IsCiEnvironment())
        {
            return false;
        }

        var machineName = Environment.MachineName;
        return !machineName.StartsWith("B8", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCiEnvironment()
    {
        var githubActions = Environment.GetEnvironmentVariable("GITHUB_ACTIONS");
        var ci = Environment.GetEnvironmentVariable("CI");

        return string.Equals(githubActions, "true", StringComparison.OrdinalIgnoreCase)
               || string.Equals(ci, "true", StringComparison.OrdinalIgnoreCase);
    }
}
