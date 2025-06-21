using System.Data.Common;
using System.Net;
using System.Net.Sockets;
using Catalyst.Common.Errors;
using Catalyst.Common.Errors.Definitions;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using ExceptionalError = Catalyst.Common.Errors.Definitions.ExceptionalError;

namespace Catalyst.Core.MediatR;

public interface IMediatrContext
{
    Task<Result<TResponse>> Send<TRequest, TResponse>(TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<Result<TResponse>>;
}

public class MediatrContext(IMediator mediator, ILogger<MediatrContext> logger) : IMediatrContext
{
    public async Task<Result<TResponse>> Send<TRequest, TResponse>(TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<Result<TResponse>> =>
        await SendWithLogging<TRequest, TResponse>(request, cancellationToken);

    private async Task<Result<TResponse>> SendWithLogging<TRequest, TResponse>(TRequest request,
        CancellationToken cancellationToken)
        where TRequest : IRequest<Result<TResponse>>
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var requestName = typeof(TRequest).Name;
        using (LogContext.PushProperty("RequestType", requestName))
        {
            try
            {
                return await mediator.Send(request, cancellationToken);
            }
            catch (DbException ex)
            {
                logger.LogError(ex, "Database error executing {RequestType}", requestName);
                return Result.Fail<TResponse>(new DatabaseError(ex.Message));
            }
            catch (HttpRequestException ex)
            {
                var errorType = MapHttpStatusCodeToErrorType(ex.StatusCode);
                logger.LogError(ex, "HTTP error executing {RequestType}", requestName);
                return Result.Fail<TResponse>(new HttpError(ex.Message, errorType, ex.StatusCode));
            }
            catch (SocketException ex)
            {
                logger.LogError(ex, "Network error executing {RequestType}", requestName);
                return Result.Fail<TResponse>(new NetworkError(ex.Message));
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken)
            {
                logger.LogError(ex, "User cancelled {RequestType}", requestName);
                return Result.Fail<TResponse>(new OperationCanceledError(ex.Message));
            }
            catch (TaskCanceledException ex)
            {
                logger.LogError(ex, "Timeout executing {RequestType}", requestName);
                return Result.Fail<TResponse>(new TimeoutError(ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled error executing {RequestType}", requestName);
                return Result.Fail<TResponse>(new ExceptionalError(ex.Message, ex));
            }
        }
    }

    private static ErrorType MapHttpStatusCodeToErrorType(HttpStatusCode? statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.NotFound => CommonErrorTypes.NotFound,
            HttpStatusCode.Unauthorized => CommonErrorTypes.InvalidCredentials,
            HttpStatusCode.Forbidden => CommonErrorTypes.AccessDenied,
            HttpStatusCode.Conflict => CommonErrorTypes.ResourceConflict,
            HttpStatusCode.RequestTimeout => CommonErrorTypes.RequestTimeout,
            HttpStatusCode.BadRequest => CommonErrorTypes.InvalidInput,
            _ => CommonErrorTypes.Internal.HttpRequestFailed
        };
    }
}