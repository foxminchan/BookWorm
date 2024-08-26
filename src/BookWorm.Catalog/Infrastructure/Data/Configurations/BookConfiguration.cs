using BookWorm.Catalog.Domain.BookAggregate;

namespace BookWorm.Catalog.Infrastructure.Data.Configurations;

internal sealed class BookConfiguration : BaseConfiguration<Book>
{
    public override void Configure(EntityTypeBuilder<Book> builder)
    {
        base.Configure(builder);

        builder.Property(p => p.Name)
            .HasMaxLength(DataSchemaLength.Medium)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(DataSchemaLength.SuperLarge)
            .IsRequired();

        builder.OwnsOne(
            p => p.Price,
            e => e.ToJson()
        );

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(DataSchemaLength.SuperLarge);

        builder.Property(p => p.AverageRating)
            .HasDefaultValue(0.0);

        builder.Property(p => p.TotalReviews)
            .HasDefaultValue(0);

        builder.Property(p => p.Embedding)
            .HasColumnType(VectorType.Type);

        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Publisher)
            .WithMany()
            .HasForeignKey(x => x.PublisherId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.BookAuthors)
            .WithOne(x => x.Book);

        builder.Navigation(e => e.Category)
            .AutoInclude();

        builder.Navigation(e => e.Publisher)
            .AutoInclude();
    }
}
