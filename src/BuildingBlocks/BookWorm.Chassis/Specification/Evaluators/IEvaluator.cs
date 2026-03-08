namespace BookWorm.Chassis.Specification.Evaluators;

internal interface IEvaluator
{
    IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification)
        where T : class;
}
