using System.Linq.Expressions;
using LinqKit;

namespace Catalyst.Data.Abstraction;

public abstract class BaseSpecification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>>? Criteria { get; protected set; }
    public List<Expression<Func<T, object>>> Includes { get; } = [];
    public Expression<Func<T, object>>? OrderBy { get; protected set; }
    public Expression<Func<T, object>>? OrderByDescending { get; protected set; }
    public int? Skip { get; protected set; }
    public int? Take { get; protected set; }
    public bool AsNoTracking { get; protected set; } = true;

    /// <summary>
    /// Applies the criteria to the specification. If criteria already exists, it's combined using AND logic.
    /// Requires LinqKit. Use this when adding REQUIRED filters.
    /// </summary>
    protected virtual void ApplyCriteria(Expression<Func<T, bool>> criteria)
    {
        // Ensure LinqKit's And extension method is used for proper composition
        Criteria = Criteria == null ? criteria : Criteria.And(criteria.Expand()); // Expand the new criteria
        // Also expand the existing criteria if needed, though setting it should handle it.
        // A safer way might be to always expand before assignment if combining:
        // if (Criteria == null) Criteria = criteria;
        // else Criteria = Criteria.And(criteria.Expand()); // Assuming Criteria is already expandable
    }

    /// <summary>
    /// Applies the criteria to the specification using OR logic with any existing criteria.
    /// Requires LinqKit. Use this for optional filters where any can match.
    /// </summary>
    protected virtual void ApplyOrCriteria(Expression<Func<T, bool>> criteria)
    {
        // Ensure LinqKit's Or extension method is used
        Criteria = Criteria == null ? criteria : Criteria.Or(criteria.Expand());
    }

    // AddInclude, ApplyOrderBy, ApplyOrderByDescending, ApplyPaging, Disable/EnableTracking remain the same...
    protected void AddInclude(Expression<Func<T, object>> includeExpression) =>
        Includes.Add(includeExpression);

    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression) =>
        OrderBy = orderByExpression;

    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression) =>
        OrderByDescending = orderByDescExpression;

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }

    protected void DisableTracking() => AsNoTracking = true;
    protected void EnableTracking() => AsNoTracking = false;
}