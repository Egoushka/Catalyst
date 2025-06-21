using JetBrains.Annotations;

namespace Catalyst.Common.Settings;

[UsedImplicitly]
public sealed class AuthOptions
{
    public required string Authority { get; set; }
    public required string Audience { get; set; }
    public bool RequireHttpsMetadata { get; set; } = true;
}