using BookWorm.Catalog.Infrastructure.Data.CompiledModels;
using EntityFramework.Exceptions.PostgreSQL;

namespace BookWorm.Catalog.Infrastructure.Data;

public static class Extension
{
    public static IHostApplicationBuilder AddPersistence(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMigration<CatalogContext, CatalogContextSeed>();

        builder.AddNpgsqlDbContext<CatalogContext>(ServiceName.Database.Catalog, configureDbContextOptions:
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
