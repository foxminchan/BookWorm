using System.Diagnostics;
using BookWorm.SharedKernel.Mediator;

namespace BookWorm.Rating.Infrastructure;

public sealed class RatingDbContext : DbContext, IUnitOfWork
{
    private readonly IPublisher _publisher;

    public RatingDbContext(DbContextOptions<RatingDbContext> options, IPublisher publisher)
        : base(options)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        Debug.WriteLine($"RatingDbContext::ctor -> {GetHashCode()}");
    }

    public DbSet<Feedback> Feedbacks => Set<Feedback>();

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        await _publisher.DispatchDomainEventsAsync(this);
        await SaveChangesAsync(cancellationToken);
        return true;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Feedback>().ToContainer(nameof(Feedbacks));
    }
}
