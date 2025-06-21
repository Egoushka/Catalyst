namespace Catalyst.Core;

public record BaseApiResponse<T>
{
    public required bool IsSuccess { get; set; }
    public required T Value { get; set; } = default!;
    public Guid CorrelationId { get; set; }
}