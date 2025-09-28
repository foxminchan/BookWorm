using BookWorm.Chat.Domain.AggregatesModel;
using BookWorm.SharedKernel.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWorm.Chat.Infrastructure.EntityConfigurations;

internal sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(p => p.Id).HasDefaultValueSql(UniqueIdentifierHelper.NewUuidV7);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(DataSchemaLength.Large);

        builder.Property(e => e.UserId).IsRequired(false);

        builder.Property(e => e.RowVersion).IsRowVersion();

        builder.OwnsMany(x => x.Messages, e => e.ToJson());

        builder.Navigation(e => e.Messages).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
