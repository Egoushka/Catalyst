namespace Catalyst.Common.Models;

public interface IPagination
{
    int PageNumber { get; init; }
    int PageSize { get; init; }

    int Skip => (PageNumber - 1) * PageSize;
    int Take => PageSize;
}