using JetBrains.Annotations;

namespace Catalyst.Common.Settings;

[UsedImplicitly]
public sealed class ConnectionStringsOptions
{
    public string DefaultConnection { get; set; } = string.Empty;
    public string DefaultConnectionMSSQL { get; set; } = string.Empty;
    public string Redis { get; set; } = string.Empty;
}