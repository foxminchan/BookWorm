using BasketGrpcServiceClient = BookWorm.Basket.Grpc.Services.BasketGrpcService.BasketGrpcServiceClient;
using BookGrpcServiceClient = BookWorm.Catalog.Grpc.Services.BookGrpcService.BookGrpcServiceClient;

namespace BookWorm.Ordering.Grpc;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static void AddGrpcServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        var scheme =
            builder.Configuration["DOTNET_LAUNCH_PROFILE"] == Protocol.Https
                ? Protocol.Https
                : Protocol.Http;

        services.AddGrpc();

        services.AddGrpcServiceReference<BookGrpcServiceClient>(
            $"{scheme}://{Application.Catalog}",
            HealthStatus.Degraded
        );
        services.AddSingleton<IBookService, BookService>();

        services
            .AddGrpcServiceReference<BasketGrpcServiceClient>(
                $"{scheme}://{Application.Basket}",
                HealthStatus.Degraded
            )
            .AddAuthToken();
        services.AddSingleton<IBasketService, BasketService>();

        services.AddScoped<BasketMetadata>();
    }
}
