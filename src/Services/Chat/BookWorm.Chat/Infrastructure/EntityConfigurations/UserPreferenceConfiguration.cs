using BookWorm.Chat.Memory.Semantic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWorm.Chat.Infrastructure.EntityConfigurations;

internal sealed class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> entity)
    {
        entity.ToTable("user_preferences");

        entity.HasKey(p => p.Id);

        entity.Property(p => p.UserId).HasMaxLength(DataSchemaLength.ExtraLarge).IsRequired();

        entity.Property(p => p.Kind).HasConversion<string>().HasMaxLength(DataSchemaLength.Small);

        entity
            .Property(p => p.Value)
            .HasMaxLength(PreferenceConsolidator.MaxValueLength)
            .IsRequired();

        entity
            .Property(p => p.NormalizedValue)
            .HasMaxLength(PreferenceConsolidator.MaxValueLength)
            .IsRequired();

        entity
            .Property(p => p.Sentiment)
            .HasConversion<string>()
            .HasMaxLength(DataSchemaLength.Small);

        entity
            .HasIndex(p => new
            {
                p.UserId,
                p.Kind,
                p.NormalizedValue,
            })
            .IsUnique();
    }
}
