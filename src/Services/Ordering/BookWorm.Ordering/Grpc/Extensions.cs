using BookWorm.Catalog.Grpc.Services;

namespace BookWorm.Ordering.Grpc;

[ExcludeFromCodeCoverage]
internal static class Extensions
{
    public static void AddGrpcServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddGrpc(options => options.EnableDetailedErrors = true);

        services.AddGrpcServiceReference<BookGrpcService.BookGrpcServiceClient>(
            $"{builder.GetScheme()}://{Application.Catalog}",
            HealthStatus.Degraded
        );

        services.AddSingleton<IBookService, BookService>();

        services
            .AddGrpcServiceReference<BasketGrpcService.BasketGrpcServiceClient>(
                $"{builder.GetScheme()}://{Application.Basket}",
                HealthStatus.Degraded
            )
            .AddAuthToken();

        services.AddSingleton<IBasketService, BasketService>();

        services.AddScoped<BasketMetadata>();
    }
}
