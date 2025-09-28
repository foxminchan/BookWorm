using BookWorm.Constants.Core;
using BookWorm.Notification.Domain.Models;
using BookWorm.SharedKernel.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWorm.Notification.Infrastructure.EntityConfigurations;

internal sealed class OutboxConfiguration : IEntityTypeConfiguration<Outbox>
{
    public void Configure(EntityTypeBuilder<Outbox> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(p => p.Id).HasDefaultValueSql(UniqueIdentifierHelper.NewUuidV7);

        builder.Property(p => p.ToName).HasMaxLength(DataSchemaLength.Large).IsRequired();

        builder.Property(p => p.ToEmail).HasMaxLength(DataSchemaLength.ExtraLarge).IsRequired();

        builder.Property(p => p.Subject).HasMaxLength(DataSchemaLength.Large).IsRequired();

        builder.Property(p => p.Body).HasMaxLength(DataSchemaLength.MaxText).IsRequired();

        builder.Property(p => p.SequenceNumber).ValueGeneratedOnAdd();

        builder.HasIndex(e => e.IsSent);
    }
}
