namespace Nuuvify.CommonPack.Extensions.Implementation;

public static class Constants
{
    /// <summary>
    /// </summary>
    /// <value>CorrelationId</value>
    public static string CorrelationHeader
    {
        get
        {
            return "CorrelationId";
        }
    }

    /// <summary>
    /// </summary>
    /// <value>x-user-claim</value>
    public static string UserClaimHeader
    {
        get
        {
            return "x-user-claim";
        }
    }

    /// <summary>
    /// </summary>
    /// <value>UserIsValidToApplication</value>
    public static string UserIsValidToApplication
    {
        get
        {
            return "UserIsValidToApplication";
        }
    }

}
