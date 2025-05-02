
using Microsoft.Extensions.Configuration;

namespace Nuuvify.CommonPack.Extensions;

public static class CacheTimeService
{

    /// <summary>
    /// Retona um DateTimeOffset com a data e hora de expiração do cache em LocatDateTime
    /// </summary>
    /// <returns></returns>
    public static DateTimeOffset ExpireAt(IConfiguration confiruration, string configSection = "AppConfig:CacheExpire:Preco:Minute")
    {
        var timeValue = confiruration.GetSection(configSection)?.Value;
        if (double.TryParse(timeValue, out double time))
            return ExpireAt(time, CacheTime.minute);
        else
            return ExpireAt(timeValue);
    }

    /// <inheritdoc cref="ExpireAt(IConfiguration, string)"/>
    public static DateTimeOffset ExpireAt(string exactTime = "17:00:00")
    {
        _ = DateTimeOffset.TryParse(exactTime, out DateTimeOffset expireTime);
        TimeSpan expireDiff = expireTime - DateTimeOffset.Now;
        if (expireDiff <= TimeSpan.Zero)
            expireTime = expireTime.AddDays(1);
        return expireTime.LocalDateTime;
    }

    /// <inheritdoc cref="ExpireAt(IConfiguration, string)"/>
    public static DateTimeOffset ExpireAt(double time, CacheTime cacheTime)
    {
        switch (cacheTime)
        {
            case CacheTime.minute:
                {
                    var expireTime = DateTimeOffset.Now.AddMinutes(time);
                    TimeSpan expireDiff = expireTime - DateTimeOffset.Now;
                    if (expireDiff <= TimeSpan.Zero)
                    {
                        time = Math.Abs(time) < 0.0001 ? 20 : time;
                        expireTime = DateTimeOffset.Now.AddSeconds(time);
                    }
                    return expireTime.LocalDateTime;
                }
            case CacheTime.seconds:
                {
                    var expireTime = DateTimeOffset.Now.AddSeconds(time);
                    TimeSpan expireDiff = expireTime - DateTimeOffset.Now;
                    if (expireDiff <= TimeSpan.Zero)
                    {
                        time = Math.Abs(time) < 0.0001 ? 20 : time;
                        expireTime = DateTimeOffset.Now.AddSeconds(time);
                    }
                    return expireTime.LocalDateTime;
                }
            case CacheTime.miliseconds:
                {
                    var expireTime = DateTimeOffset.Now.AddMilliseconds(time);
                    TimeSpan expireDiff = expireTime - DateTimeOffset.Now;
                    if (expireDiff <= TimeSpan.Zero)
                    {
                        time = Math.Abs(time) < 0.0001 ? 20 : time;
                        expireTime = DateTimeOffset.Now.AddSeconds(time);
                    }
                    return expireTime.LocalDateTime;
                }
            default:
                {
                    var expireTime = DateTimeOffset.Now.AddHours(time);
                    TimeSpan expireDiff = expireTime - DateTimeOffset.Now;
                    if (expireDiff <= TimeSpan.Zero)
                    {
                        time = Math.Abs(time) < 0.0001 ? 20 : time;
                        expireTime = DateTimeOffset.Now.AddSeconds(time);
                    }
                    return expireTime.LocalDateTime;
                }
        }
    }

    public enum CacheTime
    {
        minute = 1,
        seconds = 2,
        miliseconds = 3,
        hours = 4,

    }
}

