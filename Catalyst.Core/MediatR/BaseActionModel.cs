using System.Text.Json.Serialization;
using FluentResults;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;

namespace Catalyst.Core.MediatR;

public abstract record BaseActionModel<TResponse> : IRequest<Result<TResponse>>, IContext
{
    [SwaggerIgnore]
    [JsonIgnore]
    public string? UserId { get; set; }

    [SwaggerIgnore]
    [JsonIgnore]
    public DateTimeOffset Timestamp { get; set; }

    [SwaggerIgnore]
    [JsonIgnore]
    public Guid CorrelationId { get; set; }
}