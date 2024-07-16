using BookWorm.Catalog.Domain.BookAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWorm.Catalog.Infrastructure.Data.Configurations;

public sealed class BookConfiguration : BaseConfiguration<Book>
{
    public override void Configure(EntityTypeBuilder<Book> builder)
    {
        base.Configure(builder);

        builder.Property(p => p.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.OwnsOne(
            p => p.Price,
            e => e.ToJson()
        );

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(p => p.AverageRating)
            .HasDefaultValue(0.0);

        builder.Property(p => p.TotalReviews)
            .HasDefaultValue(0);

        builder.Property(p => p.Embedding)
            .HasColumnType("vector(384)");

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
