namespace Catalyst.Core.MediatR;

public interface IContext
{
    string? UserId { get; set; }
    DateTimeOffset Timestamp { get; set; }
    Guid CorrelationId { get; set; }
}