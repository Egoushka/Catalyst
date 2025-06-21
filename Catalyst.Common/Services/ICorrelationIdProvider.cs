namespace Catalyst.Common.Services;

public interface ICorrelationIdProvider
{
    Guid? CorrelationId { get; set; }
}

public class CorrelationIdProvider : ICorrelationIdProvider
{
    public Guid? CorrelationId { get; set; }
}