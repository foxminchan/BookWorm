using BookWorm.Catalog.Grpc.Services;
using BookWorm.Chassis.Utilities;
using BookWorm.Chassis.Utilities.Configurations;

namespace BookWorm.McpTools.Grpc;

internal static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddGrpcServices()
        {
            var services = builder.Services;

            services.AddGrpcServiceReference<BookGrpcService.BookGrpcServiceClient>(
                HttpUtilities
                    .AsUrlBuilder()
                    .WithScheme(builder.GetScheme())
                    .WithHost(Constants.Aspire.Services.Catalog)
                    .Build(),
                HealthStatus.Degraded
            );

            services.AddScoped<IBookService, BookService>();
        }
    }
}
