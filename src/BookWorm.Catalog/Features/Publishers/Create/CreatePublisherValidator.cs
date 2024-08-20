namespace BookWorm.Catalog.Features.Publishers.Create;

internal sealed class CreatePublisherValidator : AbstractValidator<CreatePublisherCommand>
{
    public CreatePublisherValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(DataSchemaLength.Large);
    }
}
