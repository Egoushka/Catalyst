namespace Catalyst.Common.Services;

public interface ICorrelationIdGenerator
{
    Guid Generate();
}

public sealed class GuidCorrelationIdGenerator : ICorrelationIdGenerator
{
    public Guid Generate() => Guid.NewGuid();
}