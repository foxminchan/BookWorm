using MediatR;

namespace BookWorm.Core.SharedKernel;

public interface ICommand<out TResponse> : IRequest<TResponse>, ITxRequest;
