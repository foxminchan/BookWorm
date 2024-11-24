namespace BookWorm.Catalog.Infrastructure.Data.Configurations;

internal abstract class BaseConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : EntityBase
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.CreatedDate).HasDefaultValue(DateTime.UtcNow);

        builder.Property(e => e.UpdateDate).HasDefaultValue(DateTime.UtcNow);

        builder.Property(e => e.Version).IsConcurrencyToken();
    }
}
