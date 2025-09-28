using BookWorm.Constants.Core;
using BookWorm.SharedKernel.Helpers;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWorm.Catalog.Infrastructure.EntityConfigurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(p => p.Id).HasDefaultValueSql(UniqueIdentifierHelper.NewUuidV7);

        builder.Property(p => p.Name).HasMaxLength(DataSchemaLength.Medium).IsRequired();

        builder.HasIndex(e => e.Name).IsUnique();
    }
}
