using Catalyst.Core.MediatR;

namespace Catalyst.ExampleModule.Example.Application.Common;

public abstract record BaseAction<TResponse> : BaseActionModel<TResponse>;