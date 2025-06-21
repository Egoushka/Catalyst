using MediatR;

namespace Catalyst.Core.MediatR;

public interface IBaseRequest<out TResponse> : IRequest<TResponse>, IBaseRequest;

public interface IBaseRequest : IRequest;