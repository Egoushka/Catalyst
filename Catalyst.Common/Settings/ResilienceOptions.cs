using JetBrains.Annotations;

namespace Catalyst.Common.Settings;

[UsedImplicitly]
public sealed class ResilienceOptions
{
    public required int MaxRetries { get; set; } = 3;
    public required double CircuitBreakerThreshold { get; set; } = 0.5;
    public required TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}