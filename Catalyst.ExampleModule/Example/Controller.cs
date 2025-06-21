using Catalyst.Common.Models;
using Catalyst.Common.Services;
using Catalyst.Core;
using Catalyst.Core.MediatR;
using Catalyst.ExampleModule.Example.Application.Features.ReadAll;
using Catalyst.ExampleModule.Example.Application.Features.ReadById;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReadAllRequest = Catalyst.ExampleModule.Example.Application.Features.ReadAll.Request;
using ReadByIdRequest = Catalyst.ExampleModule.Example.Application.Features.ReadById.Request;
using GetFilterOptionsRequest = Catalyst.ExampleModule.Example.Application.Features.GetFilterOptions.Request;
using GetFilterOptionsResponse = Catalyst.ExampleModule.Example.Application.Features.GetFilterOptions.Response;


namespace Catalyst.ExampleModule.Example;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ExampleModuleController : BaseController
{
    public ExampleModuleController(IMediatrContext mediatrContext, ICorrelationIdProvider correlationIdProvider)
        : base(mediatrContext, correlationIdProvider)
    {
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginationResponse<Result>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllExampleEntitys(
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int[]? typeIds = null,
        [FromQuery] int[]? generalStatus = null
    )
    {
        var request = new ReadAllRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            Filters = new ReadAllRequest.ExampleEntityFilters
            {
                TypeIds = typeIds,
                GeneralStatus = generalStatus?.Select(b => b.ToString()).ToArray(),
            }
        };
        // The TResponse in HandleRequestAsync should be the outer PaginationResponse<Data>
        return await HandleRequestAsync<ReadAllRequest, PaginationResponse<Result>>(request);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)] // Use the specific Response type
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExampleEntityById(int id)
    {
        var request = new ReadByIdRequest
        {
            Id = id,
        };

        return await HandleRequestAsync<ReadByIdRequest, Response>(request);
    }


    [HttpGet("filter-options")]
    [ProducesResponseType(typeof(GetFilterOptionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetExampleEntityFilterOptions()
    {
        var request = new GetFilterOptionsRequest();

        return await HandleRequestAsync<GetFilterOptionsRequest, GetFilterOptionsResponse>(request);
    }
}