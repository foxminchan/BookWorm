namespace BookWorm.SharedKernel.Specification.Evaluators;

public interface IEvaluator
{
    IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification)
        where T : class;
}
