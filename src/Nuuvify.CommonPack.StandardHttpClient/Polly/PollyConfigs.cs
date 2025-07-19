
namespace Nuuvify.CommonPack.StandardHttpClient.Polly
{
    public interface ICircuitBreakerPolicyConfig
    {
        int RetryCount { get; set; }
        int BreakDurationMilliSeconds { get; set; }
    }

    public interface IRetryPolicyConfig
    {
        int RetryCount { get; set; }
    }

    public class PolicyConfig : ICircuitBreakerPolicyConfig, IRetryPolicyConfig
    {
        public int RetryCount { get; set; }
        public int BreakDurationMilliSeconds { get; set; }
    }
}