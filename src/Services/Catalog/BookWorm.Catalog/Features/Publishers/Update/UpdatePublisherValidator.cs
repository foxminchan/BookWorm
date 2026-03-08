using BookWorm.Constants.Core;

namespace BookWorm.Catalog.Features.Publishers.Update;

internal sealed class UpdatePublisherValidator : AbstractValidator<UpdatePublisherCommand>
{
    public UpdatePublisherValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name).NotEmpty().MaximumLength(DataSchemaLength.Medium);
    }
}
