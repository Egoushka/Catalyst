using JetBrains.Annotations;

namespace Catalyst.Common.Settings;

[UsedImplicitly]
public sealed class RateLimitingOptions
{
    public required int PermitLimit { get; set; } = 100;
    public required TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);
}