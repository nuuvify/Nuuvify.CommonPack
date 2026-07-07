using Nuuvify.CommonPack.MftMailbox.Configuration;
using Nuuvify.CommonPack.MftMailbox.Utilities;

namespace Nuuvify.CommonPack.MftMailbox.xTest.Utilities;

[Trait("Category", "Unit")]
public class ResilienceGateTests
{
    [Fact]
    public void DeveAbrirCircuitoAposThreshold()
    {
        var gate = new ResilienceGate(new CircuitBreakerOptions
        {
            FailureThreshold = 2,
            BreakDuration = TimeSpan.FromSeconds(30)
        });

        gate.RegisterFailure();
        gate.RegisterFailure();

        Assert.Throws<InvalidOperationException>(() => gate.EnsureCanExecute());
    }
}
