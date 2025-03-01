namespace BookWorm.SharedKernel.Specification.Builders;

public interface ISpecificationBuilder<T>
    where T : class
{
    Specification<T> Specification { get; }
}
