using Catalyst.Common.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Catalyst.Common.Middleware;

public sealed class RequestResponseLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestResponseLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, IDateTimeProvider dateTimeProvider)
    {
        var startTime = dateTimeProvider.UtcNow;

        logger.LogInformation("Request: {Method} {Path}", context.Request.Method, context.Request.Path);

        await next(context);

        var endTime = dateTimeProvider.UtcNow;

        var diff = endTime - startTime;

        logger.LogInformation("Response: {StatusCode} in {ElapsedMilliseconds}ms",
            context.Response.StatusCode, diff.Milliseconds);
    }
}