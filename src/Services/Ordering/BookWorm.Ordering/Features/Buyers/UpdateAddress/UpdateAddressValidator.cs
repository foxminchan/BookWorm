using BookWorm.Constants.Core;

namespace BookWorm.Ordering.Features.Buyers.UpdateAddress;

public sealed class UpdateAddressValidator : AbstractValidator<UpdateAddressCommand>
{
    public UpdateAddressValidator()
    {
        RuleFor(x => x.Street).NotEmpty().MaximumLength(DataSchemaLength.Medium);

        RuleFor(x => x.City).NotEmpty().MaximumLength(DataSchemaLength.Medium);

        RuleFor(x => x.Province).NotEmpty().MaximumLength(DataSchemaLength.Medium);
    }
}
