using System;

namespace Nuuvify.CommonPack.StandardHttpClient.Polly
{
    internal static class PollyHelpers
    {
        internal static TimeSpan ComputeDuration(double input)
        {
            return TimeSpan.FromSeconds(Math.Pow(2, input)) +
                   TimeSpan.FromMilliseconds(new Random().Next(0, 1000));
        }
        
    }
}