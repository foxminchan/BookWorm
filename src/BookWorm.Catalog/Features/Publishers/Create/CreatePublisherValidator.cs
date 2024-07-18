using BookWorm.Shared.Constants;
using FluentValidation;

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
