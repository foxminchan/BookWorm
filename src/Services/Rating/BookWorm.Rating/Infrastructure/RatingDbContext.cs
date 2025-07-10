namespace BookWorm.Rating.Infrastructure;

public sealed class RatingDbContext(DbContextOptions<RatingDbContext> options)
    : DbContext(options),
        IUnitOfWork
{
    public DbSet<Feedback> Feedbacks => Set<Feedback>();

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        await SaveChangesAsync(cancellationToken);
        return true;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RatingDbContext).Assembly);
    }
}
