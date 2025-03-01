namespace BookWorm.Catalog.Features.Publishers.Update;

public sealed class UpdatePublisherValidator : AbstractValidator<UpdatePublisherCommand>
{
    public UpdatePublisherValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name).NotEmpty().MaximumLength(DataSchemaLength.Medium);
    }
}
