using Catalyst.Common.Models;
using Catalyst.Core.MediatR;
using Catalyst.ExampleModule.Example.Application.Common;
using FluentResults;

namespace Catalyst.ExampleModule.Example.Application.Features.ReadAll;

public sealed record Request : BaseAction<PaginationResponse<Result>>,
    IBasePaginationRequest<Result<PaginationResponse<Result>>>
{
    public string? SearchTerm { get; init; }
    public ExampleEntityFilters Filters { get; init; } = new();
    public required int PageNumber { get; init; }
    public required int PageSize { get; init; }

    public sealed class ExampleEntityFilters
    {
        public int[]? TypeIds { get; init; }
        public string[]? GeneralStatus { get; init; }
    }
}