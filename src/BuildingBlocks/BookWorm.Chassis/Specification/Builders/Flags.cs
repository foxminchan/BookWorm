namespace BookWorm.Chassis.Specification.Builders;

public static partial class SpecificationBuilderExtensions
{
    extension<T>(ISpecificationBuilder<T> builder)
        where T : class
    {
        public ISpecificationBuilder<T> AsNoTracking()
        {
            builder.Specification.AsNoTracking = true;
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
