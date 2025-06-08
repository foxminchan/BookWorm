using BookWorm.ServiceDefaults.Configuration;
using BasketGrpcServiceClient = BookWorm.Basket.Grpc.Services.BasketGrpcService.BasketGrpcServiceClient;
using BookGrpcServiceClient = BookWorm.Catalog.Grpc.Services.BookGrpcService.BookGrpcServiceClient;

namespace BookWorm.Ordering.Grpc;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static void AddGrpcServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddGrpc();

        services.AddGrpcServiceReference<BookGrpcServiceClient>(
            $"{builder.GetScheme()}://{Application.Catalog}",
            HealthStatus.Degraded
        );

        services.AddSingleton<IBookService, BookService>();

        services
            .AddGrpcServiceReference<BasketGrpcServiceClient>(
                $"{builder.GetScheme()}://{Application.Basket}",
                HealthStatus.Degraded
            )
            .AddAuthToken();

        services.AddSingleton<IBasketService, BasketService>();

        services.AddScoped<BasketMetadata>();
    }
}
