using BookWorm.Catalog.Grpc.Services;
using BookWorm.ServiceDefaults.Configuration;

namespace BookWorm.Basket.Grpc;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static void AddGrpcServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddGrpc();

        services.AddGrpcHealthChecks();

        services.AddGrpcServiceReference<BookGrpcService.BookGrpcServiceClient>(
            $"{builder.GetScheme()}://{Application.Catalog}",
            HealthStatus.Degraded
        );

        services.AddSingleton<IBookService, BookService>();
    }
}
