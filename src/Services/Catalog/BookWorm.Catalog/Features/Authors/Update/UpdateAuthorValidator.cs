using BookWorm.Constants.Core;

namespace BookWorm.Catalog.Features.Authors.Update;

internal sealed class UpdateAuthorValidator : AbstractValidator<UpdateAuthorCommand>
{
    public UpdateAuthorValidator()
    {
        RuleFor(x => x.Id).Must(id => (Guid)id != Guid.Empty);

        RuleFor(x => x.Name).NotEmpty().MaximumLength(DataSchemaLength.Large);
    }
}
