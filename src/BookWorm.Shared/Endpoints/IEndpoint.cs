namespace BookWorm.Shared.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}

public interface IEndpoint<TResult> : IEndpoint
{
    Task<TResult> HandleAsync();
}

public interface IEndpoint<TResult, in TRequest> : IEndpoint
{
    Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}

public interface IEndpoint<TResult, in TRequest1, in TRequest2> : IEndpoint
{
    Task<TResult> HandleAsync(
        TRequest1 request1,
        TRequest2 request2,
        CancellationToken cancellationToken = default
    );
}

public interface IEndpoint<TResult, in TRequest1, in TRequest2, in TRequest3> : IEndpoint
{
    Task<TResult> HandleAsync(
        TRequest1 request1,
        TRequest2 request2,
        TRequest3 request3,
        CancellationToken cancellationToken = default
    );
}
