using System.Diagnostics;
using BookWorm.SharedKernel.Mediator;

namespace BookWorm.Catalog.Infrastructure;

public sealed class CatalogDbContext : DbContext, IUnitOfWork
{
    private readonly IPublisher _publisher;

    public CatalogDbContext(DbContextOptions<CatalogDbContext> options, IPublisher publisher)
        : base(options)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        Debug.WriteLine($"CatalogDbContext::ctor -> {GetHashCode()}");
    }

    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Publisher> Publishers => Set<Publisher>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookAuthor> BookAuthors => Set<BookAuthor>();

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        await _publisher.DispatchDomainEventsAsync(this);
        await SaveChangesAsync(cancellationToken);
        return true;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
    }
}
