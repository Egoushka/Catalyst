using System.Text.Json.Serialization;

namespace Catalyst.Common.Models;

public record PaginationResponse<T> : BaseResponse<T>, IPaginationResponse
{
    public new IReadOnlyList<T> Data { get; init; } = [];
    public int PageSize { get; init; }
    public int PageNumber { get; init; }

    [JsonIgnore]
    public int TotalCount { get; init; }
}