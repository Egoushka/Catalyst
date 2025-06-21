using Catalyst.Common.Services;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Catalyst.Core.MediatR.Behaviours;

public sealed class CommandContextBehavior<TRequest, TResponse>(
    IHttpContextAccessor httpContextAccessor,
    IDateTimeProvider dateTimeProvider,
    ICorrelationIdProvider correlationIdProvider)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IContext, IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserId))
        {
            // TODO: Check if this is the correct way for us
            request.UserId = httpContextAccessor.HttpContext?.User.Identity?.Name;
        }

        if (request.Timestamp == default)
        {
            request.Timestamp = dateTimeProvider.UtcNow;
        }

        request.CorrelationId = correlationIdProvider.CorrelationId!.Value;

        return await next();
    }
}