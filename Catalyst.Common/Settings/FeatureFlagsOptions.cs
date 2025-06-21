using JetBrains.Annotations;

namespace Catalyst.Common.Settings;

[UsedImplicitly]
public sealed class FeatureFlagsOptions
{
    public bool EnableExperimentalRouting { get; set; } = false;
    public bool UseCacheForSchedule { get; set; } = true;
    public bool MaintenanceMode { get; set; } = false;
    public bool NewPaymentGateway { get; set; } = false;
}