using Catalyst.Common.Models;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Catalyst.Core.MediatR;

public abstract class BaseHandler<TRequest, TResponse> : IRequestHandler<TRequest, Result<TResponse>>
    where TRequest : BaseActionModel<TResponse>
{
    protected readonly ILogger<BaseHandler<TRequest, TResponse>> Logger;

    protected BaseHandler(ILogger<BaseHandler<TRequest, TResponse>> logger)
    {
        Logger = logger;
    }

    public virtual async Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken)
    {
        Result<TResponse> operationResult;

        try
        {
            operationResult = await ExecuteHandlerLogic(request, cancellationToken);

            if (operationResult is { IsSuccess: true, Value: BaseResponse response })
            {
                response.CorrelationId = request.CorrelationId;
                LogSuccess(request);
            }
            else
            {
                LogFailure(request, operationResult);
            }
        }
        catch (Exception ex)
        {
            operationResult = HandleException(request, ex);
        }

        return operationResult;
    }

    public abstract Task<Result<TResponse>> ExecuteHandlerLogic(
        TRequest request,
        CancellationToken cancellationToken);

    private void LogSuccess(TRequest request)
    {
        Logger.LogInformation("{RequestName} executed successfully.",
            typeof(TRequest).Name);
    }

    private void LogFailure(TRequest request, Result<TResponse> operationResult)
    {
        Logger.LogError("{RequestName} failed. Errors: {Errors}",
            typeof(TRequest).Name,
            string.Join(", ", operationResult.Errors.Select(e => e.Message)));
    }

    private Result<TResponse> HandleException(TRequest request, Exception ex)
    {
        Logger.LogError(ex, "General Exception during {RequestName}",
            typeof(TRequest).Name);
        return Result.Fail<TResponse>(
            new ExceptionalError($"An unexpected error occurred during {typeof(TRequest).Name}", ex));
    }
}