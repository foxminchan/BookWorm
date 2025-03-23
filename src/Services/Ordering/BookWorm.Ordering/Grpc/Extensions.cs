using BasketGrpcServiceClient = BookWorm.Basket.Grpc.Services.BasketGrpcService.BasketGrpcServiceClient;
using BookGrpcServiceClient = BookWorm.Catalog.Grpc.Services.BookGrpcService.BookGrpcServiceClient;

namespace BookWorm.Ordering.Grpc;

public static class Extensions
{
    public static void AddGrpcServices(this IServiceCollection services)
    {
        services.AddGrpc();

        services
            .AddGrpcClient<BookGrpcServiceClient>(o =>
                o.Address = new("http+https://bookworm-catalog")
            )
            .AddStandardResilienceHandler();
        services.AddSingleton<IBookService, BookService>();

        services
            .AddGrpcClient<BasketGrpcServiceClient>(o =>
                o.Address = new("http+https://bookworm-basket")
            )
            .AddAuthToken()
            .AddStandardResilienceHandler();
        services.AddSingleton<IBasketService, BasketService>();

        services.AddScoped<BasketMetadata>();
    }
}
