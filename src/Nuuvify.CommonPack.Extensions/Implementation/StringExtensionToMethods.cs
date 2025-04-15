namespace Nuuvify.CommonPack.Extensions.Implementation;

public static partial class StringExtensionMethods
{

    public static string ToUpperInvariantNotNull(this string value)
    {
        if (ValidatedNotNullExtensionMethods.NotNull<string>(value))
            return value.ToUpperInvariant();

        return null;
    }

    public static string ToUpperNotNull(this string value)
    {
        if (ValidatedNotNullExtensionMethods.NotNull<string>(value))
            return value.ToUpper();

        return null;
    }
    public static string ToLowerNotNull(this string value)
    {
        if (ValidatedNotNullExtensionMethods.NotNull<string>(value))
            return value.ToLower();

        return null;
    }
    public static string ToLowerInvariantNotNull(this string value)
    {
        if (ValidatedNotNullExtensionMethods.NotNull<string>(value))
            return value.ToLowerInvariant();

        return null;
    }

    public static string TrimNotNull(this string value)
    {
        if (ValidatedNotNullExtensionMethods.NotNull<string>(value))
            return value.Trim();

        return null;
    }
    public static string ToStringNotNull(this string value)
    {
        if (ValidatedNotNullExtensionMethods.NotNull<string>(value))
            return value.ToString();

        return null;
    }

}
