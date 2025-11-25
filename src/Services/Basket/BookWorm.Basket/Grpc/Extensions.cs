using BookWorm.Catalog.Grpc.Services;
using BookWorm.Chassis.Utilities;
using BookWorm.Chassis.Utilities.Configuration;

namespace BookWorm.Basket.Grpc;

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

        services.AddGrpcHealthChecks();

        services.AddGrpcServiceReference<BookGrpcService.BookGrpcServiceClient>(
            HttpUtilities
                .AsUrlBuilder()
                .WithScheme(builder.GetScheme())
                .WithHost(Constants.Aspire.Services.Catalog)
                .Build(),
            HealthStatus.Degraded
        );

        services.AddSingleton<IBookService, BookService>();
    }
}
