namespace BookWorm.Chassis.Specification.Builders;

internal class IncludeSpecificationBuilder<T, TProperty>(Specification<T> specification)
    : SpecificationBuilder<T>(specification),
        IIncludeSpecificationBuilder<T, TProperty>
    where T : class;
