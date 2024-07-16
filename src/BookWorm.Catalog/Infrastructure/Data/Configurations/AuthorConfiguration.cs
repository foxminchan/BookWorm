using BookWorm.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWorm.Catalog.Infrastructure.Data.Configurations;

public sealed class AuthorConfiguration : BaseConfiguration<Author>
{
    public override void Configure(EntityTypeBuilder<Author> builder)
    {
        base.Configure(builder);

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasMany(x => x.BookAuthors)
            .WithOne(x => x.Author)
            .HasForeignKey(x => x.AuthorId);

        builder.Navigation(x => x.BookAuthors)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasData(GetSampleData());
    }

    private static IEnumerable<Author> GetSampleData()
    {
        yield return new("Martin Fowler");
        yield return new("Eric Evans");
        yield return new("Robert C. Martin");
        yield return new("Kent Beck");
        yield return new("Don Box");
        yield return new("Joshua Bloch");
    }
}
