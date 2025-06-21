using JetBrains.Annotations;

namespace Catalyst.Common.Settings;

[UsedImplicitly]
public sealed class SerilogOptions
{
    public required SerilogUsingOptions[] Using { get; set; } = [];
    public required string MinimumLevel { get; set; } = "Information";
    public required SerilogWriteToOptions[] WriteTo { get; set; } = [];
    public required string[] Enrich { get; set; } = [];
    public required Dictionary<string, string> Properties { get; set; } = new();
}