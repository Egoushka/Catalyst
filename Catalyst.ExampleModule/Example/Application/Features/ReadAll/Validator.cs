using Catalyst.Common.Models;
using Catalyst.ExampleModule.Example.Application.Common;

namespace Catalyst.ExampleModule.Example.Application.Features.ReadAll;

public sealed class Validator : BaseValidator<Request, PaginationResponse<Result>>;