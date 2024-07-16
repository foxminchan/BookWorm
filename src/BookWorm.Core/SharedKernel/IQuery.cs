using MediatR;

namespace BookWorm.Core.SharedKernel;

public interface IQuery<out TResponse> : IRequest<TResponse>;
