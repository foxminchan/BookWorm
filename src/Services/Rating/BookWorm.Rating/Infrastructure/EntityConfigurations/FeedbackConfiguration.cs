using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWorm.Rating.Infrastructure.EntityConfigurations;

internal sealed class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.ToContainer(Components.Container.Feedbacks);

        builder.HasNoDiscriminator();

        builder.HasKey(e => e.Id);

        builder.HasPartitionKey(e => e.BookId);

        builder.Property(e => e.Id).ToJsonProperty("id");

        builder.Property(e => e.BookId).ToJsonProperty("bookId");

        builder.Property(e => e.FirstName).ToJsonProperty("firstName");

        builder.Property(e => e.LastName).ToJsonProperty("lastName");

        builder.Property(e => e.Comment).ToJsonProperty("comment");

        builder.Property(e => e.Rating).ToJsonProperty("rating");

        builder.Property(e => e.CreatedAt).ToJsonProperty("createdAt");

        builder.Property(e => e.LastModifiedAt).ToJsonProperty("lastModifiedAt");

        builder.Ignore(e => e.RowVersion);

        builder.UseETagConcurrency();
    }
}
