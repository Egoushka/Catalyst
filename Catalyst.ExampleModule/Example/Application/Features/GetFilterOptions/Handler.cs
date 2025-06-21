using Catalyst.ExampleModule.Example.Application.Common;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Catalyst.ExampleModule.Example.Application.Features.GetFilterOptions;

public sealed class Handler : BaseHandler<Request, Response>
{
    private readonly ExampleModuleDbContext _dbContext;

    public Handler(ILogger<Handler> logger, ExampleModuleDbContext dbContext)
        : base(logger)
    {
        _dbContext = dbContext;
    }

    public override async Task<Result<Response>> ExecuteHandlerLogic(Request request,
        CancellationToken cancellationToken)
    {
        try
        {
            var types = await _dbContext.Types
                .Select(t => new FilterOptionDto { Value = t.Id, Label = t.Name })
                .Distinct()
                .OrderBy(f => f.Label)
                .ToListAsync(cancellationToken);
            
            var filterOptionsData = new Result
            {
                Types = types,
            };

            return FluentResults.Result.Ok(new Response { Data = filterOptionsData });
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error retrieving dashboard filter options.");
            return FluentResults.Result.Fail<Response>(
                $"An error occurred while retrieving filter options: {e.Message}");
        }
    }
}