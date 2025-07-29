using MediatR;

namespace BookWorm.Chassis.CQRS.Command;

public interface ICommand : ICommand<Unit>;

public interface ICommand<out TResponse> : IRequest<TResponse>;
