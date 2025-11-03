
using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace Nuuvify.CommonPack.Extensions;

public static class CacheTimeServiceExtension
{

    /// <summary>
    /// Retona um DateTimeOffset com a data e hora de expiração do cache em LocatDateTime
    /// </summary>
    /// <returns></returns>
    public static TimeSpan ExpireAt(IConfiguration confiruration, string configSection = "AppConfig:CacheExpire:Preco:Minute")
    {
        var timeValue = confiruration.GetSection(configSection)?.Value;
        if (double.TryParse(timeValue, out double time))
            return ExpireAt(time, CacheTime.minute);
        else
            return ExpireAt(timeValue);
    }
    /// <inheritdoc cref="ExpireAt(IConfiguration, string)"/>
    public static TimeSpan ExpireAt(string exactTime = "17:00:00")
    {
        _ = DateTimeOffset.TryParse(exactTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset expireTime);
        TimeSpan expireDiff = expireTime - DateTimeOffset.Now;
        if (expireDiff <= TimeSpan.Zero)
            expireTime = expireTime.AddDays(1);
        return expireTime - DateTimeOffset.Now;
    }

    /// <inheritdoc cref="ExpireAt(IConfiguration, string)"/>
    public static TimeSpan ExpireAt(double time, CacheTime cacheTime)
    {
        var expireTime = CalculateExpireTime(time, cacheTime);
        var expireDiff = expireTime - DateTimeOffset.Now;

        if (expireDiff <= TimeSpan.Zero)
        {
            time = GetValidTime(time);
            expireTime = DateTimeOffset.Now.AddSeconds(time);
        }

        return expireTime - DateTimeOffset.Now;
    }

    private static DateTimeOffset CalculateExpireTime(double time, CacheTime cacheTime)
    {
        return cacheTime switch
        {
            CacheTime.minute => DateTimeOffset.Now.AddMinutes(time),
            CacheTime.seconds => DateTimeOffset.Now.AddSeconds(time),
            CacheTime.miliseconds => DateTimeOffset.Now.AddMilliseconds(time),
            _ => DateTimeOffset.Now.AddHours(time)
        };
    }

    private static double GetValidTime(double time)
    {
        return Math.Abs(time) < 0.0001 ? 20 : time;
    }

}

public enum CacheTime
{
    minute = 1,
    seconds = 2,
    miliseconds = 3,
    hours = 4,

}
