using BookWorm.Catalog.Grpc.Services;
using BookWorm.Chassis.Security.TokenExchange;
using BookWorm.Chassis.Utilities.Configuration;

namespace BookWorm.Ordering.Grpc;

[ExcludeFromCodeCoverage]
internal static class Extensions
{
    public static void AddGrpcServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services
            .AddGrpc(options =>
            {
                options.EnableDetailedErrors = builder.Environment.IsDevelopment();
                options.Interceptors.Add<GrpcExceptionInterceptor>();
            })
            .AddJsonTranscoding(o => o.JsonSettings.WriteIndented = true);

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
            .AddAuthTokenExchange(Constants.Aspire.Services.Basket);

        services.AddSingleton<IBasketService, BasketService>();

        services.AddScoped<BasketMetadata>();
    }
}
