using BookWorm.Catalog.Grpc.Services;
using BookWorm.ServiceDefaults.Configuration;

namespace BookWorm.Basket.Grpc;

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

        services.AddGrpcHealthChecks();

        services.AddGrpcServiceReference<BookGrpcService.BookGrpcServiceClient>(
            $"{builder.GetScheme()}://{Application.Catalog}",
            HealthStatus.Degraded
        );

        services.AddSingleton<IBookService, BookService>();
    }
}
