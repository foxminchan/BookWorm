using BookWorm.Catalog.Infrastructure.Data.CompiledModels;
using BookWorm.Core.SharedKernel;
using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.Catalog.Infrastructure.Data;

public static class Extension
{
    public static IHostApplicationBuilder AddPersistence(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMigration<CatalogContext>();

        builder.AddNpgsqlDbContext<CatalogContext>("catalogdb", configureDbContextOptions:
            dbContextOptionsBuilder =>
            {
                dbContextOptionsBuilder
                    .UseNpgsql(optionsBuilder =>
                    {
                        optionsBuilder.UseVector();
                        optionsBuilder.MigrationsAssembly(typeof(CatalogContext).Assembly.FullName);
                        optionsBuilder.EnableRetryOnFailure(15, TimeSpan.FromSeconds(30), null);
                    })
                    .UseModel(CatalogContextModel.Instance)
                    .UseExceptionProcessor()
                    .UseSnakeCaseNamingConvention();
            });

        builder.Services.AddScoped(typeof(IReadRepository<>), typeof(CatalogRepository<>));
        builder.Services.AddScoped(typeof(IRepository<>), typeof(CatalogRepository<>));

        return builder;
    }
}
