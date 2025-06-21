using JetBrains.Annotations;

namespace Catalyst.Common.Settings;

[UsedImplicitly]
public sealed class DatabaseOptions
{
    public required ConnectionStringsOptions ConnectionStrings { get; set; }
    public required int Timeout { get; set; } = 30;
}