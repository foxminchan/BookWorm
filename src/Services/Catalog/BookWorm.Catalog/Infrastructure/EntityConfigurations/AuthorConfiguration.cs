using BookWorm.Constants.Core;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWorm.Catalog.Infrastructure.EntityConfigurations;

internal sealed class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(p => p.Name).HasMaxLength(DataSchemaLength.Large).IsRequired();
    }
}
