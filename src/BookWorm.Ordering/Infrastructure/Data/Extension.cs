using BookWorm.Core.SharedKernel;
using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.Ordering.Infrastructure.Data;

public static class Extension
{
    public static IHostApplicationBuilder AddPersistence(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMigration<OrderContext>();

        builder.AddNpgsqlDbContext<OrderContext>("orderingdb", configureDbContextOptions:
            dbContextOptionsBuilder =>
            {
                dbContextOptionsBuilder
                    .UseNpgsql(optionsBuilder =>
                    {
                        optionsBuilder.MigrationsAssembly(typeof(OrderContext).Assembly.FullName);
                        optionsBuilder.EnableRetryOnFailure(15, TimeSpan.FromSeconds(30), null);
                    })
                    .UseExceptionProcessor()
                    .UseSnakeCaseNamingConvention();
            });

        builder.Services.AddScoped(typeof(IReadRepository<>), typeof(OrderRepository<>));
        builder.Services.AddScoped(typeof(IRepository<>), typeof(OrderRepository<>));

        return builder;
    }
}
