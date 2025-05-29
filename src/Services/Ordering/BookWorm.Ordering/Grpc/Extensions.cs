using BookWorm.Constants.Aspire;
using BasketGrpcServiceClient = BookWorm.Basket.Grpc.Services.BasketGrpcService.BasketGrpcServiceClient;
using BookGrpcServiceClient = BookWorm.Catalog.Grpc.Services.BookGrpcService.BookGrpcServiceClient;

namespace BookWorm.Ordering.Grpc;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static void AddGrpcServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        var isHttps = builder.Configuration["DOTNET_LAUNCH_PROFILE"] == "https";

        services.AddGrpc();

        services.AddGrpcServiceReference<BookGrpcServiceClient>(
            $"{(isHttps ? "https" : "http")}://{Application.Catalog}",
            HealthStatus.Degraded
        );
        services.AddSingleton<IBookService, BookService>();

        services
            .AddGrpcServiceReference<BasketGrpcServiceClient>(
                $"{(isHttps ? "https" : "http")}://{Application.Basket}",
                HealthStatus.Degraded
            )
            .AddAuthToken();
        services.AddSingleton<IBasketService, BasketService>();

        services.AddScoped<BasketMetadata>();
    }
}
