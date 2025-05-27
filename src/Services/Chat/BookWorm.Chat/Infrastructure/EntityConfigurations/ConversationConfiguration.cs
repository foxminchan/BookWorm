using BookWorm.Chat.Domain.AggregatesModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWorm.Chat.Infrastructure.EntityConfigurations;

internal sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(DataSchemaLength.Large);

        builder.Property(e => e.UserId).IsRequired(false);

        builder.OwnsMany(x => x.Messages, e => e.ToJson());

        builder.Navigation(e => e.Messages).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
