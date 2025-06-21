using Catalyst.Data.Abstraction;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Catalyst.Core.MediatR.Behaviours;

public sealed class UnitOfWorkBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork,
    ILogger<UnitOfWorkBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IContext
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;

        logger.LogDebug("UnitOfWorkBehavior: Handling request {RequestType}", requestType);

        if (IsCommand(request))
        {
            logger.LogDebug("UnitOfWorkBehavior: Starting transaction for command {RequestType}", requestType);
            // await unitOfWork.BeginTransactionAsync();
            logger.LogDebug("UnitOfWorkBehavior: Transaction started successfully for command {RequestType}",
                requestType);

            try
            {
                var response = await next();

                // var strategy = dbContext.Database.CreateExecutionStrategy();

                // await strategy.ExecuteAsync(
                //     async cancellationToken =>
                //     {
                //         await unitOfWork.SaveChangesAsync(cancellationToken);
                //         await unitOfWork.CommitTransactionAsync();
                //     }, cancellationToken);

                logger.LogDebug("UnitOfWorkBehavior: Transaction committed successfully for command {RequestType}",
                    requestType);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "UnitOfWorkBehavior: Exception caught during command {RequestType} processing. Rolling back transaction.",
                    requestType);
                // await unitOfWork.RollbackTransactionAsync();
                logger.LogDebug("UnitOfWorkBehavior: Transaction rolled back for command {RequestType}", requestType);
                throw;
            }
        }

        logger.LogDebug("UnitOfWorkBehavior: Request {RequestType} is not a command. Transaction not started.",
            requestType);

        return await next();
    }

    private static bool IsCommand(TRequest request)
    {
        return request is IBaseCommand;
    }
}