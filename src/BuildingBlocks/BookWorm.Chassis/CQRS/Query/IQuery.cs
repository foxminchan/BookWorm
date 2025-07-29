using MediatR;

namespace BookWorm.Chassis.CQRS.Query;

public interface IQuery<out TResponse> : IRequest<TResponse>;
