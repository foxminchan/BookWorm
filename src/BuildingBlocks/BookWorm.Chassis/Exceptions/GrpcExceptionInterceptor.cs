using Grpc.Core;
using Grpc.Core.Interceptors;

namespace BookWorm.Chassis.Exceptions;

public sealed class GrpcExceptionInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation
    )
    {
        try
        {
            return await continuation(request, context);
        }
        catch (Exception exception)
        {
            throw new RpcException(new(StatusCode.Internal, exception.Message));
        }
    }
}
