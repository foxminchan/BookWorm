using BookWorm.Constants.Core;

namespace BookWorm.Catalog.Features.Categories.Update;

internal sealed class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Id).Must(id => (Guid)id != Guid.Empty);

        RuleFor(x => x.Name).NotEmpty().MaximumLength(DataSchemaLength.Medium);
    }
}
