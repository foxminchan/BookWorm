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
    public CategoryValidator(IBookRepository repository)
    {
        RuleFor(x => x)
            .MustAsync(
                async (categoryId, cancellationToken) =>
                    !await repository.AnyAsync(
                        new BookFilterSpec([categoryId], null),
                        cancellationToken
                    )
            )
            .WithMessage("Category has books and cannot be deleted");
    }
}
