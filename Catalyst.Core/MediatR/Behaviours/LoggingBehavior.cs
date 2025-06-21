using System.Diagnostics;
using System.Text.Json;
using Catalyst.Common.Services;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Catalyst.Core.MediatR.Behaviours;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    ICorrelationIdProvider correlationIdProvider
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IContext
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling request");
        logger.LogDebug("Request details: {RequestData}", JsonSerializer.Serialize(request));

        var stopwatch = Stopwatch.StartNew();
        TResponse response;
        try
        {
            response = await next();
            stopwatch.Stop();

            logger.LogInformation("Request handled in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);

            string responseJson;
            if (response is IResultBase resultResponse && !resultResponse.IsSuccess) // Changed to IResultBase
            {
                responseJson = JsonSerializer.Serialize(new
                {
                    IsSuccess = false,
                    Errors = resultResponse.Errors.Select(e => e.Message).ToList()
                });
            }
            else
            {
                // Serialize successful result (or non-FluentResult response) as before
                responseJson = JsonSerializer.Serialize(response);
            }

            logger.LogDebug("Response details: {ResponseData}", responseJson);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Request failed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            correlationIdProvider.CorrelationId = null;
        }

        return response;
    }
}