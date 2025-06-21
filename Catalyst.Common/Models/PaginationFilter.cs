namespace Catalyst.Common.Models;

public record PaginationFilter : IPagination
{
    public required int PageNumber { get; init; } = 1;
    public required int PageSize { get; init; } = 10;

    public int Skip => (PageNumber - 1) * PageSize;
    public int Take => PageSize;
}