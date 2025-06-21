namespace Catalyst.Data.Abstraction;

public abstract class BaseEntity : BaseEntity<int>;

public abstract class BaseEntity<T> where T : struct
{
    public T Id { get; }
}