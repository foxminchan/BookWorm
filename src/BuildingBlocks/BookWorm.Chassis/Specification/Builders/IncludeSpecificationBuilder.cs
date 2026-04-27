namespace BookWorm.Chassis.Specification.Builders;

internal sealed class IncludeSpecificationBuilder<T, TProperty>(Specification<T> specification)
    : SpecificationBuilder<T>(specification),
        IIncludeSpecificationBuilder<T, TProperty>
    where T : class;
