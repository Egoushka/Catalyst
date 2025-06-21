namespace Catalyst.Common.Models;

public record BaseResponse<T>
{
    public Guid CorrelationId { get; set; }
    public T Data { get; set; } = default!;
}

public record BaseResponse : BaseResponse<object>;