using System.Net;
using Microsoft.Azure.Cosmos;

namespace BookWorm.Rating.Infrastructure;

internal sealed class RatingDbContext(DbContextOptions<RatingDbContext> options)
    : DbContext(options),
        IUnitOfWork
{
    public DbSet<Feedback> Feedbacks => Set<Feedback>();

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException(
                "The feedback was modified or deleted by another request.",
                ex
            );
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is CosmosException { StatusCode: HttpStatusCode.Conflict })
        {
            throw new InvalidOperationException(
                "A feedback from this author already exists for this book.",
                ex
            );
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.RegisterAllInRatingApiMarker();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RatingDbContext).Assembly);
    }
}
