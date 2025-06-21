using MediatR;

namespace Catalyst.Core.MediatR;

public interface IBaseCommand<out TResponse> : IRequest<TResponse>, IBaseCommand;

public interface IBaseCommand : IRequest;