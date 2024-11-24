using BookWorm.Ordering.Domain.BuyerAggregate;

namespace BookWorm.Ordering.Features.Buyers.Create;

public sealed record CreateBuyerCommand(string? Street, string? City, string? Province)
    : ICommand<Result<Guid>>;

public sealed class CreateBuyerHandler(
    IRepository<Buyer> repository,
    IIdentityService identityService
) : ICommandHandler<CreateBuyerCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateBuyerCommand request,
        CancellationToken cancellationToken
    )
    {
        var buyerId = identityService.GetUserIdentity();

        Guard.Against.NullOrEmpty(buyerId);

        var fullName = identityService.GetFullName();

        Guard.Against.Null(fullName);

        var address = new Address(request.Street, request.City, request.Province);

        var buyer = new Buyer(Guid.Parse(buyerId), fullName, address);

        var result = await repository.AddAsync(buyer, cancellationToken);

        return result.Id;
    }
}
