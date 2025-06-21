using Catalyst.Core.MediatR;
using Catalyst.ExampleModule.Example.Application.Common;
using FluentResults;

namespace Catalyst.ExampleModule.Example.Application.Features.ReadById;

public sealed record Request : BaseAction<Response>, IBaseRequest<Result<Response>>
{
    public required int Id { get; init; }
}