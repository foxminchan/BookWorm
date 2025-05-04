namespace BookWorm.Chassis.Specification.Builders;

public interface IIncludeSpecificationBuilder<T, out TProperty> : ISpecificationBuilder<T>
    where T : class;
