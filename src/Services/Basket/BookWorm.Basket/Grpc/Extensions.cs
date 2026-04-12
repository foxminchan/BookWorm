using BookWorm.Catalog.Grpc.Services;
using BookWorm.Chassis.Utilities;
using BookWorm.Chassis.Utilities.Configurations;

namespace BookWorm.Basket.Grpc;

internal static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddGrpcServices()
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
}
