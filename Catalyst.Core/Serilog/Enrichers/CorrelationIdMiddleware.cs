using Catalyst.Common.Services;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Catalyst.Core.Serilog.Enrichers;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ICorrelationIdProvider correlationIdProvider)
    {
        using (LogContext.PushProperty("CorrelationId", correlationIdProvider.CorrelationId))
        {
            await next(context);
        }
    }
}