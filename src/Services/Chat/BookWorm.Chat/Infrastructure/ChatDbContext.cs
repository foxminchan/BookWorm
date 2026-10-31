using BookWorm.Chat.Infrastructure.EntityConfigurations;
using BookWorm.Chat.Memory.Semantic;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.Chat.Infrastructure;

/// <summary>
///     Persistence for BookAgent's semantic (preference) memory in the <c>chatdb</c> database.
/// </summary>
internal sealed class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContext(options)
{
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserPreferenceConfiguration());
    }
}
