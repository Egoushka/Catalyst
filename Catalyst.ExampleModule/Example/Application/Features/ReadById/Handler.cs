using Catalyst.Common.Errors.Definitions;
using Catalyst.ExampleModule.Example.Application.Common;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Catalyst.ExampleModule.Example.Application.Features.ReadById;

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
            var dashboard = await _dbContext.ExampleEntities
                .Include(x => x.Type)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);

            if (dashboard is null)
                return Result.Fail<Response>(new NotFoundError($"ExampleEntity {request.Id}"));

            var result = Response.Result.FromEntity(dashboard);

            var response = new Response
            {
                Data = result
            };
            return Result.Ok(response);
        }
        catch (Exception e)
        {
            return Result.Fail<Response>(e.Message);
        }
    }
}