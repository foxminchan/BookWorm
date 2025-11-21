using BookWorm.Catalog.Grpc.Services;
using BookWorm.Chassis.Security.TokenExchange;
using BookWorm.Chassis.Utilities;
using BookWorm.Chassis.Utilities.Configuration;

namespace BookWorm.Ordering.Grpc;

[ExcludeFromCodeCoverage]
internal static class Extensions
{
    public static void AddGrpcServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddGrpc(options =>
        {
            options.EnableDetailedErrors = builder.Environment.IsDevelopment();
            options.Interceptors.Add<GrpcExceptionInterceptor>();
        });

        services.AddGrpcServiceReference<BookGrpcService.BookGrpcServiceClient>(
            HttpUtilities
                .AsUrlBuilder()
                .WithScheme(builder.GetScheme())
                .WithHost(Constants.Aspire.Services.Catalog)
                .Build(),
            HealthStatus.Degraded
        );

        services.AddSingleton<IBookService, BookService>();

        services
            .AddGrpcServiceReference<BasketGrpcService.BasketGrpcServiceClient>(
                HttpUtilities
                    .AsUrlBuilder()
                    .WithScheme(builder.GetScheme())
                    .WithHost(Constants.Aspire.Services.Basket)
                    .Build(),
                HealthStatus.Degraded
            )
            .AddAuthTokenExchange(Constants.Aspire.Services.Basket);

        services.AddSingleton<IBasketService, BasketService>();

        services.AddScoped<BasketMetadata>();
    }
}
