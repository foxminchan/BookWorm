namespace BookWorm.SharedKernel.Specification.Builders;

public interface IIncludeSpecificationBuilder<T, out TProperty> : ISpecificationBuilder<T>
    where T : class;
