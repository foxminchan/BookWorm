using BookWorm.Shared.Constants;

namespace BookWorm.Catalog.Features.Publishers.Create;

public sealed class CreatePublisherValidator : AbstractValidator<CreatePublisherCommand>
{
    public CreatePublisherValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(DataSchemaLength.Large);
    }
}
