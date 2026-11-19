using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;

namespace BookWorm.Catalog.Features.Publishers.Delete;

internal sealed class DeletePublisherValidator : AbstractValidator<DeletePublisherCommand>
{
    public DeletePublisherValidator(PublisherValidator publisherValidator)
    {
        RuleFor(x => x.Id).SetValidator(publisherValidator);
    }
}

internal sealed class PublisherValidator : AbstractValidator<Guid>
{
    public PublisherValidator(IBookRepository repository)
    {
        RuleFor(x => x)
            .MustAsync(
                async (publisherId, cancellationToken) =>
                    await repository.CountAsync(
                        new BookFilterSpec(null, [publisherId]),
                        cancellationToken
                    ) == 0
            )
            .WithMessage("Publisher has books and cannot be deleted");
    }
}
