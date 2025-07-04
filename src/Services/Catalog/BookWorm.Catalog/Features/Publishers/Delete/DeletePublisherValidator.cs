using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;

namespace BookWorm.Catalog.Features.Publishers.Delete;

public sealed class DeletePublisherValidator : AbstractValidator<DeletePublisherCommand>
{
    public DeletePublisherValidator(PublisherValidator publisherValidator)
    {
        RuleFor(x => x.Id).SetValidator(publisherValidator);
    }
}

public sealed class PublisherValidator : AbstractValidator<Guid>
{
    private readonly IBookRepository _repository;

    public PublisherValidator(IBookRepository repository)
    {
        _repository = repository;

        RuleFor(x => x)
            .MustAsync(IsNotPublisherBook)
            .WithMessage("Publisher has books and cannot be deleted");
    }

    private async Task<bool> IsNotPublisherBook(
        Guid publisherId,
        CancellationToken cancellationToken
    )
    {
        var publisher = await _repository.ListAsync(
            new BookFilterSpec(null, [publisherId]),
            cancellationToken
        );

        return publisher.Count == 0;
    }
}
