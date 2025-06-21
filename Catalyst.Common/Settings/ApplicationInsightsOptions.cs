using JetBrains.Annotations;

namespace Catalyst.Common.Settings;

[UsedImplicitly]
public sealed class ApplicationInsightsOptions
{
    public string? ConnectionString { get; set; }
}