using MediatR;

namespace BookWorm.Chassis.Command;

public interface ICommand : ICommand<Unit>;

public interface ICommand<out TResponse> : IRequest<TResponse>;
