using BookWorm.Catalog.Domain;
using BookWorm.Shared.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWorm.Catalog.Infrastructure.Data.Configurations;

public sealed class PublisherConfiguration : BaseConfiguration<Publisher>
{
    public override void Configure(EntityTypeBuilder<Publisher> builder)
    {
        base.Configure(builder);

        builder.Property(p => p.Name)
            .HasMaxLength(DataSchemaLength.Large)
            .IsRequired();

        builder.HasIndex(e => e.Name)
            .IsUnique();

        builder.HasData(GetSampleData());
    }

    private static IEnumerable<Publisher> GetSampleData()
    {
        yield return new("O'Reilly Media");
        yield return new("Manning Publications");
        yield return new("Packt Publishing");
        yield return new("Apress");
        yield return new("No Starch Press");
        yield return new("Pragmatic Bookshelf");
    }
}
