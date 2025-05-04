using MediatR;

namespace BookWorm.Chassis.Command;

public interface ICommandHandler<in TCommand> : ICommandHandler<TCommand, Unit>
    where TCommand : ICommand;

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>;
