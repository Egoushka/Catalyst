using JetBrains.Annotations;

namespace Catalyst.Common.Settings;

[UsedImplicitly]
public sealed class BaseInfoOptions
{
    public required string AppName { get; set; } = "Application.API";
    public required string AppVersion { get; set; } = "0.0.1";
}