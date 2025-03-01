using ISpecification = BookWorm.SharedKernel.Specification.ISpecification<BookWorm.Ordering.Domain.AggregatesModel.BuyerAggregate.Buyer>;

namespace BookWorm.Ordering.Domain.AggregatesModel.BuyerAggregate;

public interface IBuyerRepository : IRepository<Buyer>
{
    Task<Buyer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Buyer> AddAsync(Buyer buyer, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Buyer>> ListAsync(
        ISpecification spec,
        CancellationToken cancellationToken = default
    );

    Task<long> CountAsync(CancellationToken cancellationToken = default);

    void Delete(Buyer buyer);
}
