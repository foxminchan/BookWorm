namespace BookWorm.Chassis.Specification.Builders;

internal abstract class SpecificationBuilder<T>(Specification<T> specification)
    : ISpecificationBuilder<T>
    where T : class
{
    public Specification<T> Specification { get; } = specification;
}
