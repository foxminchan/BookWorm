using BookWorm.Chat.Domain.AggregatesModel;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.Chat.Infrastructure;

public sealed class ChatDbContext(DbContextOptions<ChatDbContext> options)
    : DbContext(options),
        IUnitOfWork
{
    public DbSet<Conversation> Conversations => Set<Conversation>();

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        await SaveChangesAsync(cancellationToken);
        return true;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChatDbContext).Assembly);
    }
}
