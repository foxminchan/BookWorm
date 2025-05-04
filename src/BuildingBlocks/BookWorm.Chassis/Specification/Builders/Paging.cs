namespace BookWorm.Chassis.Specification.Builders;

public static partial class SpecificationBuilderExtensions
{
    public static ISpecificationBuilder<T> Skip<T>(this ISpecificationBuilder<T> builder, int skip)
        where T : class
    {
        builder.Specification.Skip = skip;

        return builder;
    }

    public static ISpecificationBuilder<T> Take<T>(this ISpecificationBuilder<T> builder, int take)
        where T : class
    {
        builder.Specification.Take = take;

        return builder;
    }
}
