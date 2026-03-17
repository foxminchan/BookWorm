using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.Exceptions;

public sealed class GrpcExceptionInterceptor(ILogger<GrpcExceptionInterceptor> logger) : Interceptor
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
            logger.LogError(
                exception,
                "[{Interceptor}] Unhandled exception processing gRPC request",
                nameof(GrpcExceptionInterceptor)
            );
            throw new RpcException(new(StatusCode.Internal, "An internal error occurred."));
        }
    }
}
