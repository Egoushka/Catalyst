using Catalyst.Common.Models;
using Catalyst.Data.Abstraction;
using Catalyst.ExampleModule.Example.Application.Common;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Catalyst.ExampleModule.Example.Application.Features.ReadAll;

public sealed class Handler : BaseHandler<Request, PaginationResponse<Result>>
{
    private readonly IRepository<Domain.Models.ExampleEntity> _dashboardRepository;

    public Handler(ILogger<Handler> logger, IRepository<Domain.Models.ExampleEntity> dashboardRepository)
        : base(logger)
    {
        _dashboardRepository = dashboardRepository;
    }

    public override async Task<Result<PaginationResponse<Result>>> ExecuteHandlerLogic(Request request,
        CancellationToken cancellationToken)
    {
        try
        {
            var countSpec = new Spec(request.SearchTerm, request.Filters);
            var totalCount = await _dashboardRepository.CountAsync(countSpec, cancellationToken);

            if (totalCount == 0)
            {
                return FluentResults.Result.Ok(new PaginationResponse<Result>
                {
                    Data = [],
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalCount = 0
                    // TotalPages will be calculated by PaginationResponse getter or should be set here
                });
            }


            var pagedSpec = new Spec(request.PageNumber, request.PageSize, request.SearchTerm, request.Filters);
            var dashboards = await _dashboardRepository.ListAsync(pagedSpec, cancellationToken);

            var mappedResults = dashboards.Select(Result.FromEntity);

            var response = new PaginationResponse<Result>
            {
                Data = mappedResults.ToList(),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
                // TotalPages will be calculated by PaginationResponse getter or should be set here
            };

            return FluentResults.Result.Ok(response);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error retrieving dashboards for request {@Request}", request);
            return FluentResults.Result.Fail<PaginationResponse<Result>>(
                $"An error occurred while retrieving dashboards: {e.Message}");
        }
    }
}