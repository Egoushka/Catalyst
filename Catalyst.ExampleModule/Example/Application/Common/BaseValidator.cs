using FluentValidation;

namespace Catalyst.ExampleModule.Example.Application.Common;

public abstract class BaseValidator<TRequest, TResponse> : AbstractValidator<TRequest>
    where TRequest : BaseAction<TResponse>;