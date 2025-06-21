using Catalyst.Data.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace Catalyst.Data;

public static class SpecificationEvaluator
{
    public static IQueryable<T> GetQuery<T>(
        IQueryable<T> inputQuery, // Should already be AsExpandable() if needed
        ISpecification<T> spec) where T : class
    {
        var query = inputQuery;

        // 1) Apply Criteria (essential for both list and count)
        if (spec.Criteria is not null)
        {
            // Assuming inputQuery is already AsExpandable(), Where should work with PredicateBuilder expressions
            query = query.Where(spec.Criteria);
        }

        // 2) Includes
        query = spec.Includes.Aggregate(query,
            (current, include) => current.Include(include));

        // 3) Ordering
        if (spec.OrderBy is not null)
            query = query.OrderBy(spec.OrderBy);
        else if (spec.OrderByDescending is not null)
            query = query.OrderByDescending(spec.OrderByDescending);

        // 4) Paging
        if (spec.Skip.HasValue)
            query = query.Skip(spec.Skip.Value);
        if (spec.Take.HasValue)
            query = query.Take(spec.Take.Value);

        // 5) Tracking (can apply to count too, though often less relevant)
        if (spec.AsNoTracking) // Apply AsNoTracking at the end
            query = query.AsNoTracking();


        return query;
    }
}