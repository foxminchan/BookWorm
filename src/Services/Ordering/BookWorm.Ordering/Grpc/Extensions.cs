using BasketGrpcServiceClient = BookWorm.Basket.Grpc.Services.BasketGrpcService.BasketGrpcServiceClient;
using BookGrpcServiceClient = BookWorm.Catalog.Grpc.Services.BookGrpcService.BookGrpcServiceClient;

namespace BookWorm.Ordering.Grpc;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static void AddGrpcServices(this IServiceCollection services)
    {
        services.AddGrpc();

        services.AddGrpcServiceReference<BookGrpcServiceClient>(
            $"https://{Application.Catalog}",
            HealthStatus.Degraded
        );
        services.AddSingleton<IBookService, BookService>();

        services
            .AddGrpcServiceReference<BasketGrpcServiceClient>(
                $"https://{Application.Basket}",
                HealthStatus.Degraded
            )
            .AddAuthToken();
        services.AddSingleton<IBasketService, BasketService>();

        services.AddScoped<BasketMetadata>();
    }
}
