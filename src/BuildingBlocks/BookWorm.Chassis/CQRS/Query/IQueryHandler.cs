using MediatR;

namespace BookWorm.Chassis.CQRS.Query;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>;
