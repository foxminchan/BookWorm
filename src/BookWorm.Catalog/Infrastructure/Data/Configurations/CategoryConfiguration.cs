using BookWorm.Catalog.Domain;

namespace BookWorm.Catalog.Infrastructure.Data.Configurations;

internal sealed class CategoryConfiguration : BaseConfiguration<Category>
{
    public override void Configure(EntityTypeBuilder<Category> builder)
    {
        base.Configure(builder);

        builder.Property(p => p.Name)
            .HasMaxLength(DataSchemaLength.Medium)
            .IsRequired();

        builder.HasIndex(e => e.Name)
            .IsUnique();

        builder.HasData(GetSampleData());
    }

    private static IEnumerable<Category> GetSampleData()
    {
        yield return new("Technology");
        yield return new("Personal Development");
        yield return new("Business");
        yield return new("Science");
        yield return new("Psychology");
        yield return new("Light Novel");
    }
}
