namespace BookWorm.Chat.Features.Stream;

public sealed class ChatStreamEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapHub<ChatStreamHub>("/stream", o => o.AllowStatefulReconnects = true)
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization()
            .RequirePerUserRateLimit();
    }
}
