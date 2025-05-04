using MediatR;

namespace BookWorm.Chassis.Query;

public interface IQuery<out TResponse> : IRequest<TResponse>;
