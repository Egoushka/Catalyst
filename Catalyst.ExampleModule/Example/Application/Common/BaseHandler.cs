using Microsoft.Extensions.Logging;

namespace Catalyst.ExampleModule.Example.Application.Common;

public abstract class BaseHandler<TCommand, TResponse> : Core.MediatR.BaseHandler<TCommand, TResponse>
    where TCommand : BaseAction<TResponse>
{
    protected BaseHandler(ILogger<BaseHandler<TCommand, TResponse>> logger) : base(logger)
    {
    }
}