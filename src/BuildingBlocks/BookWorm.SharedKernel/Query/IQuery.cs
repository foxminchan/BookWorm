using MediatR;

namespace BookWorm.SharedKernel.Query;

public interface IQuery<out TResponse> : IRequest<TResponse>;
