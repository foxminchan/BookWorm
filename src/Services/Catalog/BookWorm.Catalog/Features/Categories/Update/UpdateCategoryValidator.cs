using BookWorm.Constants.Core;

namespace BookWorm.Catalog.Features.Categories.Update;

public sealed class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name).NotEmpty().MaximumLength(DataSchemaLength.Medium);
    }
}
