using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWorm.Rating.Infrastructure.EntityConfigurations;

internal sealed class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Rating).IsRequired();

        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(DataSchemaLength.Medium);

        builder.Property(e => e.LastName).IsRequired().HasMaxLength(DataSchemaLength.Medium);

        builder.Property(e => e.Comment).HasMaxLength(DataSchemaLength.Max);
    }
}
