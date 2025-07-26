using BookWorm.Catalog.Grpc.Services;

namespace BookWorm.Ordering.Grpc;

[ExcludeFromCodeCoverage]
internal static class Extensions
{
    public static void AddGrpcServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddGrpc(options =>
        {
            options.EnableDetailedErrors = true;
            options.Interceptors.Add<GrpcExceptionInterceptor>();
        });

        services.AddGrpcServiceReference<BookGrpcService.BookGrpcServiceClient>(
            $"{builder.GetScheme()}://{Constants.Aspire.Services.Catalog}",
            HealthStatus.Degraded
        );

        services.AddSingleton<IBookService, BookService>();

        services
            .AddGrpcServiceReference<BasketGrpcService.BasketGrpcServiceClient>(
                $"{builder.GetScheme()}://{Constants.Aspire.Services.Basket}",
                HealthStatus.Degraded
            )
            .AddAuthToken();

        services.AddSingleton<IBasketService, BasketService>();

        services.AddScoped<BasketMetadata>();
    }
}
