namespace BookWorm.Chassis.Specification.Builders;

internal class SpecificationBuilder<T>(Specification<T> specification) : ISpecificationBuilder<T>
    where T : class
{
    public Specification<T> Specification { get; } = specification;
}
