namespace Catalyst.Core;

public record PaginatedApiResponse<T> : BaseApiResponse<IEnumerable<T>> // Inherit from BaseApiResponse
{
    // Properties from IPaginationResponse
    public int PageSize { get; init; }
    public int PageNumber { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }

    // Remove IsSuccess and Value, inherit from BaseApiResponse
    // public required bool IsSuccess { get; set; } = true;
    // public required IEnumerable<T> Value { get; set; } = []; // Now comes from BaseApiResponse<IEnumerable<T>>
}