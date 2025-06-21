using System.Linq.Expressions;
using Catalyst.Data.Abstraction;

namespace Catalyst.Data.Specifications;

public sealed class AnonymousSpecification<T> : BaseSpecification<T>
{
    public AnonymousSpecification(Expression<Func<T, bool>> criteria)
    {
        ApplyCriteria(criteria);
    }
}