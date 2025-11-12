namespace BookWorm.Chassis.Specification.Builders;

public static partial class SpecificationBuilderExtensions
{
    extension<T>(ISpecificationBuilder<T> builder)
        where T : class
    {
        public ISpecificationBuilder<T> Skip(int skip)
        {
            builder.Specification.Skip = skip;

            return builder;
        }

        public ISpecificationBuilder<T> Take(int take)
        {
            builder.Specification.Take = take;

            return builder;
        }
    }
}
