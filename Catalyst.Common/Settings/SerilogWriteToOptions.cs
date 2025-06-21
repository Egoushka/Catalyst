using JetBrains.Annotations;

namespace Catalyst.Common.Settings;

[UsedImplicitly]
public sealed class SerilogWriteToOptions
{
    public string? Name { get; set; }
    public Dictionary<string, object> Args { get; set; } = new();
}