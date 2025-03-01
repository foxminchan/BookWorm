using MediatR;

namespace BookWorm.SharedKernel.Command;

public interface ICommand : ICommand<Unit>;

public interface ICommand<out TResponse> : IRequest<TResponse>;
