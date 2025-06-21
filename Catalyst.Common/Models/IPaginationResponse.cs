namespace Catalyst.Common.Models;

public interface IPaginationResponse : IPagination
{
    int TotalCount { get; init; }
    int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}