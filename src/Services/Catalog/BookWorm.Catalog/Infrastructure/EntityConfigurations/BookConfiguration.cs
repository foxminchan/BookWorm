using BookWorm.Constants.Core;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWorm.Catalog.Infrastructure.EntityConfigurations;

internal sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(p => p.Name).HasMaxLength(DataSchemaLength.Medium).IsRequired();

        builder.Property(p => p.Description).HasMaxLength(DataSchemaLength.Max).IsRequired();

        builder.Property(x => x.Image).HasMaxLength(DataSchemaLength.Large);

        builder.OwnsOne(
            p => p.Price,
            e =>
            {
                e.Property(p => p.DiscountPrice).HasPrecision(18, 2);
                e.Property(p => p.OriginalPrice).HasPrecision(18, 2).IsRequired();
            }
        );

        builder.Property(p => p.AverageRating).HasDefaultValue(0.0);

        builder.Property(p => p.CreatedAt).HasDefaultValue(DateTime.UtcNow);

        builder.Property(p => p.LastModifiedAt).HasDefaultValue(DateTime.UtcNow);

        builder.Property(p => p.Version).IsConcurrencyToken();

        builder
            .HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.Publisher)
            .WithMany()
            .HasForeignKey(x => x.PublisherId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.BookAuthors).WithOne(x => x.Book).OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(e => e.Category).AutoInclude();

        builder.Navigation(e => e.Publisher).AutoInclude();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
