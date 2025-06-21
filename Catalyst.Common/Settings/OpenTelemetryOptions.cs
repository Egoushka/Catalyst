using JetBrains.Annotations;

namespace Catalyst.Common.Settings;

[UsedImplicitly]
public sealed class OpenTelemetryOptions
{
    public string? JaegerEndpoint { get; set; }
    public string? PrometheusEndpoint { get; set; }
    public string? OtlpEndpoint { get; set; }
}