using System.Linq.Expressions;

namespace Catalyst.Data.Abstraction;

public interface ISpecification<T>
{
    // A LINQ predicate to apply as a WHERE
    Expression<Func<T, bool>>? Criteria { get; }

    // List of related navigations to Include()
    List<Expression<Func<T, object>>> Includes { get; }

    // Optional ORDER BY
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }

    // Optional paging
    int? Skip { get; }
    int? Take { get; }

    // Whether to disable tracking
    bool AsNoTracking { get; }
}