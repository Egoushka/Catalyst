using Catalyst.Core.MediatR;
using Catalyst.ExampleModule.Example.Application.Common;
using FluentResults;

namespace Catalyst.ExampleModule.Example.Application.Features.GetFilterOptions;

public sealed record Request : BaseAction<Response>, IBaseRequest<Result<Response>>;