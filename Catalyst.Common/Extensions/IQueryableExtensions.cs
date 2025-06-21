using Catalyst.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Catalyst.Common.Extensions;

public static class IQueryableExtensions
{
    public static async Task<PaginationResponse<TResult>> ToPaginatedResponseAsync<TInput, TResult>(
        this IQueryable<TInput> query,
        IPagination pagination,
        Func<TInput, TResult> mapper,
        CancellationToken ct = default)
    {
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip(pagination.Skip)
            .Take(pagination.Take)
            .OrderBy(x => x)
            .ToListAsync(ct);

        var data = items.Count > 0 ? items.Select(mapper).ToList() : Enumerable.Empty<TResult>().ToList();

        return new PaginationResponse<TResult>
        {
            Data = data,
            TotalCount = totalCount,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
    }
}