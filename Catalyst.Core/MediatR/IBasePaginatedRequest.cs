using Catalyst.Common.Models;

namespace Catalyst.Core.MediatR;

public interface IBasePaginationRequest<out TResponse> : IBaseRequest<TResponse>, IPagination;