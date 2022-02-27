using Polly;

namespace Nuuvify.CommonPack.StandardHttpClient.Polly
{
    public static class PollyCustomExtensions
    {

        public const string ServiceNameKey = "ServiceName";

        public static string GetServiceName(this Context context)
        {
            if (context.TryGetValue(ServiceNameKey, out object value))
            {
                return value.ToString();
            }

            return string.Empty;
        }

        public static void SetServiceName(this Context context, string serviceName)
        {
            if (!string.IsNullOrWhiteSpace(serviceName))
            {
                if (context.TryGetValue(ServiceNameKey, out object value))
                {
                    context.Remove(ServiceNameKey);
                }
                context.Add(ServiceNameKey, serviceName);

            }
        }

    }
}