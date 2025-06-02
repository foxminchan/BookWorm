namespace BookWorm.Ordering.Features.Buyers.Create;

public sealed class CreateBuyerValidator : AbstractValidator<CreateBuyerCommand>
{
    public CreateBuyerValidator()
    {
        RuleFor(x => x.Street).NotEmpty().MaximumLength(DataSchemaLength.Medium);

        RuleFor(x => x.City).NotEmpty().MaximumLength(DataSchemaLength.Medium);

        RuleFor(x => x.Province).NotEmpty().MaximumLength(DataSchemaLength.Medium);
    }
}
