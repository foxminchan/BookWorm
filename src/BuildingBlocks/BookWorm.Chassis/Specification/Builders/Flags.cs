namespace BookWorm.Chassis.Specification.Builders;

public static partial class SpecificationBuilderExtensions
{
    public static ISpecificationBuilder<T> AsNoTracking<T>(this ISpecificationBuilder<T> builder)
        where T : class
    {
        builder.Specification.AsNoTracking = true;
        return builder;
    }

    public static ISpecificationBuilder<T> AsSplitQuery<T>(this ISpecificationBuilder<T> builder)
        where T : class
    {
        builder.Specification.AsSplitQuery = true;
        return builder;
    }

    public static ISpecificationBuilder<T> IgnoreQueryFilters<T>(
        this ISpecificationBuilder<T> builder
    )
        where T : class
    {
        builder.Specification.IgnoreQueryFilters = true;
        return builder;
    }
}
