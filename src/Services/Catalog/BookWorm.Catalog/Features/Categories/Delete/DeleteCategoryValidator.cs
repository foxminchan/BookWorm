using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;

namespace BookWorm.Catalog.Features.Categories.Delete;

internal sealed class DeleteCategoryValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryValidator(CategoryValidator categoryValidator)
    {
        RuleFor(x => x.Id).SetValidator(categoryValidator);
    }
}

internal sealed class CategoryValidator : AbstractValidator<Guid>
{
    private readonly IBookRepository _repository;

    public CategoryValidator(IBookRepository repository)
    {
        _repository = repository;

        RuleFor(x => x)
            .MustAsync(IsNotCategoryBook)
            .WithMessage("Category has books and cannot be deleted");
    }

    private async Task<bool> IsNotCategoryBook(Guid categoryId, CancellationToken cancellationToken)
    {
        var category = await _repository.ListAsync(
            new BookFilterSpec([categoryId], null),
            cancellationToken
        );

        return category.Count == 0;
    }
}
