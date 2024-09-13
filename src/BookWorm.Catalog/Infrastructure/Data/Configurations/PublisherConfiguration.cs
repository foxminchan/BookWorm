using BookWorm.Catalog.Domain;

namespace BookWorm.Catalog.Infrastructure.Data.Configurations;

internal sealed class PublisherConfiguration : BaseConfiguration<Publisher>
{
    public override void Configure(EntityTypeBuilder<Publisher> builder)
    {
        base.Configure(builder);

        builder.Property(p => p.Name)
            .HasMaxLength(DataSchemaLength.Large)
            .IsRequired();

        builder.HasIndex(e => e.Name)
            .IsUnique();
    }
}
