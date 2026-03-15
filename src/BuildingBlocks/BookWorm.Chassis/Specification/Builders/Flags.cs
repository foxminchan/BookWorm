namespace BookWorm.Chassis.Specification.Builders;

public static partial class SpecificationBuilderExtensions
{
    extension<T>(ISpecificationBuilder<T> builder)
        where T : class
    {
        public ISpecificationBuilder<T> AsNoTracking()
        {
            builder.Specification.AsNoTracking = true;
            builder.Specification.AsTracking = false;
            return builder;
        }

        public ISpecificationBuilder<T> AsTracking()
        {
            builder.Specification.AsTracking = true;
            builder.Specification.AsNoTracking = false;
            return builder;
        }

        public ISpecificationBuilder<T> AsSplitQuery()
        {
            builder.Specification.AsSplitQuery = true;
            return builder;
        }

        public ISpecificationBuilder<T> IgnoreQueryFilters()
        {
            builder.Specification.IgnoreQueryFilters = true;
            return builder;
        }
    }
}
