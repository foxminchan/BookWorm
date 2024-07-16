using BookWorm.Catalog.Domain;
using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.Catalog.Infrastructure.Data;

public sealed class CatalogContext(DbContextOptions<CatalogContext> options) : DbContext(options)
{
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Publisher> Publishers => Set<Publisher>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookAuthor> BookAuthors => Set<BookAuthor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresExtension(UniqueType.Extension);
        modelBuilder.HasPostgresExtension(VectorType.Extension);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogContext).Assembly);
    }
}
