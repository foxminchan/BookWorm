using BookWorm.Core.SharedKernel;
using BookWorm.Ordering.Infrastructure.Data.CompiledModels;
using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.Ordering.Infrastructure.Data;

public static class Extension
{
    public static IHostApplicationBuilder AddPersistence(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMigration<OrderingContext>();

        builder.AddNpgsqlDbContext<OrderingContext>("orderingdb", configureDbContextOptions:
            dbContextOptionsBuilder =>
            {
                dbContextOptionsBuilder
                    .UseNpgsql(optionsBuilder =>
                    {
                        optionsBuilder.MigrationsAssembly(typeof(OrderingContext).Assembly.FullName);
                        optionsBuilder.EnableRetryOnFailure(15, TimeSpan.FromSeconds(30), null);
                    })
                    .UseModel(OrderingContextModel.Instance)
                    .UseExceptionProcessor()
                    .UseSnakeCaseNamingConvention();
            });

        builder.Services.AddScoped(typeof(IReadRepository<>), typeof(OrderingRepository<>));
        builder.Services.AddScoped(typeof(IRepository<>), typeof(OrderingRepository<>));

        return builder;
    }
}
